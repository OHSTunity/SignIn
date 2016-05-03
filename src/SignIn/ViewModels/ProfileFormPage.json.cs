using Starcounter;
using Concepts.Ring8.Tunity;
using Concepts.Ring1;
using Concepts.Ring2;

namespace SignIn {
    partial class ProfileFormPage : Page {
        protected override void OnData() {
            base.OnData();

            TunityUser user = TunityUser.GetCurrentUser();
            this.Username = user.Name;
            this.Email = user.Email;
        }

        void Handle(Input.UpdateClick Action) {
            Action.Cancel();
            this.Message = null;
            this.MessageCss = "alert alert-danger";

            if (string.IsNullOrEmpty(this.Email)) {
                this.Message = "E-mail address is required!";
                return;
            }

            if (!Utils.IsValidEmail(this.Email)) {
                this.Message = "This is not a valid e-mail address!";
                return;
            }

            Db.Transact(() => {
                TunityUser user = TunityUser.GetCurrentUser();
                Address email = user.EmailAddress;// Utils.GetUserAddress(user);

                if (email == null) {
                    email = new Address();

                    EMailRelation relation = new EMailRelation() {
                        Address = email,
                        Addressee = user.WhoIs
                    };
                }

                email.Name = this.Email;
            });

            this.Message = "Profile changes has been updated";
            this.MessageCss = "alert alert-success";
        }

        void Handle(Input.ChangePasswordClick Action) {
            Action.Cancel();
            this.Message = null;
            this.MessageCss = "alert alert-danger";

            TunityUser user = TunityUser.GetCurrentUser();
            string password = TunityUser.GenerateClientSideHash(this.OldPassword);
            TunityUser.GeneratePasswordHash(user.Name, password, user.PasswordSalt, out password);

            if (password != user.Password) {
                this.Message = "Invalid old password!";
                return;
            }

            if (string.IsNullOrEmpty(this.NewPassword)) {
                this.Message = "New password is required!";
                return;
            }

            if (this.NewPassword != this.RepeatPassword) {
                this.Message = "Passwords do not match!";
                return;
            }

            password = TunityUser.GenerateClientSideHash(this.NewPassword);
            TunityUser.GeneratePasswordHash(user.Name, password, user.PasswordSalt, out password);

            Db.Transact(() => {
                user.Password = password;
            });

            this.Message = "Your password has been successfully changed";
            this.MessageCss = "alert alert-success";
            this.OldPassword = null;
            this.NewPassword = null;
            this.RepeatPassword = null;
        }
    }
}
