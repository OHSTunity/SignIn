using Concepts.Ring8;
using Starcounter;

namespace SignIn {
    partial class SignInFormPage : Page {
        void Handle(Input.SignInClick Action) {
            this.Message = null;
            Action.Cancel();

            if (string.IsNullOrEmpty(this.Username)) {
                this.Message = "Username is required!";
                return;
            }

            this.Submit++;
        }
    }
}
