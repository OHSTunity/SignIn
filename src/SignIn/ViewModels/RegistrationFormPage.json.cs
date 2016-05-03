using Starcounter;
using Concepts.Ring8.Tunity;

namespace SignIn {
    partial class RegistrationFormPage : Page {
        protected override void OnData() {
            base.OnData();

            this.Username = string.Empty;
            this.Email = string.Empty;
            this.Password = string.Empty;
            this.RepeatPassword = string.Empty;

            if (TunityUser.GetCurrentUser() != null) {
                this.Readonly = true;
                this.RegisterClick = 1;
                this.Message = "You are already signed in!";
                this.MessageCss = "alert alert-warning";
            } else {
                this.Readonly = false;
                this.RegisterClick = 0;
                this.Message = string.Empty;
            }
        }

        void Handle(Input.RegisterClick Action) {
            string message = null;

            if (string.IsNullOrEmpty(this.Username)) {
                message = "Username is required!";
            } else if (string.IsNullOrEmpty(this.Email)) {
                message = "E-mail address is required!";
            } else if (!Utils.IsValidEmail(this.Email)) {
                message = "This is not a valid e-mail address!";
            } else if (string.IsNullOrEmpty(this.Password)) {
                message = "Password is required!";
            } else if (this.Password != this.RepeatPassword) {
                message = "Passwords do not match!";
            } else if (Db.SQL("SELECT u FROM Simplified.Ring3.TunityUser u WHERE u.Username = ?", this.Username).First != null) {
                message = "This username is already taken!";
            }

            if (!string.IsNullOrEmpty(message)) {
                Action.Cancel();
                this.Message = message;
                this.MessageCss = "alert alert-danger";

                return;
            }

          //  TunityUser user = null;

            Db.Transact(() => {
                //user = TunityUser.RegisterTunityUser(this.Username, this.Email, this.Password);
            });

            this.Message = "Registration completed now you can sign in!";
            this.MessageCss = "alert alert-success";
            this.Readonly = true;
        }
    }
}
