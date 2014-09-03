using Concepts.Ring1;
using Concepts.Ring3;
using SignInApp.Database;
using Starcounter;
using System;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Web;

namespace SignInApp.Server {

    [SignIn_json]
    partial class SignIn : Json {

        public object MyPage;


        static void Main() {

            // Add some sample data
            SampleData.Init();

            // Register handlers
            Handlers.RegisterHandlers();
        }

        #region Handler

        /// <summary>
        /// Sign-In handler
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.SignIn action) {

            string message;
            SystemUserSession userSession = SignInOut.SignInSystemUser(this.UserID, this.Password, this.SignInAuthToken, out message);
            if (userSession == null) {
                if (!string.IsNullOrEmpty(message)) {
                    this.ClearViewModelProperties(true);
                    this.Message = message;
                }
                else {
                    this.ClearViewModelProperties(true);
                }
            }
        }

        /// <summary>
        /// Sign-Out handler
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.SignOut action) {

            SignInOut.SignOutSystemUser(this.AuthToken);
        }

        #endregion

        #region Update View Model
        /// <summary>
        /// Set properties
        /// </summary>
        /// <param name="userSession"></param>
        internal void SetViewModelProperties(SystemUserSession userSession) {

            this.Message = string.Empty;

            if (userSession.User.WhoIs != null) {
                this.FullName = userSession.User.WhoIs.FullName;
            }
            else {
                this.FullName = userSession.User.Username;
            }

            //this.FullName = signedInUserSession.User.Somebody.FullName;
            this.AuthToken = userSession.Token.Token;
            this.IsSignedIn = true;
        }

        /// <summary>
        /// Clear properties
        /// </summary>
        internal void ClearViewModelProperties() {

            this.ClearViewModelProperties(false);
        }

        /// <summary>
        /// Clear properties
        /// </summary>
        /// <param name="keepUserIDAndPassword"></param>
        void ClearViewModelProperties(bool keepUserIDAndPassword) {

            if (!keepUserIDAndPassword) {
                this.UserID = string.Empty;
                this.Password = string.Empty;
            }
            this.AuthToken = string.Empty;
            this.FullName = string.Empty;
            this.Message = string.Empty;
            this.IsSignedIn = false;
        }

        #endregion

        #region Base
        // Browsers will ask for "text/html" and we will give it to them
        // by loading the contents of the URI in our Html property
        public override string AsMimeType(MimeType type) {
            if (type == MimeType.Text_Html) {
                return X.GET<string>(Html);
            }
            return base.AsMimeType(type);
        }

        /// <summary>
        /// The way to get a URL for HTML partial if any.
        /// </summary>
        /// <returns></returns>
        public override string GetHtmlPartialUrl() {
            return Html;
        }

        /// <summary>
        /// Whenever we set a bound data object to this page, we update the
        /// URI property on this page.
        /// </summary>
        protected override void OnData() {
            base.OnData();
            var str = "";
            Json x = this;
            while (x != null) {
                if (x is SignIn)
                    str = (x as SignIn).UriFragment + str;
                x = x.Parent;
            }
            Uri = str;
        }

        /// <summary>
        /// Override to provide an URI fragment
        /// </summary>
        /// <returns></returns>
        protected virtual string UriFragment {
            get {
                return "";
            }
        }
        #endregion
    }

}
