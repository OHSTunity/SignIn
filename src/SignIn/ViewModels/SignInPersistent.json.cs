using Concepts.Ring8.Tunity;
using Starcounter;
using Colab.Common;
using Starcounter.Internal;
using System;

namespace SignIn
{
    partial class SignInPersistent : Page, IBound<UserSession>
    {
        public string OriginUri;

        public bool IsAdmin => Data?.User?.HeadAdmin ?? false;

        public bool AdminRightsEnabled => Data?.User?.ShouldDisplayAdminView ?? false;

        void Handle(Input.AdminRightsEnabled input)
        {
            Data.User.ShouldDisplayAdminView = input.Value;
            Transaction.Commit();
            ColabX.PublishMessage(ColabAPI.Messages.UserRights);
            UserInfo.Tools = Self.GET("/signin/tunityuser/shortcuts/"+ Data.User.DbIDString);
        }

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
                SessionStarted = DateTime.UtcNow.ToString("o");
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
            Master.SendCommand(ColabCommand.MORPH_URL, "/");
        }

        public void RefreshSignInState()
        {
            UserSession session = TunityUser.GetCurrentUserSession();
            if (session != null)
            {
                this.SetAuthorizedState(session);
                //Check if user accepted terms of service
                var tosr = TermsOfServiceResponse.For(session.User, TermsOfService.Version);
                if (tosr == null || tosr.Accepted == false)
                {
                    Db.Scope(() =>
                    {
                        Master.AddModal(new TermsOfServicePage() { Data = session.User });
                    });
                }
            }
            else
            {
                this.SetAnonymousState();
            }
        }

        [SignInPersistent_json.UserInfo]
        partial class UserInfoJson : Json, IBound<TunityUser>
        {
            protected override void OnData()
            {
                base.OnData();
                if (Data != null)
                {
                    Tools = Self.GET("/signin/tunityuser/shortcuts/" + Data.DbIDString);
                }
                else
                    Tools = null;
            }
        }
    }
}
