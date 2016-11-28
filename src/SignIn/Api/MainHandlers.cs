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
                if (!(m.Utils.PersistantApp is SignInPage))
                {
                    Db.Scope(() =>
                    {
                        var page = new SignInPage()
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
                if (!(m.Utils.PersistantApp is SignInPage))
                {
                    Db.Scope(() =>
                    {
                        var page = new SignInPage();
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
                            Url = "/settings"
                        });
                    }
                    if (AccessHelper.IsSuperUser())
                    {
                        menu.Items.Add(new MenuItem()
                        {
                            Label = "Configurations",
                            Icon = "database",
                            Url = "/configs"
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
                    p = new SignInFormPage() { SessionUri = Session.Current.SessionUri };
                }
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
                    p = new SignInFormPage() { SessionUri = Session.Current.SessionUri };
                }
                if (pars != null)
                {
                    NameValueCollection values = HttpUtility.ParseQueryString(pars);
                    Utils.SetOriginUri(values["originurl"]);
                }
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
            this.SetAuthCookie("", false);
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
            Handle.AddOutgoingCookie(cookie.Name, cookie.GetFullValueString());
        }

        protected void SetAuthCookie(string token, bool RememberMe)
        {
            Cookie cookie = new Cookie()
            {
                Name = AuthCookieName,
                Value = token
            };

            if (token == "")
            {
                //to delete a cookie, explicitly use a date in the past
                cookie.Expires = DateTime.UtcNow.AddDays(-1);
            }
            else if (RememberMe)
            {
                //cookie with expiration date is persistent until that date
                //cookie without expiration date expires when the browser is closed
                cookie.Expires = DateTime.UtcNow.AddDays(rememberMeDays);
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
                SetAuthCookie(session.Token.Name, RememberMe == "true");
            }

            RefreshSignInState();

            return session != null;
        }

      
        protected Response HandleSignOut()
        {
            TunityUser.SignOutUser();
            ClearAuthCookie();
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
