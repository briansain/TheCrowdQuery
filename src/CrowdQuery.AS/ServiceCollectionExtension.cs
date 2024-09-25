using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Cluster.Sharding;
using Akka.Event;
using Akka.Hosting;
using Akka.Logger.Serilog;
using Akka.Persistence.Sql.Hosting;
using Akka.Remote.Hosting;
using Akkatecture.Clustering;
using Akkatecture.Clustering.Core;
using CrowdQuery.AS.Actors.Prompt;
using CrowdQuery.AS.Projections.BasicPromptStateProjection;
using CrowdQuery.AS.Projections.PromptProjection;
using LinqToDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdQuery.AS;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddCrowdQueryAkka(this IServiceCollection services, IConfiguration configuration, string[] roles)
    {
        var config = new CrowdQueryAkkaConfiguration();
        configuration.Bind("Akka", config);

        var promptProjectionConfiguration = new PromptProjectionConfiguration();
        configuration.Bind("CrowdQuery:PromptProjection", promptProjectionConfiguration);
        services.AddSingleton(promptProjectionConfiguration);

        var promptBasicStateProjectorConfiguration = new BasicPromptStateConfiguration();
        configuration.Bind("CrowdQuery:BasicStateProjector", promptBasicStateProjectorConfiguration);
        services.AddSingleton(promptBasicStateProjectorConfiguration);

        if (config.IsInvalid())
        {
            throw new ArgumentException("Must provide a valid 'Akka' section config");
        }

        services.AddAkka("crowd-query", builder =>
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
                Roles = roles,
                SeedNodes = ["akka.tcp://crowd-query@localhost:5053"]
            })
            .WithDistributedData(new DDataOptions()
            {
                RecreateOnFailure = true,
                Durable = new DurableOptions()
                {
                    Keys = []
                }

            })
            .WithActors((actorSystem, registry) =>
            {
                var clusterSharding = ClusterSharding.Get(actorSystem);
                var promptManagerShard = clusterSharding.Start(
                    typeof(PromptManager).Name,
                    Props.Create(() => new ClusterParentProxy(PromptManager.PropsFor(), false)),
                    clusterSharding.Settings.WithRole(ClusterConstants.MainNode),
                    new MessageExtractor<PromptActor, PromptId>(100));
                registry.Register<PromptManager>(promptManagerShard);

                IActorRef promptProjectorShard = ActorRefs.Nobody;
                if (roles.Contains(ClusterConstants.ProjectionNode))
                {
                    promptProjectorShard = clusterSharding.Start(
                        typeof(PromptProjector).Name,
                        persistenceId => PromptProjector.PropsFor(persistenceId, promptProjectionConfiguration),
                        clusterSharding.Settings.WithRole(ClusterConstants.ProjectionNode),
                        new PromptProjectorMessageExtractor(100)
                    );

                    var promptProjectorManager = actorSystem.ActorOf(PromptProjectorManager.PropsFor(promptProjectorShard), "projection-manager");
                    registry.Register<PromptProjectorManager>(promptProjectorManager);
                }
                else
                {
                    promptProjectorShard = clusterSharding.StartProxy(
                        typeof(PromptProjector).Name,
                        ClusterConstants.ProjectionProxyNode,
                        new PromptProjectorMessageExtractor(100)
                    );
                }
                registry.Register<PromptProjector>(promptProjectorShard);

                var promptBasicStateProjector = actorSystem.ActorOf(BasicPromptStateProjector.PropsFor(promptBasicStateProjectorConfiguration), "basic-prompt-projector");
                registry.Register<BasicPromptStateProjector>(promptBasicStateProjector);
            });
        });
        return services;
    }

    public static class ClusterConstants
    {
        public static readonly string MainNode = "main-node";
        public static readonly string ProjectionNode = "projection-node";
        public static readonly string ProjectionProxyNode = "projection-proxy-node";
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
