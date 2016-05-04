using Starcounter;
using Starcounter.Internal;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Concepts.Ring8.Tunity;
using Tunity.Common;

namespace SignIn {
    internal class CommitHooks {
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
     //       if (Session.Current != null) //Do not refresh if called before launcher has created session!!!!
            //    Utils.RefreshSignInState();
        }
    }
}
