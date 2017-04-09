using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Starcounter;
using Colab.Common;
using Concepts.Ring8.Tunity;

namespace SignIn
{
    internal class MainHandlers
    {
        protected string AuthCookieName = "soauthtoken";
        protected int rememberMeDays = 30;

        public void Register()
        {
            Colab.Common.MainCommon.RegisterWithMobileSupport(false);
            //Colab.Common.MainCommon.Register(false);

            Application.Current.Use((Request req) =>
            {
                Cookie cookie = GetSignInCookie();

                if (cookie != null)
                {
                    if (Session.Current == null)
                    {
                        Session.Current = new Session(SessionOptions.PatchVersioning);
                    }

                    UserSession session = TunityUser.SignInUser(cookie.Value);

                    if (session != null)
                    {
                        RefreshAuthCookie(session);
                    }
                }

                return null;
            });

            #region mobile
            Handle.GET("/signin/mobile/user", () =>
            {
                Master m = (Master)Self.GET("/signin/mobile/master");
                m.Utils.Html = "/co-common/mobile-utils.html";
                if (!(m.Utils.PersistantApp is SignInPersistent))
                {
                    Db.Scope(() =>
                    {
                        var page = new SignInPersistent()
                        {
                            Html = "/SignIn/viewmodels/mobile-signin-persistent.html"
                        };
                        m.Utils.PersistantApp = page;
                        Cookie cookie = GetSignInCookie();
                        if (cookie != null)
                        {
                            var us = TunityUser.SignInUser(cookie.Value);
                            this.RefreshSignInState();
                            if (us != null)
                                RefreshAuthCookie(us);
                        }
                    });
                }
                return m.Utils;
            });

            Handle.GET("/signin/mobile/signinoutform", () =>
            {
                Master m = (Master)Self.GET("/signin/mobile/master");
                var p = m.GetApplication<SignInFormPage>();
                if (p == null)
                {
                    p = new SignInFormPage()
                    {
                        Html = "/signin/viewmodels/mobile-signin-form.html"
                    };
                }
                m.SetApplication(p);
                return p;
            });
            #endregion

            Handle.GET("/signin/user", () =>
            {
                Master m = (Master)Self.GET("/signin/master");
                if (!(m.Utils.PersistantApp is SignInPersistent))
                {
                    Db.Scope(() =>
                    {
                        var page = new SignInPersistent();
                        m.Utils.PersistantApp = page;
                        Cookie cookie = GetSignInCookie();
                        if (cookie != null)
                        {
                            var us = TunityUser.SignInUser(cookie.Value);
                            this.RefreshSignInState();
                            if (us != null)
                                RefreshAuthCookie(us);
                        }
                    });
                }
                return m.Utils;
            });


            Handle.POST("/signin/partial/signin", (Request request) =>
            {
                NameValueCollection values = HttpUtility.ParseQueryString(request.Body);
                string username = values["username"];
                string password = values["password"];
                string rememberMe = values["rememberMe"];

                if (HandleSignIn(username, password, rememberMe))
                {
                    Utils.MorphToOriginUri();
                }
                Session.Current.CalculatePatchAndPushOnWebSocket();

                return 200;
            }, new HandlerOptions() { SkipRequestFilters = true });

            Handle.GET("/signin/tunityuser/shortcuts/{?}", (Request request, String userid) =>
            {
                var menu = new Menu();
                var user = ColabDbHelper.FromIDString<TunityUser>(userid);
                if (user != null && Db.Equals(user, SessionData.Current.User))
                {
                    if (AccessHelper.IsHeadAdmin())
                    {
                        menu.Items.Add(new MenuItem()
                        {
                            Label = "Global settings",
                            Icon = "wrench",
                            Url = "/launcher/settings"
                        });
                    }
                    if (AccessHelper.IsSuperUser())
                    {
                        menu.Items.Add(new MenuItem()
                        {
                            Label = "Configurations",
                            Icon = "database",
                            Url = "/launcher/configs"
                        });

                    }
                }
                return menu;
            });

            Handle.GET("/signin/signinuser", (Request request) =>
            {
                Master m = (Master)Self.GET("/signin/mobile/master");
                var p = m.GetApplication<SignInFormPage>();
                if (p == null)
                {
                    p = new SignInFormPage()
                    {
                        SessionUri = Session.Current.SessionUri,
                        Title = ColabConfiguration.Get<String>(ColabConfig.TITLE)
                    };
                }
                p.ClearPassword++;
                Utils.SetOriginUri("/");
                m.SetApplication(p);
                return p;
            
            });

            Handle.GET("/signin/signinuser?{?}", (Request request, string pars) =>
            {
                Master m = (Master)Self.GET("/signin/mobile/master");
                var p = m.GetApplication<SignInFormPage>();
                if (p == null)
                {
                    p = new SignInFormPage()
                    {
                        SessionUri = Session.Current.SessionUri,
                        Title = ColabConfiguration.Get<String>(ColabConfig.TITLE)
                    };
                }
                if (pars != null)
                {
                    NameValueCollection values = HttpUtility.ParseQueryString(pars);
                    Utils.SetOriginUri(values["originurl"]);
                }
                p.ClearPassword++;
                p.RefreshSignInState();
                m.SetApplication(p);
                return p;

            });



            Handle.GET("/signin/partial/signout", HandleSignOut, new HandlerOptions() { SkipRequestFilters = true });

            UriMapping.Map("/signin/tunityuser/shortcuts/@w", UriMapping.MappingUriPrefix + "/tunityuser/shortcuts/@w");
            UriMapping.Map("/signin/signinuser", UriMapping.MappingUriPrefix + "/signin");
            UriMapping.Map("/signin/signinuser?@w", UriMapping.MappingUriPrefix + "/signin/@w");
            UriMapping.Map("/signin/user", UriMapping.MappingUriPrefix + "/user");
            UriMapping.Map("/signin/mobile/user", UriMapping.MappingUriPrefix + "/mobile/user");
        }

        protected void ClearAuthCookie()
        {
            this.SetAuthCookie(null, false);
        }

        protected void RefreshAuthCookie(UserSession session)
        {
            Cookie cookie = GetSignInCookie();

            if (cookie == null)
            {
                return;
            }

            Db.Transact(() =>
            {
                session.Token.Name = TunityUser.CreateAuthToken(session.Token.User.Name);
            });

            cookie.Value = session.Token.Name;
            if (session.Token.Expires == DateTime.MinValue)
                cookie.Expires = null;
            else
            {
                Db.Transact(() =>
                {
                    session.Token.Expires = DateTime.UtcNow.AddDays(rememberMeDays);
                    cookie.Expires = session.Token.Expires;
                });
            }

            Handle.AddOutgoingCookie(cookie.Name, cookie.GetFullValueString());
        }

        protected void SetAuthCookie(UserSession session, bool RememberMe)
        {
            Cookie cookie = new Cookie()
            {
                Name = AuthCookieName,
                Value = session?.Token?.Name
            };

            if (session == null || session.Token == null)
            {
                //to delete a cookie, explicitly use a date in the past
                cookie.Expires = DateTime.UtcNow.AddDays(-1);
            }
            else
            {
                //cookie with expiration date is persistent until that date
                //cookie without expiration date expires when the browser is closed
                Db.Transact(() =>
                {
                    if (RememberMe)
                    {
                        session.Token.Expires = DateTime.UtcNow.AddDays(rememberMeDays);
                        cookie.Expires = session.Token.Expires;
                    }
                    else
                    {
                        session.Token.Expires = DateTime.MinValue;
                        cookie.Expires = null;
                    }
                });
            }
           
            Handle.AddOutgoingCookie(cookie.Name, cookie.GetFullValueString());
        }


        protected void RefreshSignInState()
        {
            Utils.RefreshSignInState();
        }


        protected Boolean HandleSignIn(string Username, string Password, string RememberMe)
        {
            Username = Uri.UnescapeDataString(Username);

            UserSession session = TunityUser.SignInUser(Username, Password);

            if (session == null)
            {
                Utils.SetMessage("Invalid username or password!");
            }
            else
            {
                Utils.SetMessage("success");
                SetAuthCookie(session, RememberMe == "true");
            }

            RefreshSignInState();

            return session != null;
        }

      
        protected Response HandleSignOut()
        {
            TunityUser.SignOutUser();
            try
            {
                ClearAuthCookie();
            }
            catch
            { }

            RefreshSignInState();
            Master.SendCommand(ColabCommand.REREQUEST_URL);
            return Master.Current;
        }

        protected Cookie GetSignInCookie()
        {
            List<Cookie> cookies = Handle.IncomingRequest.Cookies.Select(x => new Cookie(x)).ToList();
            Cookie cookie = cookies.FirstOrDefault(x => x.Name == AuthCookieName);

            return cookie;
        }
    }
}
