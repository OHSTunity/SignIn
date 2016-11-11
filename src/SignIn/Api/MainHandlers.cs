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

            Handle.GET<string, string, string>("/signin/partial/signin/{?}/{?}/{?}", HandleSignIn, new HandlerOptions() { SkipRequestFilters = true });
            Handle.GET("/signin/partial/signin/", HandleSignIn, new HandlerOptions() { SkipRequestFilters = true });
            Handle.GET("/signin/partial/signin", HandleSignIn, new HandlerOptions() { SkipRequestFilters = true });
            Handle.GET("/signin/partial/signout", HandleSignOut, new HandlerOptions() { SkipRequestFilters = true });

            Handle.GET("/signin/signinuser", HandleSignInForm);
            Handle.GET<string>("/signin/signinuser?{?}", HandleSignInForm);

            Handle.GET("/signin/partial/signin-form", () => new SignInFormPage(), new HandlerOptions() { SelfOnly = true });

            UriMapping.Map("/signin/user", UriMapping.MappingUriPrefix + "/user");
            UriMapping.Map("/signin/partial/signin-form", UriMapping.MappingUriPrefix + "/signin");
            UriMapping.Map("/signin/mobile/user", UriMapping.MappingUriPrefix + "/mobile/user");
            UriMapping.Map("/signin/signinuser", UriMapping.MappingUriPrefix + "/userform"); //inline form; used in RSE Launcher
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
            Handle.AddOutgoingCookie(cookie.Name, cookie.GetFullValueString());
        }

        protected void SetAuthCookie(UserSession session, bool RememberMe)
        {
            Cookie cookie = new Cookie()
            {
                Name = AuthCookieName
            };

            if (session != null && session.Token != null)
            {
                cookie.Value = session.Token.Name;
            }

            if (session == null)
            {
                cookie.Expires = DateTime.Today;
            }
            else if (RememberMe)
            {
                cookie.Expires = DateTime.Now.AddDays(rememberMeDays);
            }
            else
            {
                cookie.Expires = DateTime.Now.AddDays(1);
            }

            Handle.AddOutgoingCookie(cookie.Name, cookie.GetFullValueString());
        }



        protected MasterPage GetMainPage()
        {
            MasterPage m = Master.Current.GetApplication<MasterPage>();
           
            if (m == null)
            {
                m = Master.Current.SetApplication(new MasterPage()) as MasterPage;
            }

            return m;
        }

        protected void RefreshSignInState()
        {
            Utils.RefreshSignInState();
        }

        protected Response HandleSignIn()
        {
            return HandleSignIn(null, null, null);
        }

        protected Response HandleSignIn(string Username, string Password, string RememberMe)
        {
            Username = Uri.UnescapeDataString(Username);

            UserSession session = TunityUser.SignInUser(Username, Password);

            if (session == null)
            {
                Utils.SetMessage("Invalid username or password!");
            }

            SetAuthCookie(session, RememberMe == "true");

            RefreshSignInState();
            return Master.Current;
        }

        protected Response HandleSignInForm()
        {
            return this.HandleSignInForm(string.Empty);
        }

        protected Response HandleSignInForm(string query)
        {
            MasterPage main = this.GetMainPage();

            main.RequireSignIn = false;
            main.OriginalUrl = GetOriginalUrl(query);
            main.Open("/signin/partial/signin-form");

            return main;
        }

        protected String GetOriginalUrl(String query)
        {
            var collection = HttpUtility.ParseQueryString(query);
            try
            {
                return collection.Get("originurl");
            }
            catch
            {
                return "";
            }
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
