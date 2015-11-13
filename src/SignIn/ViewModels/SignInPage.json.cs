using Concepts.Ring8.Tunity;
using Starcounter;

namespace SignIn {
    partial class SignInPage : Page {
        public string SignInAuthToken { get; set; }

        public void SignIn(string Username, string Password) {
            if (string.IsNullOrEmpty(Username)) {
                this.SetAnonymousState(false, "Please input your username!");
                return;
            }

            string message;
            UserSession session = SignInOut.SignInSystemUser(Username, Password, null, out message);

            if (session == null) {
                this.SetAnonymousState(true, message);
            } else {
                this.SetAuthorizedState(session);
            }
        }

        public void SignOut() {
            if (IsSignedIn)
                this.RedirectUrl = "current";
            SignInOut.SignOutSystemUser();
            this.SetAnonymousState();
        }

        public void FromCookie(string SignInAuthToken) {
            TunitySessionCookie token =  TunityDbHelper.FromName<TunitySessionCookie>(SignInAuthToken);

            if (token == null) {
                return;
            }

            UserSession session = SignInOut.SignInSystemUser(token.Name);

            if (session != null) {
                this.SetAuthorizedState(session);
            }
        }

        public void SetAuthorizedState(UserSession Session) {
            this.Message = string.Empty;

            if (Session.Token.User.WhoIs != null) {
                this.FullName = Session.Token.User.WhoIs.FullName;

                if (!string.IsNullOrEmpty(Session.Token.User.WhoIs.ImageURL)) {
                    this.ImageUrl = Session.Token.User.WhoIs.ImageURL;
                }
                else {
                    this.ImageUrl = Concepts.Ring8.Tunity.Avatar.GetValueString(Session.Token.User.WhoIs);//Utils.GetGravatarUrl(string.Empty);
                }
            }
            else {
                this.FullName = Session.Token.User.Username;
                this.ImageUrl = Utils.GetGravatarUrl(string.Empty);
            }

            this.SignInAuthToken = Session.Token.Name;
            this.IsSignedIn = true;

            this.UpdateSignInForm();
        }

        public void SetAnonymousState() {
            this.SetAnonymousState(false);
        }

        public void SetAnonymousState(bool KeepUsernameAndPassword) {
            this.SetAnonymousState(KeepUsernameAndPassword, string.Empty);
        }

        public void SetAnonymousState(bool KeepUsernameAndPassword, string Message) {
            if (!KeepUsernameAndPassword) {
                this.Username = string.Empty;
                this.Password = string.Empty;
            }

            this.SignInAuthToken = string.Empty;
            this.FullName = string.Empty;
            this.Message = Message;
            this.IsSignedIn = false;

            this.UpdateSignInForm();
        }

        public void RefreshState() {
            UserSession session = SignInOut.GetCurrentSystemUserSession();

            if (session != null) {
                this.SetAuthorizedState(session);
            } else {
                this.SetAnonymousState();
            }
        }

        public void UpdateSignInForm() {
            SessionContainer container = Session.Current.Data as SessionContainer;

            if (container == null) {
                return;
            }

            SignInFormPage page = container.SignInForm;

            if (page == null) {
                return;
            }

            if (this.IsSignedIn)
            {
                page.Username = string.Empty;
                page.Password = string.Empty;
                page.RedirectUrl = page.OriginUrl;
            }


            page.IsSignedIn = this.IsSignedIn;
            page.Message = this.Message;
        }
    }
}
