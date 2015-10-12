using Starcounter;
using Starcounter.Internal;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Concepts.Ring8.Tunity;

namespace SignIn {
    internal class CommitHooks {
        public static string LocalAppUrl = "/signin/__db/__" + StarcounterEnvironment.DatabaseNameLower + "/societyobjects/systemusersession";
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

            // User signed out event
            Handle.DELETE(CommitHooks.LocalAppUrl, () => {
                SignInPage page = GetSignInPage();

                if (page != null) {
                    page.SetAnonymousState();
                }

                return (ushort)System.Net.HttpStatusCode.OK;
            });

            UriMapping.Map(CommitHooks.LocalAppUrl, CommitHooks.MappedTo, "POST");
            UriMapping.Map(CommitHooks.LocalAppUrl, CommitHooks.MappedTo, "DELETE");
        }

        private SignInPage GetSignInPage() {
            if (Session.Current != null && Session.Current.Data is SignInPage) {
                return Session.Current.Data as SignInPage;
            }

            return null;
        }
    }
}
