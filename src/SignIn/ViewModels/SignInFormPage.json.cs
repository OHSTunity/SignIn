using Concepts.Ring8.Tunity;
using Starcounter;
using Colab.Common;

namespace SignIn {
    partial class SignInFormPage : Page {

        protected override void OnData()
        {
            base.OnData();
        }

        void Handle(Input.SignInClick Action) {
            this.Message = null;
         
            this.Submit++;
        }

        public void RefreshSignInState()
        {
            UserSession session = TunityUser.GetCurrentUserSession();
            IsSignedIn = session != null;
        }



        void Handle(Input.SignOut action)
        {

        }
        void Handle(Input.ForgotPassword input)
        {
            //Always reset message when changing status on forgotpassword
            this.Message = "";
        }

        void Handle(Input.SubmitForgotPassword input)
        {
            if (!EmailManager.IsValidEmailAddress(this.Username))
            {
                this.Message = "That is not a valid email adress";
                return;
            }
            var user = UserHelper.FromEmailToUser(this.Username);
            if (user == null)
            {
                this.Message = "No user registred with that email address";
                return;
            }
            Db.Transact(() =>
            {
                var er = UserHelper.SendResetPassword(user);
                if (er.Succeded)
                {
                    this.Message = "success";
                }
                else
                {
                    this.Message = "Email not sent:" + er.ResultAsString;
                }
            });
        }
    }
}
