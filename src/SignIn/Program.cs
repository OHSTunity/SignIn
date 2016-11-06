using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Starcounter;


namespace SignIn {
    public class Program {
         public static void Main() {
             CommitHooks hooks = new CommitHooks();
             MainHandlers handlers = new MainHandlers();

             hooks.Register();
             handlers.Register();

             Colab.Common.SignInOut.AssureAdminTunityUser();
         }
    }
}
