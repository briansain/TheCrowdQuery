using System.Reactive.Linq;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Sharding;
using Akka.DistributedData;
using Akka.Event;
using Akka.Logger.Serilog;
using Akka.Persistence;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akkatecture.Aggregates;
using CrowdQuery.AS.Actors.Prompt;
using CrowdQuery.AS.Actors.Prompt.Events;
using CrowdQuery.AS.Projections.BasicPromptStateProjection;
using CrowdQuery.Messages;

namespace CrowdQuery.AS.Projections.PromptProjection
{
    public class PromptProjector : ReceivePersistentActor
    {
        private readonly string _persistenceId;
        public override string PersistenceId => _persistenceId;
        private PromptProjectionState _state = new(string.Empty, new Dictionary<string, int>(), -1);
        private readonly HashSet<IActorRef> _subscribers = new HashSet<IActorRef>();
        private IActorRef _debouncer = ActorRefs.Nobody;
        private readonly PromptProjectionConfiguration _config;
        private ILoggingAdapter _logger => Context.GetLogger();
        private IActorRef _replicator = ActorRefs.Nobody;
        public PromptProjector(string persistenceId, PromptProjectionConfiguration config)
        {
            _config = config;
            _persistenceId = persistenceId;
            _replicator = DistributedData.Get(Context.System).Replicator;
            // Become(Project);
            Command<IDomainEvent<PromptActor, PromptId, PromptCreated>>(msg =>
            {
                _logger.Info($"Received Domain Event PromptCreated for {msg.AggregateIdentity}");
                var evnt = new ProjectionCreated(msg.AggregateEvent.Prompt, msg.AggregateEvent.Answers.ToDictionary(x => x, y => 0));
                Persist(evnt, Handle);
                DeferAsync(evnt, Defer);
            });
            Command<IDomainEvent<PromptActor, PromptId, AnswerVoteIncreased>>(msg =>
            {
                _logger.Info($"Received Domain Event AnswerVoteIncreased for {msg.AggregateIdentity}");
                var evnt = new ProjectionAnswerIncreased(msg.AggregateEvent.Answer, msg.AggregateSequenceNumber);
                Persist(evnt, Handle);
                DeferAsync(evnt, Defer);
            });
            Command<IDomainEvent<PromptActor, PromptId, AnswerVoteDecreased>>(msg =>
            {
                _logger.Info($"Received Domain Event AnswerVoteDecreased for {msg.AggregateIdentity}");
                var evnt = new ProjectionAnswerDecreased(msg.AggregateEvent.Answer, msg.AggregateSequenceNumber);
                Persist(evnt, Handle);
                DeferAsync(evnt, Defer);
            });
            Command<AddSubscriber>(msg =>
            {
                _logger.Info($"Adding new subscriber: {msg.Subscriber.Path}");
                _subscribers.Add(msg.Subscriber);
                Context.WatchWith(msg.Subscriber, new RemoveSubscriber(msg.Subscriber));
                msg.Subscriber.Tell(_state);
            });
            Command<RemoveSubscriber>(msg =>
            {
                _logger.Info($"Removing new subscriber: {msg.Subscriber.Path}");
                _subscribers.Remove(msg.Subscriber);
                Context.Unwatch(msg.Subscriber);
            });
            Command<IUpdateResponse>(msg => 
            {
                switch(msg)
                {
                    case UpdateSuccess success: _logger.Debug("Successfully updated DistributedData state");
                        break;
                    case ModifyFailure failure: _logger.Warning($"Failed to update DistributedData state: {failure.ErrorMessage}");
                        break;
                    case UpdateTimeout timeout: _logger.Info($"Received UpdateTimeout");
                        break;
                    case DataDeleted deleted: _logger.Warning("Received DataDeleted when updating DistributedData");
                        break;
                }
            });
            // Command<Rebuild>(msg => {
            //     Become(Rebuilding);
            //     Self.Tell(msg);
            // });
        }
        private void Defer(IProjectorEvent evnt)
        {
            _debouncer.Tell(_state);
            _replicator.Tell(Dsl.Update(
                BasicPromptStateProjector.Key,
                LWWDictionary<string, BasicPromptState>.Empty,
                WriteLocal.Instance,
                oldDictionary => 
                {
                    var key = _persistenceId.ToPromptId().ToBase64();
                    return oldDictionary.SetItem(Cluster.Get(Context.System), key, new BasicPromptState(_state.Prompt, _state.Answers.Count, _state.Answers.Values.Sum()));
                }
            ));
        }
        
        public static Props PropsFor(string persistenceId, PromptProjectionConfiguration config)
        {
            return Props.Create(() => new PromptProjector(persistenceId, config));
        }

        public override void AroundPreStart()
        {
            var (actorRef, src) = Source.ActorRef<PromptProjectionState>(0, OverflowStrategy.DropHead)
                .PreMaterialize(Context.System);
            _debouncer = actorRef;

            var source = src.Conflate((currentValue, newValue) => newValue)
                .Delay(TimeSpan.FromMilliseconds(_config.DebouceTimerMilliseconds))
                .RunForeach(x => NotifySubscribers(x), Context.System);

            base.AroundPreStart();
        }

        public void NotifySubscribers(PromptProjectionState state)
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber.Tell(state);
            }
        }

        public void Handle(ProjectionCreated evnt)
        {
            _state = new(evnt.Prompt, evnt.Answers, 1);
        }

        public void Handle(ProjectionAnswerIncreased evnt)
        {
            if (evnt.PromptSequenceNumber > _state.LastSequenceNumber)
            {
            _state.Answers[evnt.Answer] = _state.Answers[evnt.Answer]++;
                _state = _state with {LastSequenceNumber = evnt.PromptSequenceNumber};
            }
            else
            {
                _logger.Warning($"Received sequenceNumber {evnt.PromptSequenceNumber} but lastSequenceNumber is {_state.LastSequenceNumber}");
            }
        }

        public void Handle(ProjectionAnswerDecreased evnt)
        {
            if (evnt.PromptSequenceNumber > _state.LastSequenceNumber)
            {
                _state.Answers[evnt.Answer] = _state.Answers[evnt.Answer]--;
                _state = _state with {LastSequenceNumber = evnt.PromptSequenceNumber};
            }
            else
            {
                _logger.Warning($"Received sequenceNumber {evnt.PromptSequenceNumber} but lastSequenceNumber is {_state.LastSequenceNumber}");
            }
        }

        // public void Rebuilding()
        // {
        //     Command<Rebuild>(msg => {
        //         var source = PersistenceQuery.Get(Context.System)
        //             .ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier)
        //             .CurrentEventsByPersistenceId(_persistenceId.Replace("projector-", ""), 0, msg.SequenceNumberTo);
        //         source.RunForeach(eventEnvelope =>
        //         {
        //                 if (eventEnvelope.Event is IDomainEvent<PromptActor, PromptId, PromptCreated> promptCreated)
        //                 {
        //                     Handle(new ProjectionCreated(promptCreated.AggregateEvent.Prompt, promptCreated.AggregateEvent.Answers.ToDictionary(x => x, y => 0)));
        //                 }
        //                 else if (eventEnvelope.Event is IDomainEvent<PromptActor, PromptId, AnswerVoteDecreased> voteDecreased)
        //                 {
        //                     Handle(new ProjectionAnswerDecreased(voteDecreased.AggregateEvent.Answer, voteDecreased.AggregateSequenceNumber));
        //                 }
        //                 else if (eventEnvelope.Event is IDomainEvent<PromptActor, PromptId, AnswerVoteIncreased> voteIncreased)
        //                 {
        //                     Handle(new ProjectionAnswerIncreased(voteIncreased.AggregateEvent.Answer, voteIncreased.AggregateSequenceNumber));
        //                 }
        //                 throw new Exception($"Could not map event {eventEnvelope.Event.GetType()} for Prompt Projector");
        //         }, Context.System).PipeTo(Self, ActorRefs.Nobody, success: () => new RebuildComplete(), failure: e => new RebuildFailed(e));
        //     });
        //     Command<RebuildComplete>(_ => {
        //         // need to delete current EJ
        //         // need to hard reset state
        //         // need to save snapshot
        //     });
        //     Command<RebuildFailed>(_ => {
        //         // log failure
        //     });
        // }
    }

    public static class PromptProjectorExtensions
    {
        public static string ToPromptProjectorId(this PromptId input) => $"projector-{input.Value}";
        internal static PromptId ToPromptId(this string input) => PromptId.With(input.Replace("projector-", ""));
    }

    public record PromptProjectionState(string Prompt, Dictionary<string, int> Answers, long LastSequenceNumber){ }
    public record Rebuild(long SequenceNumberTo);
    public record RebuildComplete();
    public record RebuildFailed(Exception e);
    public class PromptProjectionConfiguration
    {
        public int DebouceTimerMilliseconds { get; set; }
        public PromptProjectionConfiguration(int debounceTimerSeconds = 5000)
        {
            DebouceTimerMilliseconds = debounceTimerSeconds;
        }
    }

    public class PromptProjectorMessageExtractor : HashCodeMessageExtractor
    {
        public PromptProjectorMessageExtractor(int maxNumberOfShards) : base(maxNumberOfShards)
        {
        }

        public override string? EntityId(object message)
        {
            if (message is IDomainEvent<PromptActor, PromptId> prompt)
            {
                return prompt.AggregateIdentity.ToPromptProjectorId();
            }

            throw new Exception($"Could not get EntityId from type {message.GetType()}");
        }
    }
}