using Akka.Actor;
using Akka.Cluster;
using Akka.DistributedData;
using Akka.Event;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akkatecture.Aggregates;
using Akkatecture.Sagas;
using Akkatecture.Sagas.AggregateSaga;
using CrowdQuery.AS.Actors;
using CrowdQuery.AS.Actors.Prompt;
using CrowdQuery.AS.Actors.Prompt.Events;
using CrowdQuery.AS.Sagas.PromptSaga.Commands;
using CrowdQuery.AS.Sagas.PromptSaga.Events;
using CrowdQuery.AS.Sagas.PromptSaga.ResponseModels;
using CrowdQuery.Messages;
using AnswerVoteDecreased = CrowdQuery.AS.Actors.Prompt.Events.AnswerVoteDecreased;
using AnswerVoteIncreased = CrowdQuery.AS.Actors.Prompt.Events.AnswerVoteIncreased;

namespace CrowdQuery.AS.Sagas.PromptSaga
{
	public class PromptSaga : AggregateSaga<PromptSaga, PromptSagaId, PromptSagaState>,
		ISagaIsStartedBy<PromptActor, PromptId, PromptCreated>,
		ISagaHandles<PromptActor, PromptId, AnswerVoteIncreased>,
		ISagaHandles<PromptActor, PromptId, AnswerVoteDecreased>
	{
		private ILoggingAdapter _loggingAdapter => Context.GetLogger();
		public HashSet<IActorRef> Subscribers = new HashSet<IActorRef>();
		private IActorRef _debouncer = ActorRefs.Nobody;
		private PromptSagaConfiguration _config;
		public PromptSaga() : this(new PromptSagaConfiguration()) { }
		private readonly IWriteConsistency _writeConsistency = new WriteAll(TimeSpan.FromSeconds(1));
		private readonly Cluster _cluster = Cluster.Get(Context.System);

		public PromptSaga(PromptSagaConfiguration config)
		{
			_config = config;
			Command<SubscriberTerminated>(command =>
			{
				if (Subscribers.TryGetValue(command.Subscriber, out var sub))
				{
					Subscribers.Remove(sub);
				}
			});
			Command<SubscribeToPrompt>(command =>
			{
				Subscribers.Add(command.Subscriber);
				Context.WatchWith(command.Subscriber, new SubscriberTerminated(command.Subscriber));
				Sender.Tell(new PromptStateUpdated(State.Prompt, new Dictionary<string, long>(State.Answers), Subscribers.Count));
				return true;
			});
		}

		public override void AroundPreStart()
		{
			var cancellationToken = new CancellationTokenSource();
			var (actorRef, src) = Source.ActorRef<PromptStateUpdated>(0, OverflowStrategy.DropHead)
				.PreMaterialize(Context.System);
			_debouncer = actorRef;

			var source = src.Conflate((currentValue, newValue) => newValue)
				.Delay(TimeSpan.FromMilliseconds(_config.DebouceTimerMilliseconds))
				.RunForeach(x => TellSubscribersStateChange(x), Context.System);

			base.AroundPreStart();
		}

		public bool Handle(IDomainEvent<PromptActor, PromptId, PromptCreated> domainEvent)
		{

			var evnt = new PromptSagaCreated(domainEvent.AggregateEvent.Prompt, domainEvent.AggregateEvent.Answers);
			Emit(evnt);
			var replicator = DistributedData.Get(Context.System).Replicator;

			replicator.Tell(Dsl.Update(
				AllPromptsActor.AllPromptsBasicKey,
				LWWDictionary<string, BasicPromptState>.Empty,
				_writeConsistency,
				state =>
					state.SetItem(_cluster, domainEvent.AggregateIdentity.Value, new BasicPromptState(domainEvent.AggregateIdentity.Value, domainEvent.AggregateEvent.Prompt, 0, 0))
			));

			var updatedState = new PromptStateUpdated(evnt.Prompt, evnt.Answers.ToDictionary(x => x, y => (long)0), Subscribers.Count);
			_debouncer.Tell(updatedState);
			return true;
		}
		public bool Handle(IDomainEvent<PromptActor, PromptId, AnswerVoteIncreased> domainEvent)
		{
			var evnt = new Events.AnswerVoteIncreased(domainEvent.AggregateEvent.Answer);
			var updatedState = new PromptStateUpdated(State.Prompt, new Dictionary<string, long>(State.Answers), Subscribers.Count);
			updatedState.Answers[evnt.Answer]++;
			_debouncer.Tell(updatedState);
			Emit(evnt);
			return true;
		}

		public bool Handle(IDomainEvent<PromptActor, PromptId, AnswerVoteDecreased> domainEvent)
		{
			var evnt = new Events.AnswerVoteDecreased(domainEvent.AggregateEvent.Answer);
			Emit(evnt);
			var updatedState = new PromptStateUpdated(State.Prompt, new Dictionary<string, long>(State.Answers), Subscribers.Count);
			updatedState.Answers[evnt.Answer]--;
			_debouncer.Tell(updatedState);
			return true;
		}

		private void TellSubscribersStateChange(PromptStateUpdated state)
		{
			_loggingAdapter.Debug($"Telling {Subscribers.Count} subscriber(s) of state change");
			foreach (var subscriber in Subscribers)
			{
				subscriber.Tell(state);
			}
		}
	}
}
