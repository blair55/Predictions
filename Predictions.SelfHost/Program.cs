using System;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Predictions.Api;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.StaticFiles;

namespace Predictions.SelfHost
{
    class Program
    {
        public static void Main(string[] args)
        {
            //using (WebApp.Start("http://localhost:9001/", app =>
            using (WebApp.Start("http://*:80/", app =>
            {
                app.UseErrorPage();
                //app.UseWelcomePage("/");

                var options = new FileServerOptions
                {
                    EnableDirectoryBrowsing = true,
                    RequestPath = PathString.Empty,
                    //FileSystem = new PhysicalFileSystem(@"../../../Predictions.Host"),
                    FileSystem = new PhysicalFileSystem(@"../../../Predictions.Host.AppHb"),
                };

                app.UseFileServer(options);

                Config.buildApp(app);
            }))
            {
                Console.WriteLine("starting app on *:9000");
                Console.WriteLine("I'm running on {0} directly from assembly {1}", Environment.OSVersion, System.Reflection.Assembly.GetEntryAssembly().FullName);

                Console.ReadLine();
            }
        }
    }
}
