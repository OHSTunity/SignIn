using Starcounter;
using Starcounter.Internal;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Concepts.Ring8.Tunity;

namespace SignIn {
    internal class CommitHooks {
        /*public static string LocalAppUrl = "/signin/__db/__" + StarcounterEnvironment.DatabaseNameLower + "/societyobjects/systemusersession";
        public static string MappedTo = UriMapping.MappingUriPrefix + "/signin";

        public void Register() {
            // User signed in event
            Handle.POST(CommitHooks.LocalAppUrl, (Request request) => {
                string sessionId = request.Body;
                UserSession userSession = Db.SQL<UserSession>("SELECT o FROM Concepts.Ring8.Tunity.UserSession o WHERE o.SessionIdString = ?", sessionId).First;
                SignInPage page = GetSignInPage();

                if (userSession != null && page != null) {
                    page.SetAuthorizedState(userSession);
                }

                return (ushort)System.Net.HttpStatusCode.OK;
            });
=======

namespace SignIn {
    internal class CommitHooks {*/
        public void Register() {
            Hook<UserSession>.CommitInsert += (s, a) => {
                this.RefreshSignInState();
            };

            Hook<UserSession>.CommitDelete += (s, a) => {
                this.RefreshSignInState();
            };

            Hook<UserSession>.CommitUpdate += (s, a) => {
                this.RefreshSignInState();
            };
        }

        protected void RefreshSignInState() {
            SignInPage page = GetSignInPage();
            if (page != null) {
                page.RefreshState();
            }
        }

        protected SignInPage GetSignInPage() {
            SessionContainer container = null;

            if (Session.Current != null && Session.Current.Data is SessionContainer) {
                container = Session.Current.Data as SessionContainer;
            }

            return container != null ? container.SignIn : null;
        }
    }
}
