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
        private ILoggingAdapter _log;
        private readonly IActorRef _projectorShard;
        public PromptProjectorManager(IActorRef projectorShard)
        {
            _projectorShard = projectorShard;
            _log = Context.GetLogger();
            var mediator = DistributedPubSub.Get(Context.System).Mediator;
            mediator.Tell(new Subscribe(ProjectionConstants.PromptCreated, Self, ProjectionConstants.GroupId));
            mediator.Tell(new Subscribe(ProjectionConstants.AnswerVoteIncreased, Self, ProjectionConstants.GroupId));
            mediator.Tell(new Subscribe(ProjectionConstants.AnswerVoteDecreased, Self, ProjectionConstants.GroupId));
            Receive<ProjectedEvent<PromptCreated, PromptId>>(Forward);
            Receive<ProjectedEvent<AnswerVoteIncreased, PromptId>>(Forward);
            Receive<ProjectedEvent<AnswerVoteDecreased, PromptId>>(Forward);
            Receive<SubscribeAck>(msg => _log.Info($"Successfully Subscribed to {msg.Subscribe.Topic}"));
        }

        public void Forward(object msg)
        {
            _log.Debug($"Received message type {msg.GetType()}");
            _projectorShard.Forward(msg);
        }

        public static Props PropsFor(IActorRef projectorShard)
        {
            return Props.Create(() => new PromptProjectorManager(projectorShard));
        }
    }
}