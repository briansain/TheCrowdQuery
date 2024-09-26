using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using Akkatecture.Aggregates;
using CrowdQuery.AS.Actors.Prompt;
using CrowdQuery.AS.Actors.Prompt.Events;

namespace CrowdQuery.AS.Projections.PromptProjection
{
    public class PromptProjectorManager : ReceiveActor
    {

        private readonly IActorRef _projectorShard;
        public PromptProjectorManager(IActorRef projectorShard)
        {
            _projectorShard = projectorShard;
            
            var mediator = DistributedPubSub.Get(Context.System).Mediator;
            mediator.Tell(new Subscribe(ProjectionConstants.PromptCreated, Self, ProjectionConstants.GroupId));
            mediator.Tell(new Subscribe(ProjectionConstants.AnswerVoteIncreased, Self, ProjectionConstants.GroupId));
            mediator.Tell(new Subscribe(ProjectionConstants.AnswerVoteDecreased, Self, ProjectionConstants.GroupId));
            Receive<ProjectedEvent<PromptCreated, PromptId>>(evnt => _projectorShard.Forward(evnt));
            Receive<ProjectedEvent<AnswerVoteIncreased, PromptId>>(evnt => _projectorShard.Forward(evnt));
            Receive<ProjectedEvent<AnswerVoteDecreased, PromptId>>(evnt => _projectorShard.Forward(evnt));
            Receive<SubscribeAck>(msg => Context.GetLogger().Info($"Successfully Subscribed to {msg.Subscribe.Topic}"));
        }

        public static Props PropsFor(IActorRef projectorShard)
        {
            return Props.Create(() => new PromptProjectorManager(projectorShard));
        }
    }
}