using System.Collections.Generic;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.DistributedData;
using Akka.Event;
using Akka.Persistence;
using Akka.Streams;
using Akka.Streams.Dsl;
namespace CrowdQuery.AS.Projections.BasicPromptStateProjection
{
    public class BasicPromptStateProjector : ReceiveActor
    {
        public static LWWDictionaryKey<string, BasicPromptState> Key = new("BasicPromptState_Key");
        private readonly HashSet<IActorRef> _subscribers = new HashSet<IActorRef>();
        private LWWDictionary<string, BasicPromptState> _state = LWWDictionary<string, BasicPromptState>.Empty;
        private IActorRef debouncer = ActorRefs.Nobody;
        private ILoggingAdapter _logger => Context.GetLogger();
        private readonly BasicPromptStateConfiguration _config;
        public BasicPromptStateProjector(BasicPromptStateConfiguration configuration)
        {
            _config = configuration;
            // Receive<AddSubscriber>(msg => 
            // {
            //     _subscribers.Add(msg.Subscriber);
            //     msg.Subscriber.Tell(_state.ToImmutableDictionary());
            // });
            Receive<IGetResponse>(msg =>
            {
                switch(msg)
                {
                    case GetSuccess success: _state = _state.Merge(success.Get(Key));
                        break;
                    case NotFound notFound: _logger.Debug($"Could not find key {notFound.Key}");
                        break;
                    case GetFailure failure: _logger.Warning($"Failed to get key {failure.Key}");
                        break;
                    case DataDeleted deleted: _logger.Warning($"Received DataDeleted for {deleted.Key}");
                        break;
                }
            });
            Receive<Changed>(msg =>
            {
                _state = _state.Merge(msg.Get(Key));
            });
            Receive<GetBasicPromptState>(msg => 
            {
                Sender.Tell(_state.ToDictionary());
            });
        }

        private void NotifySubscribers(UpdateSubscribers state)
        {
            var comms = _state.ToDictionary();
            foreach (var subscriber in _subscribers)
            {
                subscriber.Tell(comms);
            }
        }

        public static Props PropsFor(BasicPromptStateConfiguration config)
        {
            return Props.Create(() => new BasicPromptStateProjector(config));
        }

        public override void AroundPreStart()
        {
            var replicator = DistributedData.Get(Context.System).Replicator;
            replicator.Tell(Dsl.Subscribe(Key, Self));
            replicator.Tell(Dsl.Get(Key, ReadLocal.Instance));

            var (actorRef, src) = Source.ActorRef<UpdateSubscribers>(0, OverflowStrategy.DropHead)
                .PreMaterialize(Context.System);
            debouncer = actorRef;

            var source = src.Conflate((currentValue, newValue) => newValue)
                .Delay(TimeSpan.FromMilliseconds(_config.DebouceTimerMilliseconds))
                .RunForeach(NotifySubscribers, Context.System);

            base.AroundPreStart();
        }
    }

    public class BasicPromptStateConfiguration
    {
        public int DebouceTimerMilliseconds { get; set; }
        public BasicPromptStateConfiguration(int debounceTimerMilliseconds = 5000)
        {
            DebouceTimerMilliseconds = debounceTimerMilliseconds;
        }
    }

    public class GetBasicPromptState {}
	public record BasicPromptState(string prompt, int answerCount, int totalVotes) {}
}