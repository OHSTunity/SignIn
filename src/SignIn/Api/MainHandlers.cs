using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Starcounter;
using Tunity.Common;
using Concepts.Ring8.Tunity;

namespace SignIn
{
    internal class MainHandlers
    {
        protected string AuthCookieName = "soauthtoken";
        protected int rememberMeDays = 30;

        public void Register()
        {
            Tunity.Common.MainCommon.Register("signin", false);

            Application.Current.Use((Request req) =>
            {
                Cookie cookie = GetSignInCookie();

                if (cookie != null)
                {
                    if (Session.Current == null)
                    {
                        Session.Current = new Session(SessionOptions.PatchVersioning);
                    }

                    UserSession session = TunityUser.SignInSystemUser(cookie.Value);

                    if (session != null)
                    {
                        RefreshAuthCookie(session);
                    }
                }

                return null;
            });

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
                            TunityUser.SignInSystemUser(cookie.Value);
                            this.RefreshSignInState();
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
            /*
            Handle.GET("/signin/registration", () =>
            {
                MasterPage master = this.GetMaster();

                master.RequireSignIn = false;
                master.Open("/signin/partial/registration-form");

                return master;
            });

            Handle.GET("/signin/restore", () =>
            {
                MasterPage master = this.GetMaster();

                master.RequireSignIn = false;
                master.Open("/signin/partial/restore-form");

                return master;
            });

            Handle.GET("/signin/profile", () =>
            {
                MasterPage master = this.GetMaster();

                master.RequireSignIn = true;
                master.Open("/signin/partial/profile-form");

                return master;
            });

            Handle.GET("/signin/partial/signin-form", () => new SignInFormPage(), new HandlerOptions() { SelfOnly = true });
            Handle.GET("/signin/partial/registration-form", () => new RegistrationFormPage(), new HandlerOptions() { SelfOnly = true });
            Handle.GET("/signin/partial/alreadyin-form", () => new AlreadyInPage() { Data = null }, new HandlerOptions() { SelfOnly = true });
            Handle.GET("/signin/partial/restore-form", () => new RestorePasswordFormPage(), new HandlerOptions() { SelfOnly = true });
            Handle.GET("/signin/partial/profile-form", () => new ProfileFormPage() { Data = null }, new HandlerOptions() { SelfOnly = true });
            Handle.GET("/signin/partial/accessdenied-form", () => new AccessDeniedPage(), new HandlerOptions() { SelfOnly = true });

            //Test handler
            /*Handle.GET("/signin/deleteadminuser", () => {
                Db.Transact(() => {
                    Db.SlowSQL("DELETE FROM Simplified.Ring3.SystemUserGroupMember WHERE SystemUser.Username = ?", SignInOut.AdminUsername);
                    Db.SlowSQL("DELETE FROM Simplified.Ring3.SystemUser WHERE Username = ?", SignInOut.AdminUsername);
                });
                return 200;
            });*/

            UriMapping.Map("/signin/user", UriMapping.MappingUriPrefix + "/user");
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

        protected void RefreshSignInState()
        {
            SessionContainer container = this.GetSessionContainer();

            container.RefreshSignInState();
        }

        protected Response HandleSignIn()
        {
            return HandleSignIn(null, null, null);
        }

        protected Response HandleSignIn(string Username, string Password, string RememberMe)
        {
            Username = Uri.UnescapeDataString(Username);

            UserSession session = TunityUser.SignInSystemUser(Username, Password);

            if (session == null)
            {
                Master m = (Master)Self.GET("/signin/master");
               
                string message = "Invalid username or password!";

                if (container.SignIn != null)
                {
                    container.SignIn.Message = message;
                }

                if (master != null && master.Partial is SignInFormPage)
                {
                    SignInFormPage page = master.Partial as SignInFormPage;
                    page.Message = message;
                }
            }

            SetAuthCookie(session, RememberMe == "true");

            return this.GetSessionContainer();

            SignInPage page = Self.GET<Master.MasterUtils>("/signin/user").PersistantApp as SignInPage;
            page.SignIn(Username, Password);
            SetAuthCookie(page);

            var sifp = Master.Current.GetApplication<SignInFormPage>();
            if (sifp == null)
            {
                Db.Scope(() =>
                    {
                        sifp = new SignInFormPage();
                        Master.Current.SetApplication(sifp);
                    });
            }
            return sifp;
        }

        protected Response HandleSignInForm()
        {
            return this.HandleSignInForm(string.Empty);
        }

        protected Response HandleSignInForm(string OriginalUrl)
        {
            MasterPage master = this.GetMaster();

            master.RequireSignIn = false;
            master.OriginalUrl = OriginalUrl;
            master.Open("/signin/partial/signin-form");

            return master;
        }

        protected Response HandleSignOut()
        {
            SystemUser.SignOutSystemUser();
            ClearAuthCookie();

            return this.GetSessionContainer();
        }

        protected Cookie GetSignInCookie()
        {
            List<Cookie> cookies = Handle.IncomingRequest.Cookies.Select(x => new Cookie(x)).ToList();
            Cookie cookie = cookies.FirstOrDefault(x => x.Name == AuthCookieName);

            return cookie;
        }
    }

    internal class MainHandlers
    {
        }

        protected void SetAuthCookie(SignInPage Page)
        {
            Cookie cookie = new Cookie(AuthCookieName, Page.SignInAuthToken);
            Handle.AddOutgoingCookie(cookie.Name, cookie.GetFullValueString());
        }

        protected Response GetNoSessionResult()
        {
            return new Response()
            {
                StatusCode = (ushort)System.Net.HttpStatusCode.InternalServerError,
                Body = "No Current Session"
            };
        }


        protected Response HandleSignIn()
        {
            return HandleSignIn(null, null);
        }

        protected Response HandleSignIn(string Username, string Password)
        {
            SignInPage page = Self.GET<Master.MasterUtils>("/signin/user").PersistantApp as SignInPage;
            page.SignIn(Username, Password);
            SetAuthCookie(page);

            var sifp = Master.Current.GetApplication<SignInFormPage>();
            if (sifp == null)
            {
                Db.Scope(() =>
                    {
                        sifp = new SignInFormPage();
                        Master.Current.SetApplication(sifp);
                    });
            }
            return sifp;
        }

        protected Response HandleSignIn(string Query)
        {
            SignInPage page = Self.GET<Master.MasterUtils>("/signin/user").PersistantApp as SignInPage;
            
            var sifp = Master.Current.GetApplication<SignInFormPage>();
            if (sifp == null)
            {
                Db.Scope(() =>
                    {
                        sifp = new SignInFormPage();
                        Master.Current.SetApplication(sifp);
                    });
            }
            string decodedQuery = HttpUtility.UrlDecode(Query);
            NameValueCollection queryCollection = HttpUtility.ParseQueryString(decodedQuery);

            page.RedirectUrl = queryCollection.Get("originurl");
            page.UpdateSignInForm();

            return sifp;
        }

        protected Response HandleSignInUser()
        {
            SignInPage page = Self.GET<Master.MasterUtils>("/signin/user").PersistantApp as SignInPage;
            
            var sifp = Master.Current.GetApplication<SignInFormPage>();
            if (sifp == null)
            {
                Db.Scope(() =>
                {
                    sifp = new SignInFormPage();
                    Master.Current.SetApplication(sifp);
                });
            }
            page.UpdateSignInForm();

            return sifp;
        }

        protected Response HandleSignOut()
        {
            SignInPage page = Self.GET<Master.MasterUtils>("/signin/user").PersistantApp as SignInPage;

            page.SignOut();
            SetAuthCookie(page);

            var sifp = Master.Current.GetApplication<SignInFormPage>();
            if (sifp == null)
            {
                Db.Scope(() =>
                {
                    sifp = new SignInFormPage();
                    Master.Current.SetApplication(sifp);
                });
            }
            Master.SendCommand(TunityCommand.REREQUEST_URL);
            return sifp;
        }

        protected Response HandleUser()
        {
            Master m = (Master)Self.GET("/signin/master");
            if (!(m.Utils.PersistantApp is SignInPage))
            {
                Db.Scope(() =>
                {
                    var page = new SignInPage();
                    m.Utils.PersistantApp = page;
                    List<Cookie> cookies = Handle.IncomingRequest.Cookies.Select(x => new Cookie(x)).ToList();
                    Cookie cookie = cookies.FirstOrDefault(x => x.Name == AuthCookieName);
                    if (cookie != null)
                    {
                        page.FromCookie(cookie.Value);
                    }
                    else
                    {
                        page.SetAnonymousState();
                    }
                });
            }
            return m.Utils;
        }
    }
}
