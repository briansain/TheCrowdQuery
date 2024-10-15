using Akka.Cluster.Sharding;
using CrowdQuery.AS.Actors.Prompt;
using CrowdQuery.AS.Actors.Prompt.Events;

namespace CrowdQuery.AS.Projections.PromptProjection
{
    public record Rebuild(long SequenceNumberTo);
    public record RebuildComplete();
    public record RebuildFailed(Exception e);

    public class PromptProjectorMessageExtractor : HashCodeMessageExtractor
    {
        public PromptProjectorMessageExtractor(int maxNumberOfShards) : base(maxNumberOfShards)
        {
        }

        public override string? EntityId(object message)
        {
            switch(message)
            {
                case ProjectedEvent<PromptCreated, PromptId> prompt:
                    return prompt.AggregateId.ToPromptProjectorId();
                case ProjectedEvent<AnswerVoteIncreased, PromptId> increased:
                    return increased.AggregateId.ToPromptProjectorId();
                case ProjectedEvent<AnswerVoteDecreased, PromptId> decreased:
                    return decreased.AggregateId.ToPromptProjectorId();
                case AddSubscriber addSubscriber:
                    return addSubscriber.ProjectorId;
                case RemoveSubscriber removeSubscriber:
                    return removeSubscriber.ProjectorId;
            }

            throw new Exception($"Could not get EntityId from type {message.GetType()}");
        }
    }
}