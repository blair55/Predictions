using System;
using Microsoft.Owin.Hosting;
using Predictions.Api;
using Owin;

namespace Predictions.SelfHost
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (WebApp.Start("http://127.0.0.1:9000/", app =>
            {
                //Config.BuildApp(app);
                app.UseErrorPage();
                app.UseWelcomePage("/");
            }))
            {
                Console.WriteLine("starting app on :9000");
                Console.WriteLine("I'm running on {0} directly from assembly {1}", Environment.OSVersion, System.Reflection.Assembly.GetEntryAssembly().FullName);
          
                Console.ReadLine();
            }
        }
    }
}
