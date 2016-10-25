using Concepts.Ring8;
using Starcounter;

namespace SignIn {
    partial class SignInFormPage : Page {
        void Handle(Input.SignInClick Action) {
            this.Message = null;
         
            if (string.IsNullOrEmpty(this.Username)) {
                this.Message = "Username is required!";
                return;
            }

            this.Submit++;
        }

        void Handle(Input.SignOut action)
        {

        }
    }
}
