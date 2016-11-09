using Concepts.Ring8.Tunity;
using Starcounter;
using Colab.Common;
using Starcounter.Internal;
using System;

namespace SignIn
{
    partial class SignInPage : Page, IBound<UserSession>
    {
        void Handle(Input.SignInClick Action)
        {
            this.Message = null;

            if (string.IsNullOrEmpty(this.Username))
            {
                this.Message = "Username is required!";
                return;
            }

            this.Submit++;
        }

        public void SetAuthorizedState(UserSession session)
        {
            if (!Db.Equals(Data, session.Token.User))
            {
                this.Message = string.Empty;
                SessionStarted = DateTime.Now.ToString("o");
                Data = session;
                this.IsSignedIn = true;
            }
        }

        public void SetAnonymousState()
        {
            this.Username = string.Empty;
            this.Data = null;
            this.Message = Message;
            this.IsSignedIn = false;
        }

        public void RefreshSignInState()
        {
            UserSession session = TunityUser.GetCurrentUserSession();
            if (session != null)
            {
                this.SetAuthorizedState(session);
            }
            else
            {
                this.SetAnonymousState();
            }
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
