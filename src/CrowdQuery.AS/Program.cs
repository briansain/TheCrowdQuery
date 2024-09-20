using Akka.Actor;
using Akka.Event;
using Akka.Hosting;
using Akka.Logger.Serilog;
using Akka.Persistence.Sql.Hosting;
using CrowdQuery.AS.Actors.Question;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CrowdQuery.AS
{
	internal class Program
	{
		static void Main(string[] args)
		{

			// Host.CreateDefaultBuilder(args)
			// 	.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
			// 		.ReadFrom.Configuration(hostingContext.Configuration)
			// 		.Enrich.FromLogContext()
			// 		.WriteTo.Console())
			// 	.ConfigureServices((hostContext, services) =>
			// 	{
			// 		Log.Fatal("STARTING AS ON ITS OWN");
			// 		//services.AddCrowdQueryAkka(hostContext.Configuration);
			// 		services.AddHostedService<AkkaHostedService>();
			// 	}).Build().Run();
		}
	}
}
