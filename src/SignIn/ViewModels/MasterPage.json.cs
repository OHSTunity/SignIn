using Starcounter;
using Concepts.Ring8.Tunity;
using Tunity.Common;

namespace SignIn {
    partial class MasterPage : Page {
        protected string url;

        public void Open(string Url) {
            this.url = Url;
            this.RefreshSignInState();
        }

        public void RefreshSignInState() {
            TunityUser user = TunityUser.GetCurrentUser();

            if (this.RequireSignIn && user != null) {
                this.Partial = Self.GET(this.url);
            } else if (this.RequireSignIn && user == null) {
                this.Partial = Self.GET("/signin/partial/accessdenied-form");
            } else if (user == null && !string.IsNullOrEmpty(this.url)) {
                this.Partial = Self.GET(this.url);
            } else if(!string.IsNullOrEmpty(this.OriginalUrl)) {
                this.Partial = null;
                Master.SendCommand(TunityCommand.MORPH_URL, this.OriginalUrl);
               // this.RedirectUrl = this.OriginalUrl;
                this.OriginalUrl = null;
            } else if (user != null) {
                this.Partial = Self.GET("/signin/partial/alreadyin-form");
            }
        }
    }
}
