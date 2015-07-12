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
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build()
                .PerformUpgrade();

            return result.Successful ? 0 : 1;
        }
    }
}