using System;
using System.Configuration;
using System.Reflection;
using DbUp;

namespace Predictions.Sql
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var result = DeployChanges.To
                .SqlDatabase(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString)
                .WithScriptsFromFileSystem("Scripts")
                .LogToConsole()
                .Build()
                .PerformUpgrade();
            
            Console.ReadKey();
            return result.Successful ? 0 : 1;
        }
    }
}