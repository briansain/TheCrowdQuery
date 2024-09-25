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
        public static string PromptCreated = "PromptCreated";
        public static string AnswerVoteIncreased = "AnswerVoteIncreased";
        public static string AnswerVoteDecreased = "AnswerVoteDecreased";
        public static string GroupId = "ProjectorGroup";
        private readonly IActorRef _promptShard;
        public PromptProjectorManager(IActorRef promptShard)
        {
            _promptShard = promptShard;
            
            var mediator = DistributedPubSub.Get(Context.System).Mediator;
            mediator.Tell(new Subscribe(PromptCreated, Self, GroupId));
            mediator.Tell(new Subscribe(AnswerVoteIncreased, Self, GroupId));
            mediator.Tell(new Subscribe(AnswerVoteDecreased, Self, GroupId));
            Receive<IDomainEvent<PromptActor, PromptId, PromptCreated>>(evnt => _promptShard.Forward(evnt));
            Receive<IDomainEvent<PromptActor, PromptId, AnswerVoteIncreased>>(evnt => _promptShard.Forward(evnt));
            Receive<IDomainEvent<PromptActor, PromptId, AnswerVoteDecreased>>(evnt => _promptShard.Forward(evnt));
            Receive<SubscribeAck>(msg => Context.GetLogger().Info($"Successfully Subscribed to {msg.Subscribe.Topic}"));
        }

        public static Props PropsFor(IActorRef promptShard)
        {
            return Props.Create(() => new PromptProjectorManager(promptShard));
        }
    }
}