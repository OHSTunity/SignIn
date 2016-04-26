using Concepts.Ring8.Tunity;
using Starcounter;
using Tunity.Common;
using Starcounter.Internal;
using System;

namespace SignIn {
    partial class SignInPage : Page, IBound<UserSession>
    {
        public string SignInAuthToken { get; set; }

        public void SignIn(string Username, string Password) {
            if (string.IsNullOrEmpty(Username)) {
                this.SetAnonymousState("Please input your username!");
                return;
            }
            
            string message;
            UserSession session = SignInOut.SignInTunityUser(Username, Password, null, out message);

            if (session == null) {
                this.FailedLoginCount++;
                this.SetAnonymousState(message);
            } else {
                this.RedirectUrl = "current";
                this.SetAuthorizedState(session);
            }
        }

        public TunityUser UserCB
        {
            get
            {
                return Data != null ? Data.Token.User : null;
            }
        }

        public void SignOut() {
            if (IsSignedIn)
                this.RedirectUrl = "current";
            SignInOut.SignOutTunityUser();
            this.SetAnonymousState();
        }


        public void FromCookie(string SignInAuthToken) {
            TunitySessionCookie token =  TunityDbHelper.FromName<TunitySessionCookie>(SignInAuthToken);

            if (token == null) {
                return;
            }

            UserSession session = SignInOut.SignInTunityUser(token.Name);

            if (session != null) {
                this.SetAuthorizedState(session);
            }
        }

        public void SetAuthorizedState(UserSession session) {
            if (!Db.Equals(Data, session.Token.User))
            {
                Session.ScheduleTask(session.SessionIdString, (Session s, String sessionId) =>
                {
                    try
                    {
                        this.Message = string.Empty;
                        Data = session;
                        SessionStarted = DateTime.Now.ToString();
                        this.SignInAuthToken = session.Token.Name;
                        this.IsSignedIn = true;
                        this.UpdateSignInForm();
                        s.CalculatePatchAndPushOnWebSocket();
                    }
                    catch { }
                });
            }
        }

        public void SetAnonymousState() {
            this.SetAnonymousState(String.Empty);
        }


        public void SetAnonymousState(string Message) {
            Data = null;
            SessionStarted = DateTime.Now.ToString();
            this.Message = Message;
            this.IsSignedIn = false;
            this.UpdateSignInForm();
        }

        public void RefreshState() {
            UserSession session = SignInOut.GetCurrentTunityUserSession();

            if (session != null) {
                this.SetAuthorizedState(session);
            } else {
                this.SetAnonymousState();
            }
        }

        public void UpdateSignInForm() {

            SignInFormPage page = Master.Current.GetApplication<SignInFormPage>();
            if (page == null) {
                return;
            }
            page.Redirecting = false;
            if (this.IsSignedIn)
            {
                page.RedirectUrl = page.OriginUrl;
                if (!String.IsNullOrEmpty(page.RedirectUrl) && !String.Equals(page.RedirectUrl, "current"))
                {
                    page.Redirecting = true;
                }
            }
            page.IsSignedIn = this.IsSignedIn;
            page.Message = this.Message;
            page.FailedLoginCount = this.FailedLoginCount;
        }

        [SignInPage_json.UserInfo]
        partial class UserInfoJson : Json, IBound<TunityUser>
        {
            protected override void OnData()
            {
                base.OnData();
                if (Data != null)
                {
                    Tools = Self.GET(UriMapping.MappingUriPrefix + "/user-shortcuts/" + Data.DbIDString, () =>
                    {
                        var p = new Page();
                        return p;
                    });
                }
                else
                    Tools = null;
            }
        }
    }
}
