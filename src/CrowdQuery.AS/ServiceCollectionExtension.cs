using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using Akka.Event;
using Akka.Hosting;
using Akka.Logger.Serilog;
using Akka.Persistence.Sql.Config;
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

        var promptProjectionConfiguration = new Projections.PromptProjection.Configuration();
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
            builder
            .AddHocon(ConfigurationFactory.ParseString("akka.cluster.sharding.verbose-debug-logging=true"), HoconAddMode.Append)
            .ConfigureLoggers(configLoggers =>
            {
                configLoggers.LogLevel = LogLevel.DebugLevel;
                configLoggers.LogConfigOnStart = true;
                configLoggers.ClearLoggers();
                configLoggers.AddLogger<SerilogLogger>();
            })
            .WithSqlPersistence(config.ConnectionString, ProviderName.PostgreSQL)
            .WithSqlPersistence(journalOptions =>
            {
                journalOptions.ProviderName = ProviderName.PostgreSQL;
                journalOptions.ConnectionString = config.ConnectionString;
                journalOptions.Identifier = "sharding-journal";
                journalOptions.DatabaseOptions = new JournalDatabaseOptions(DatabaseMapping.PostgreSql);
                journalOptions.DatabaseOptions.JournalTable = JournalTableOptions.PostgreSql;
                journalOptions.DatabaseOptions.JournalTable.TableName = "ShardingEventJournal";
                journalOptions.TagStorageMode = TagMode.TagTable;
            },
            snapshotOptions =>
            {
                snapshotOptions.ProviderName = ProviderName.PostgreSQL;
                snapshotOptions.ConnectionString = config.ConnectionString;
                snapshotOptions.Identifier = "sharding-snapshot";
                snapshotOptions.DatabaseOptions = new SnapshotDatabaseOptions(DatabaseMapping.PostgreSql);
                snapshotOptions.DatabaseOptions.SnapshotTable = SnapshotTableOptions.PostgreSql;
                snapshotOptions.DatabaseOptions.SnapshotTable.TableName = "ShardingSnapshotStore";
            }, false)
            .WithRemoting("localhost", 5110)
            .WithClustering(new ClusterOptions()
            {
                Roles = roles,
                SeedNodes = ["akka.tcp://crowd-query@localhost:5110"]
            })
            .WithDistributedData(new DDataOptions()
            {
                RecreateOnFailure = true,
                Durable = new DurableOptions()
                {
                    Keys = [BasicPromptStateProjector.Key]
                }

            })
            .WithShardRegion<PromptProjector>(
                typeof(PromptProjector).Name,
                persistenceId => PromptProjector.PropsFor(persistenceId, promptProjectionConfiguration),
                new PromptProjectorMessageExtractor(100),
                new ShardOptions()
                {
                    JournalPluginId = "akka.persistence.journal.sharding-journal",
                    SnapshotPluginId = "akka.persistence.snapshot-store.sharding-snapshot",
                    Role = ClusterConstants.ProjectionNode,
                    PassivateIdleEntityAfter = TimeSpan.FromMinutes(5)
                    // HandOffStopMessage = new SoftStop()
                }
            )
            .WithShardRegion<PromptActor>(
                typeof(PromptActor).Name, 
                persistenceId => PromptActor.PropsFor(persistenceId),
                new MessageExtractor<PromptActor, PromptId>(100),
                new ShardOptions()
                {
                    JournalPluginId = "akka.persistence.journal.sharding-journal",
                    SnapshotPluginId = "akka.persistence.snapshot-store.sharding-snapshot",
                    Role = ClusterConstants.ProjectionNode

                }
            )
            .WithActors((actorSystem, registry) =>
            {
                // var clusterSharding = ClusterSharding.Get(actorSystem);
                // var promptManagerShard = clusterSharding.Start(
                //     typeof(PromptManager).Name,
                //     Props.Create(() => new ClusterParentProxy(PromptManager.PropsFor(), false)),
                //     clusterSharding.Settings.WithRole(ClusterConstants.MainNode),
                //     new MessageExtractor<PromptActor, PromptId>(100));
                // registry.Register<PromptManager>(promptManagerShard);

                var projectorShard = registry.Get<PromptProjector>();
                var promptProjectorManager = actorSystem.ActorOf(PromptProjectorManager.PropsFor(projectorShard), "projection-manager");
                registry.Register<PromptProjectorManager>(promptProjectorManager);

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
