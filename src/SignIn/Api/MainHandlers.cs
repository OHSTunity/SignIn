using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Starcounter;

namespace SignIn {
    internal class MainHandlers {
        protected string AuthCookieName = "soauthtoken";

        public void Register() {
            //Testing JWT
            /*Handle.GET("/signin/jwt/{?}/{?}", (string Username, string Password) => {
                string message;
                SystemUserSession session = SignInOut.SignInSystemUser(Username, Password, null, out message);

                if (session != null) {
                    string jwt = JWT.JsonWebToken.Encode(new { Username = Username, Issuer = "Polyjuice.SignIn" }, session.Token.User.Password, JWT.JwtHashAlgorithm.HS256);
                    Handle.AddOutgoingHeader("x-jwt", jwt);
                }

                return 200;
            });*/

            Handle.GET("/signin/user", HandleUser);
            Handle.GET<string, string>("/signin/signin/{?}/{?}", HandleSignIn);
            Handle.GET("/signin/signin/", HandleSignIn);
            Handle.GET("/signin/signin", HandleSignIn);
            Handle.GET("/signin/signout", HandleSignOut);
            Handle.GET("/signin/signinuser", HandleSignInUser);
            Handle.GET<string>("/signin/signinuser?{?}", HandleSignIn);

            //Test handler
            /*Handle.GET("/signin/deleteadminuser", () => {
                Db.Transact(() => {
                    Db.SlowSQL("DELETE FROM Simplified.Ring3.SystemUserGroupMember WHERE SystemUser.Username = ?", SignInOut.AdminUsername);
                    Db.SlowSQL("DELETE FROM Simplified.Ring3.SystemUser WHERE Username = ?", SignInOut.AdminUsername);
                });

                return 200;
            });*/

            UriMapping.Map("/signin/user", UriMapping.MappingUriPrefix + "/user");
        }

        protected void SetAuthCookie(SignInPage Page) {
            Cookie cookie = new Cookie(AuthCookieName, Page.SignInAuthToken);

            Handle.AddOutgoingCookie(cookie.Name, cookie.GetFullValueString());
        }

        protected Response GetNoSessionResult() {
            return new Response() {
                StatusCode = (ushort)System.Net.HttpStatusCode.InternalServerError,
                Body = "No Current Session"
            };
        }

        protected SessionContainer GetSessionContainer() {
            SessionContainer container = null;

            if (Session.Current != null) {
                container = Session.Current.Data as SessionContainer;

                if (container == null && Session.Current.Data != null) {
                    throw new Exception("Invalid object in session!");
                }
            }

            if (container == null) {
                container = new SessionContainer();
                Session.Current.Data = container;
            }

            return container;
        }

        protected Response HandleSignIn() {
            return HandleSignIn(null, null);
        }

        protected Response HandleSignIn(string Username, string Password) {
            SessionContainer container = this.GetSessionContainer();

            container.SignIn.SignIn(Username, Password);
            SetAuthCookie(container.SignIn);

            return container.SignInForm != null ? (Json)container.SignInForm : (Json)container.SignIn;
        }

        protected Response HandleSignIn(string Query) {
            SignInPage master = Self.GET<Page>("/signin/user") as SignInPage;
            SignInFormPage page = new SignInFormPage();
            string decodedQuery = HttpUtility.UrlDecode(Query);
            NameValueCollection queryCollection = HttpUtility.ParseQueryString(decodedQuery);
            SessionContainer container = this.GetSessionContainer();

            page.OriginUrl = queryCollection.Get("originurl");
            container.SignInForm = page;
            master.UpdateSignInForm();

            return page;
        }

        protected Response HandleSignInUser() {
            SignInPage master = Self.GET<Page>("/signin/user") as SignInPage;
            SignInFormPage page = new SignInFormPage();
            SessionContainer container = this.GetSessionContainer();

            container.SignInForm = page;
            master.UpdateSignInForm();

            return page;
        }

        protected Response HandleSignOut() {
            SessionContainer container = this.GetSessionContainer();

            container.SignIn.SignOut();
            SetAuthCookie(container.SignIn);

            return container.SignInForm != null ? (Json)container.SignInForm : (Json)container.SignIn;
        }

        protected Response HandleUser() {
            SessionContainer container = this.GetSessionContainer();

            if (container.SignIn != null) {
                return container.SignIn;
            }

            List<Cookie> cookies = Handle.IncomingRequest.Cookies.Select(x => new Cookie(x)).ToList();
            Cookie cookie = cookies.FirstOrDefault(x => x.Name == AuthCookieName);
            SignInPage page = new SignInPage();

            if (cookie != null) {
                page.FromCookie(cookie.Value);
            } else {
                page.SetAnonymousState();
            }

            container.SignIn = page;

            //Testing JWT
            /*if (Handle.IncomingRequest.HeadersDictionary.ContainsKey("x-jwt")) {
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string jwt = Handle.IncomingRequest.HeadersDictionary["x-jwt"];
                Dictionary<string, string> payload = JWT.JsonWebToken.DecodeToObject<Dictionary<string, string>>(jwt, string.Empty, false);
                string username = payload["Username"];
                SystemUser user = Db.SQL<SystemUser>("SELECT su FROM Simplified.Ring3.SystemUser su WHERE su.Username = ?", username).First;

                try {
                    JWT.JsonWebToken.DecodeToObject<Dictionary<string, string>>(jwt, user.Password, true);
                    page.SetAuthorizedState(SignInOut.SignInSystemUser(user));
                } catch (JWT.SignatureVerificationException) { 
                }
            }*/

            return page;
        }
    }
}
