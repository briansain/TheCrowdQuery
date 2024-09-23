using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Event;
using Akka.Hosting;
using Akka.Logger.Serilog;
using Akka.Persistence.Sql.Hosting;
using Akka.Remote.Hosting;
using CrowdQuery.AS.Actors;
using CrowdQuery.AS.Actors.Prompt;
using CrowdQuery.AS.Sagas.PromptSaga;
using LinqToDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdQuery.AS;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddCrowdQueryAkka(this IServiceCollection services, IConfiguration configuration)
    {
        var config = new CrowdQueryAkkaConfiguration();
        configuration.Bind("Akka", config);

        var PromptSagaConfig = new PromptSagaConfiguration();
        configuration.Bind("CrowdQuery:PromptSaga", PromptSagaConfig);
        if (config.IsInvalid())
        {
            throw new ArgumentException("Must provide a valid 'Akka' section config");
        }

        services.AddAkka("crowdquery-service", builder =>
        {
            builder.ConfigureLoggers(configLoggers =>
            {
                configLoggers.LogLevel = LogLevel.DebugLevel;
                configLoggers.LogConfigOnStart = true;
                configLoggers.ClearLoggers();
                configLoggers.AddLogger<SerilogLogger>();
            })
            .WithSqlPersistence(config.ConnectionString, ProviderName.PostgreSQL15)
            .WithRemoting("localhost", 5110)
            .WithClustering(new ClusterOptions()
            {
                SeedNodes = ["akka://crowdquery-service"]
            })
            // .WithDistributedData(options => 
            // {
            //     // configure DData accordingly
            //     options.Durable = new DurableOptions()
            //     {
            //         // disable durable storage for this actor
            //         Keys = []
            //     };
            //     options.RecreateOnFailure = true;
            // })
            .WithDistributedData(new DDataOptions())
            .WithActors((actorSystem, registry) =>
            {
                var promptManager = actorSystem.ActorOf(PromptManager.PropsFor(), "Prompt-manager");
                registry.Register<PromptManager>(promptManager);
                var promptSagaManager = actorSystem.ActorOf(PromptSagaManager.PropsFor(() => new PromptSaga(PromptSagaConfig)), "Prompt-saga-manager");
                registry.Register<PromptSagaManager>(promptSagaManager);
                var allPromptsActor = actorSystem.ActorOf(AllPromptsActor.PropsFor(), "all-Prompts-actor");
                registry.Register<AllPromptsActor>(allPromptsActor);
            });
        });
        return services;
    }
}

/// <summary>
/// Binds to the "Akka" configuration object
/// </summary>
public class CrowdQueryAkkaConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    internal bool IsInvalid()
    {
        return string.IsNullOrEmpty(ConnectionString);
    }
}
