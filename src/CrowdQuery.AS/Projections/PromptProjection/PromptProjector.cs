using System.Reactive.Linq;
using Akka.Actor;
using Akka.Cluster;
using Akka.DistributedData;
using Akka.Event;
using Akka.Logger.Serilog;
using Akka.Persistence;
using Akka.Streams;
using Akka.Streams.Dsl;
using CrowdQuery.AS.Actors.Prompt;
using CrowdQuery.AS.Actors.Prompt.Events;
using CrowdQuery.AS.Projections.BasicPromptStateProjection;

namespace CrowdQuery.AS.Projections.PromptProjection
{
    public record State(string Prompt, Dictionary<string, int> Answers, long LastSequenceNumber){ }

    public class PromptProjector : ReceivePersistentActor
    {
        private readonly string _persistenceId;
        public override string PersistenceId => _persistenceId;
        private State _state = new(string.Empty, new Dictionary<string, int>(), -1);
        private readonly HashSet<IActorRef> _subscribers = new HashSet<IActorRef>();
        private IActorRef _debouncer = ActorRefs.Nobody;
        private readonly Configuration _config;
        private ILoggingAdapter _logger => Context.GetLogger();
        private IActorRef _replicator = ActorRefs.Nobody;
        public PromptProjector(string persistenceId, Configuration config)
        {
            _config = config;
            _persistenceId = persistenceId;
            _replicator = DistributedData.Get(Context.System).Replicator;
            Command<ProjectedEvent<PromptCreated, PromptId>>(msg =>
            {
                _logger.Info($"Received Domain Event PromptCreated for {msg.AggregateId}");
                var evnt = new ProjectionCreated(msg.AggregateEvent.Prompt, msg.AggregateEvent.Answers.ToDictionary(x => x, y => 0));
                Persist(evnt, Handle);
                DeferAsync(evnt, Defer);
            });
            Command<ProjectedEvent<AnswerVoteIncreased, PromptId>>(msg =>
            {
                _logger.Info($"Received Domain Event AnswerVoteIncreased for {msg.AggregateId}");
                var evnt = new ProjectionAnswerIncreased(msg.AggregateEvent.Answer, msg.SequenceNumber);
                Persist(evnt, Handle);
                DeferAsync(evnt, Defer);
            });
            Command<ProjectedEvent<AnswerVoteDecreased, PromptId>>(msg =>
            {
                _logger.Info($"Received Domain Event AnswerVoteDecreased for {msg.AggregateId}");
                var evnt = new ProjectionAnswerDecreased(msg.AggregateEvent.Answer, msg.SequenceNumber);
                Persist(evnt, Handle);
                DeferAsync(evnt, Defer);
            });
            Command<AddSubscriber>(msg =>
            {
                _logger.Info($"Adding new subscriber: {msg.Subscriber.Path}");
                _subscribers.Add(msg.Subscriber);
                Context.WatchWith(msg.Subscriber, new RemoveSubscriber(msg.Subscriber, _persistenceId));
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
            Recover<ProjectionCreated>(Handle);
            Recover<ProjectionAnswerIncreased>(Handle);
            Recover<ProjectionAnswerDecreased>(Handle);
            Command<SoftStop>(msg => _logger.Debug("Received SoftStop"));
        }
        private void Defer(IProjectorEvent evnt)
        {
            _logger.Debug($"PromptProjector-Defer: {_subscribers.Count}");
            _debouncer.Tell(UpdateSubscribers.Instance);
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

        public override void AroundPostStop()
        {
            _logger.Debug($"Around PostStop");
            base.AroundPostStop();
        }

        protected override void PostRestart(Exception reason)
        {
            _logger.Debug("PostRestart");
            base.PostRestart(reason);
        }

        protected override void OnPersistFailure(Exception cause, object @event, long sequenceNr)
        {
            _logger.Debug("OnPersistFailure");
            base.OnPersistFailure(cause, @event, sequenceNr);
        }

        protected override void OnRecoveryFailure(Exception reason, object? message = null)
        {
            _logger.Debug("OnRecoveryFaiture");
            base.OnRecoveryFailure(reason, message);
        }

        protected override bool AroundReceive(Receive receive, object message)
        {
            _logger.Debug($"PromptProjector received message: {message.GetType()}");
            return base.AroundReceive(receive, message);
        }

        public static Props PropsFor(string persistenceId, Configuration? config = null)
        {
            config ??= new Configuration();
            return Props.Create(() => new PromptProjector(persistenceId, config));
        }

        public override void AroundPreStart()
        {
            var (actorRef, src) = Source.ActorRef<UpdateSubscribers>(0, OverflowStrategy.DropHead)
                .PreMaterialize(Context.System);
            _debouncer = actorRef;

            var source = src.Conflate((currentValue, newValue) => newValue)
                .Delay(TimeSpan.FromMilliseconds(_config.DebouceTimerMilliseconds))
                .RunForeach(x => NotifySubscribers(x), Context.System);

            base.AroundPreStart();
        }

        private void NotifySubscribers(UpdateSubscribers _)
        {
            _logger.Debug("NotifySubscribers");
            foreach (var subscriber in _subscribers)
            {
                subscriber.Tell(_state);
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
                _state.Answers[evnt.Answer]++;
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
                _state.Answers[evnt.Answer]--;
                _state = _state with {LastSequenceNumber = evnt.PromptSequenceNumber};
            }
            else
            {
                _logger.Warning($"Received sequenceNumber {evnt.PromptSequenceNumber} but lastSequenceNumber is {_state.LastSequenceNumber}");
            }
        }
    }

    public static class PromptProjectorExtensions
    {
        public static string ToPromptProjectorId(this PromptId input) => $"projector-{input.Value}";
        internal static PromptId ToPromptId(this string input) => PromptId.With(input.Replace("projector-", ""));
    }

    public class SoftStop() {}

}