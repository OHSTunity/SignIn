using System;
using Starcounter;

namespace SignIn {
    internal class SessionContainer : Json {
        public SignInPage SignIn {
            get;
            set;
        } 

        public MasterPage Master {
            get;
            set;
        }

        public void RefreshSignInState() {
            if (this.SignIn != null) {
                this.SignIn.RefreshSignInState();
            }

            if (this.Master != null) {
                this.Master.RefreshSignInState();
            }
        }
    }
}
