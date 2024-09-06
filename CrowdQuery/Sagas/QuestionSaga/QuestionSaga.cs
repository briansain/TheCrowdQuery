using Akka.Actor;
using Akka.Event;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akkatecture.Aggregates;
using Akkatecture.Sagas;
using Akkatecture.Sagas.AggregateSaga;
using CrowdQuery.Actors.Question;
using CrowdQuery.Actors.Question.Events;
using CrowdQuery.Sagas.QuestionSaga.Commands;
using CrowdQuery.Sagas.QuestionSaga.Events;
using CrowdQuery.Sagas.QuestionSaga.ResponseModels;
using AnswerVoteDecreased = CrowdQuery.Actors.Question.Events.AnswerVoteDecreased;
using AnswerVoteIncreased = CrowdQuery.Actors.Question.Events.AnswerVoteIncreased;

namespace CrowdQuery.Sagas.QuestionSaga
{
	public class QuestionSaga : AggregateSaga<QuestionSaga, QuestionSagaId, QuestionSagaState>,
		ISagaIsStartedBy<QuestionActor, QuestionId, QuestionCreated>,
		ISagaHandles<QuestionActor, QuestionId, AnswerVoteIncreased>,
		ISagaHandles<QuestionActor, QuestionId, AnswerVoteDecreased>
	{
		private ILoggingAdapter _loggingAdapter => Context.GetLogger();
		public HashSet<IActorRef> Subscribers = new HashSet<IActorRef>();
		private IActorRef _debouncer = ActorRefs.Nobody;
		private QuestionSagaConfiguration _config;
		public QuestionSaga() : this(new QuestionSagaConfiguration()) { }

		public QuestionSaga(QuestionSagaConfiguration config)
		{
			_config = config;
			Command<SubscriberTerminated>(command =>
			{
				if (Subscribers.TryGetValue(command.Subscriber, out var sub))
				{
					Subscribers.Remove(sub);
				}
			});
			Command<SubscribeToQuestion>(command =>
			{
				Subscribers.Add(command.Subscriber);
				Context.WatchWith(command.Subscriber, new SubscriberTerminated(command.Subscriber));
				Sender.Tell(new QuestionStateUpdated(State.Question, new Dictionary<string, long>(State.Answers), Subscribers.Count));
				return true;
			});
		}

		public override void AroundPreStart()
		{
			var cancellationToken = new CancellationTokenSource();
			var (actorRef, src) = Source.ActorRef<QuestionStateUpdated>(0, OverflowStrategy.DropHead)
				.PreMaterialize(Context.System);
			_debouncer = actorRef;

			var source = src.Conflate((currentValue, newValue) => newValue)
				.Delay(TimeSpan.FromMilliseconds(_config.DebouceTimerMilliseconds))
				.RunForeach(x => TellSubscribersStateChange(x), Context.System);

			base.AroundPreStart();
		}

		public bool Handle(IDomainEvent<QuestionActor, QuestionId, QuestionCreated> domainEvent)
		{
			var evnt = new QuestionSagaCreated(domainEvent.AggregateEvent.Question, domainEvent.AggregateEvent.Answers);
			Emit(evnt);
			var updatedState = new QuestionStateUpdated(evnt.Question, evnt.Answers.ToDictionary(x => x, y => (long)0), Subscribers.Count);
			_debouncer.Tell(updatedState);
			return true;
		}
		public bool Handle(IDomainEvent<QuestionActor, QuestionId, AnswerVoteIncreased> domainEvent)
		{
			var evnt = new Events.AnswerVoteIncreased(domainEvent.AggregateEvent.Answer);
			var updatedState = new QuestionStateUpdated(State.Question, new Dictionary<string, long>(State.Answers), Subscribers.Count);
			updatedState.Answers[evnt.Answer]++;
			_debouncer.Tell(updatedState);
			Emit(evnt);
			return true;
		}

		public bool Handle(IDomainEvent<QuestionActor, QuestionId, AnswerVoteDecreased> domainEvent)
		{
			var evnt = new Events.AnswerVoteDecreased(domainEvent.AggregateEvent.Answer);
			Emit(evnt);
			var updatedState = new QuestionStateUpdated(State.Question, new Dictionary<string, long>(State.Answers), Subscribers.Count);
			updatedState.Answers[evnt.Answer]--;
			_debouncer.Tell(updatedState);
			return true;
		}

		private void TellSubscribersStateChange(QuestionStateUpdated state)
		{
			_loggingAdapter.Debug($"Telling {Subscribers.Count} subscriber(s) of state change");
			foreach (var subscriber in Subscribers)
			{
				subscriber.Tell(state);
			}
		}
	}
}
