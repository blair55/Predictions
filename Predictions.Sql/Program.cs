using System;
using System.Configuration;
using DbUp;

namespace Predictions.Sql
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var result = DeployChanges.To
                .SqlDatabase(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString)
                .WithScriptsFromFileSystem("/Users/nblair/lab/Predictions/Predictions.Sql/Scripts")
                // .WithScriptsFromFileSystem("/Users/nblair/lab/Predictions/Predictions.Sql/ResultScripts")
                .LogToConsole()
                .Build()
                .PerformUpgrade();

            Console.ReadKey();
            return result.Successful ? 0 : 1;
        }
    }
}
