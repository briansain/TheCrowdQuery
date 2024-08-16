using Akka.Actor;
using Akka.Hosting;
using Akka.Logger.Serilog;
using Akka.Persistence.Sql.Hosting;
using Akka.Skeleton.Persistence.Actors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using LinqToDB;
using CrowdQuery.Actors.Question;
using CrowdQuery.Actors.Answer;

namespace Akka.Skeleton.Persistence
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
                            configLoggers.LogLevel = Event.LogLevel.DebugLevel;
                            configLoggers.LogConfigOnStart = true;
                            configLoggers.ClearLoggers();
                            configLoggers.AddLogger<SerilogLogger>();
                        })
                        .WithSqlPersistence("Host=localhost;Port=5432;database=crowdquery;username=postgres;password=postgrespassword;", ProviderName.PostgreSQL15)
                        .WithActors((actorSystem, registry) =>
                        {
                            var questionManager = actorSystem.ActorOf(Props.Create<QuestionManager>(), "question-manager");
                            registry.Register<QuestionManager>(questionManager);

                            var answerManager = actorSystem.ActorOf(Props.Create<AnswerManager>(), "answer-manager");
                            registry.Register<AnswerManager>(answerManager);
                        });
                    });
                    services.AddHostedService<AkkaHostedService>();
                }).Build().Run();
        }
    }
}
