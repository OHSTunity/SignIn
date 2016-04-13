using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Starcounter;
using Tunity.Common;

namespace SignIn
{
    internal class MainHandlers
    {
        protected string AuthCookieName = "soauthtoken";

        public void Register()
        {

            Tunity.Common.MainCommon.Register("signin");

            Handle.GET("/signin/user", HandleUser);
            Handle.GET<string, string>("/signin/signin/{?}/{?}", HandleSignIn);
            Handle.GET("/signin/signin/", HandleSignIn);
            Handle.GET("/signin/signin", HandleSignIn);
            Handle.GET("/signin/signout", HandleSignOut);
            Handle.GET("/signin/signinuser", HandleSignInUser);
            Handle.GET<string>("/signin/signinuser?{?}", HandleSignIn);

            UriMapping.Map("/signin/user", UriMapping.MappingUriPrefix + "/user");
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
            SignInPage page = Self.GET<Master>("/signin/user").PersistantApp as SignInPage;
            page.SignIn(Username, Password);
            SetAuthCookie(page);

            Root root = Root.Current;
            var sifp = root.GetApplication<SignInFormPage>();
            if (sifp == null)
            {
                Db.Scope(() =>
                    {
                        sifp = new SignInFormPage();
                        root.SetApplication(sifp);
                    });
            }
            return sifp;
        }

        protected Response HandleSignIn(string Query)
        {
            SignInPage page = Self.GET<Master>("/signin/user").PersistantApp as SignInPage;
            var sifp = Root.Current.GetApplication<SignInFormPage>();
            if (sifp == null)
            {
                Db.Scope(() =>
                    {
                        sifp = new SignInFormPage();
                        Root.Current.SetApplication(sifp);
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
            SignInPage page = Self.GET<Master>("/signin/user").PersistantApp as SignInPage;
            var sifp = Root.Current.GetApplication<SignInFormPage>();
            if (sifp == null)
            {
                Db.Scope(() =>
                {
                    sifp = new SignInFormPage();
                    Root.Current.SetApplication(sifp);
                });
            }
            page.UpdateSignInForm();

            return sifp;
        }

        protected Response HandleSignOut()
        {
            SignInPage page = Self.GET<Master>("/signin/user").PersistantApp as SignInPage;

            page.SignOut();
            SetAuthCookie(page);

            var sifp = Root.Current.GetApplication<SignInFormPage>();
            if (sifp == null)
            {
                Db.Scope(() =>
                {
                    sifp = new SignInFormPage();
                    Root.Current.SetApplication(sifp);
                });
            }

            return sifp;
        }

        protected Response HandleUser()
        {
            Root m = (Root)Self.GET("/signin/root");
            if (!((m.Utils as Master).PersistantApp is SignInPage))
            {
                Db.Scope(() =>
                {
                    var page = new SignInPage();
                    (m.Utils as Master).PersistantApp = page;
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
