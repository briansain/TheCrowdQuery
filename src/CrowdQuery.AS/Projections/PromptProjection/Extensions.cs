using System;
using Akka.Cluster.Hosting;
using Akka.Hosting;
using static CrowdQuery.AS.ServiceCollectionExtension;

namespace CrowdQuery.AS.Projections.PromptProjection;

public static class Extensions
{
    public static AkkaConfigurationBuilder AddPromptProjector(this AkkaConfigurationBuilder builder, Configuration config, string journalPluginId, string snapshotPluginId)
    {
        return builder
            .WithShardRegion<PromptProjector>(
                typeof(PromptProjector).Name,
                persistenceId => PromptProjector.PropsFor(persistenceId, config),
                new PromptProjectorMessageExtractor(100),
                new ShardOptions()
                {
                    JournalPluginId = journalPluginId,
                    SnapshotPluginId = snapshotPluginId,
                    Role = ClusterConstants.ProjectionNode
                }
            );
    }

}
