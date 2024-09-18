using Akka.Actor;
using Akka.Event;
using Akka.Hosting;
using Akka.Logger.Serilog;
using Akka.Persistence.Sql.Hosting;
using CrowdQuery.Akka.Actors.Question;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CrowdQuery.Akka
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Host.CreateDefaultBuilder(args)
				.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
					.ReadFrom.Configuration(hostingContext.Configuration)
					.WriteTo.Console())
				.ConfigureServices(services =>
				{
					services.AddAkka("crowdquery-service", builder =>
					{
						builder.ConfigureLoggers(configLoggers =>
						{
							configLoggers.LogLevel = LogLevel.DebugLevel;
							configLoggers.LogConfigOnStart = true;
							configLoggers.ClearLoggers();
							configLoggers.AddLogger<SerilogLogger>();
						})
						.WithSqlPersistence("Host=localhost;Port=5432;database=crowdquery;username=postgres;password=postgrespassword;", ProviderName.PostgreSQL15)
						.WithActors((actorSystem, registry) =>
						{
							var questionManager = actorSystem.ActorOf(Props.Create<QuestionManager>(), "question-manager");
							registry.Register<QuestionManager>(questionManager);
						});
					});
					services.AddHostedService<AkkaHostedService>();
				}).Build().Run();
		}
	}
}
