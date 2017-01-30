using Concepts.Ring8.Tunity;
using Starcounter;
using Colab.Common;
using System;

namespace SignIn {
    partial class TermsOfServicePage : Page, IBound<TunityUser> {

        protected override void OnData()
        {
            base.OnData();
            AlreadyAccepted = TermsOfServiceResponse.For(Data, TermsOfService.Version)?.Accepted ?? false;
            Message = TermsOfService.Terms;
        }

        void Handle(Input.Accept Action) {
            var tosr = TermsOfServiceResponse.For(SessionData.Current.User, TermsOfService.Version );
            if (tosr == null)
            {
                tosr = new TermsOfServiceResponse()
                {
                    User = SessionData.Current.User
                };
            }
            tosr.Accepted = true;
            tosr.Time = DateTime.UtcNow;
            tosr.Text = TermsOfService.Terms;
            tosr.Version = TermsOfService.Version;
            Transaction.Commit();
            Master.RemoveModal(this);
        }

        void Handle(Input.NotAccept Action)
        {
            var tosr = TermsOfServiceResponse.For(SessionData.Current.User, TermsOfService.Version);
            if (tosr == null)
            {
                tosr = new TermsOfServiceResponse()
                {
                    User = SessionData.Current.User
                };
            }
            tosr.Accepted = false;
            tosr.Time = DateTime.UtcNow;
            tosr.Text = TermsOfService.Terms;
            tosr.Version = TermsOfService.Version;
            Transaction.Commit();
            Master.SendCommand(ColabCommand.SECRET_MORPH, "/signin/partial/signout");
            Master.RemoveModal(this);
        }
    }
}
