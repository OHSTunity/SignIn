using Starcounter;
using Concepts.Ring8.Tunity;

namespace SignIn {
    partial class AlreadyInPage : Page {
        protected override void OnData() {
            base.OnData();

            TunityUser user = TunityUser.GetCurrentUser();

            if (user != null) {
                this.Username = user.Name;
            }
        }
    }
}
