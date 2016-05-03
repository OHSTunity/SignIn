using System;
using System.Net;
using System.Net.Mail;
using Starcounter;
using Concepts.Ring8.Tunity;
using Concepts.Ring1;
using Concepts.Ring2;

namespace SignIn {
    partial class RestorePasswordFormPage : Page {
        void Handle(Input.RestoreClick Action) {
            this.MessageCss = "alert alert-danger";

            if (string.IsNullOrEmpty(this.Username)) {
                this.Message = "E-mail address is required!";
                return; 
            }

            TunityUser user = TunityUser.GetUser(this.Username);

            if (user == null) {
                this.Message = "Invalid username!";
                return;
            }

            Person person = user.WhoIs as Person;
            Address email = null;// Utils.GetUserAddress(user);

            if (person == null || email == null) {
                this.Message = "Unable to restore password, no e-mail address found!";
                return;
            }

            string password = Utils.RandomString(5);
            string hash = TunityUser.GenerateClientSideHash(password);

            TunityUser.GeneratePasswordHash(user.Name, hash, user.PasswordSalt, out hash);

            Db.Transact(() => {
                user.Password = hash;
            });

            this.SendNewPassword(person.FullName, user.Name, password, email.Name);
            this.Message = "Your new password has been sent to your email address.";
            this.MessageCss = "alert alert-success";
        }

        protected void SendNewPassword(string Name, string Username, string NewPassword, string Email) {
         /*   SettingsMailServer settings = this.GetSettings();
            MailMessage mail = new MailMessage(settings.Username, Email);
            SmtpClient client = new SmtpClient();

            client.Port = settings.Port;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(settings.Username, settings.Password);
            client.Host = settings.Host;
            client.EnableSsl = settings.EnableSsl;

            mail.Subject = "Restore password";
            mail.Body = string.Format("<h1>Hello {0}</h1><p>You have requested a new password for your <b>{1}</b> account.</p><p>Your new password is: <b>{2}</b>.</p>", Name, Username, NewPassword);
            mail.IsBodyHtml = true;
            client.Send(mail);*/
        }

     /*   protected SettingsMailServer GetSettings() {
            string name = "SignInRestorePassword";
            SettingsMailServer settings = Db.SQL<SettingsMailServer>("SELECT s FROM Simplified.Ring6.SettingsMailServer s WHERE s.Name = ?", name).First;

            if (settings == null) {
                Db.Transact(() => {
                    settings = new SettingsMailServer() {
                        Name = name,
                        Port = 587,
                        Host = "mail.your-server.de",
                        Username = "signinapp@starcounter.io",
                        Password = "*****",
                        EnableSsl = true
                    };
                });
            }

            return settings;
        }*/
    }
}
