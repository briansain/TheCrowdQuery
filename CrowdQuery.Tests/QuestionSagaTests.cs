using Akka.Actor;
using Akka.Persistence;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using Akkatecture.Aggregates;
using CrowdQuery.Actors.Question;
using CrowdQuery.Actors.Question.Events;
using CrowdQuery.Sagas.QuestionSaga;
using CrowdQuery.Sagas.QuestionSaga.Commands;
using CrowdQuery.Sagas.QuestionSaga.Events;
using CrowdQuery.Sagas.QuestionSaga.ResponseModels;
using Xunit;
using AnswerVoteDecreased = CrowdQuery.Sagas.QuestionSaga.Events.AnswerVoteDecreased;
using AnswerVoteIncreased = CrowdQuery.Sagas.QuestionSaga.Events.AnswerVoteIncreased;

namespace CrowdQuery.Tests
{
	public class QuestionSagaTests : TestKit
	{
		private readonly TestProbe _testProbe;
		private readonly TestProbe _aggregateEventTestProbe;
		public QuestionSagaTests()
		{
			_testProbe = CreateTestProbe();
			_aggregateEventTestProbe = CreateTestProbe("aggregate-event-test-probe");
		}

		[Fact]
		public void SubscribesTo_QuestionCreated_EmitsQuestionCreated()
		{
			var questionSaga = Sys.ActorOf(Props.Create<QuestionSaga>(), "question-saga");
			Sys.EventStream.Subscribe(_testProbe, typeof(IDomainEvent<QuestionSaga, QuestionSagaId, QuestionSagaCreated>));
			var questionCreated = new QuestionCreated("Are you there?", ["Yes", "No"]);
			questionSaga.Tell(new DomainEvent<QuestionActor, QuestionId, QuestionCreated>(
									QuestionId.New, questionCreated, new Metadata(), DateTimeOffset.Now, 1));
			_testProbe.ExpectMsg<IDomainEvent<QuestionSaga, QuestionSagaId, QuestionSagaCreated>>();
		}

		[Fact]
		public void SubscribesTo_AnswerVoteIncreased_EmitsAnswerVoteIncreased()
		{
			var questionSagaId = new QuestionSagaId($"questionsaga-question-{Guid.NewGuid()}");

			InitializeEventJournal(questionSagaId, new QuestionSagaCreated("Are you there?", ["Yes", "No"]));

			var questionSaga = Sys.ActorOf(Props.Create<QuestionSaga>(), questionSagaId.Value);
			Sys.EventStream.Subscribe(_testProbe, typeof(IDomainEvent<QuestionSaga, QuestionSagaId, AnswerVoteIncreased>));
			var answerIncreased = new Actors.Question.Events.AnswerVoteIncreased("Yes");

			questionSaga.Tell(new DomainEvent<QuestionActor, QuestionId, Actors.Question.Events.AnswerVoteIncreased>(
									questionSagaId.ToQuestionId(), answerIncreased, new Metadata(), DateTimeOffset.Now, 1));
			_testProbe.ExpectMsg<IDomainEvent<QuestionSaga, QuestionSagaId, AnswerVoteIncreased>>();
		}

		[Fact]
		public void SubscribesTo_AnswerVoteDecreased_EmitsAnswerVoteDecreased()
		{
			var questionSagaId = new QuestionSagaId($"questionsaga-question-{Guid.NewGuid()}");

			InitializeEventJournal(questionSagaId, new QuestionSagaCreated("Are you there?", ["Yes", "No"]));

			var questionSaga = Sys.ActorOf(Props.Create<QuestionSaga>(), questionSagaId.Value);
			Sys.EventStream.Subscribe(_testProbe, typeof(IDomainEvent<QuestionSaga, QuestionSagaId, AnswerVoteDecreased>));
			var answerVoteDecreased = new Actors.Question.Events.AnswerVoteDecreased("Yes");


			questionSaga.Tell(new DomainEvent<QuestionActor, QuestionId, Actors.Question.Events.AnswerVoteDecreased>(
									questionSagaId.ToQuestionId(), answerVoteDecreased, new Metadata(), DateTimeOffset.Now, 1));
			_testProbe.ExpectMsg<IDomainEvent<QuestionSaga, QuestionSagaId, AnswerVoteDecreased>>();
		}

		[Fact]
		public void Command_SubscribeToQuestion_RepliesQuestionStateUpdated()
		{
			var questionSagaId = new QuestionSagaId($"questionsaga-question-{Guid.NewGuid()}");

			InitializeEventJournal(questionSagaId, new QuestionSagaCreated("Are you there?", ["Yes", "No"]));

			var questionSaga = Sys.ActorOf(Props.Create<QuestionSaga>(), questionSagaId.Value);
			var command = new SubscribeToQuestion(questionSagaId, _testProbe);
			questionSaga.Tell(command);
			ExpectMsg<QuestionStateUpdated>();

		}

		[Fact]
		public void Command_SubscribeToQuestion_UpdatesSubscribers()
		{
			var questionSagaId = new QuestionSagaId($"questionsaga-question-{Guid.NewGuid()}");

			InitializeEventJournal(questionSagaId, new QuestionSagaCreated("Are you there?", ["Yes", "No"]));

			var questionSaga = ActorOfAsTestActorRef<QuestionSaga>(Props.Create<QuestionSaga>(), questionSagaId.Value);
			var command = new SubscribeToQuestion(questionSagaId, _testProbe);
			questionSaga.Tell(command);
			AwaitAssert(
				() => Assert.Contains(_testProbe, questionSaga.UnderlyingActor.Subscribers));
		}

		[Fact]
		public void Command_SubscriberTerminated_RemovesFromSubscribers()
		{
			var questionSagaId = new QuestionSagaId($"questionsaga-question-{Guid.NewGuid()}");

			InitializeEventJournal(questionSagaId, new QuestionSagaCreated("Are you there?", ["Yes", "No"]));

			var questionSaga = ActorOfAsTestActorRef<QuestionSaga>(Props.Create<QuestionSaga>(), questionSagaId.Value);
			var command = new SubscribeToQuestion(questionSagaId, _testProbe);
			questionSaga.Tell(command);
			_testProbe.Tell(PoisonPill.Instance);

			AwaitAssert(
				() => Assert.DoesNotContain(_testProbe, questionSaga.UnderlyingActor.Subscribers));

		}

		[Fact]
		public void SubscribesTo_AnswerVoteIncreased_NotifiesSubscribers()
		{
			var questionSagaId = new QuestionSagaId($"questionsaga-question-{Guid.NewGuid()}");

			InitializeEventJournal(questionSagaId, new QuestionSagaCreated("Are you there?", ["Yes", "No"]));

			var questionSaga = Sys.ActorOf(Props.Create(() => new QuestionSaga(new QuestionSagaConfiguration(0))), questionSagaId.Value);
			var command = new SubscribeToQuestion(questionSagaId, _testProbe);
			questionSaga.Tell(command);
			var answerIncreased = new Actors.Question.Events.AnswerVoteIncreased("Yes");

			questionSaga.Tell(new DomainEvent<QuestionActor, QuestionId, Actors.Question.Events.AnswerVoteIncreased>(
									questionSagaId.ToQuestionId(), answerIncreased, new Metadata(), DateTimeOffset.Now, 1));

			AwaitAssert(
				() => _testProbe.ExpectMsg<QuestionStateUpdated>()
			);
		}

		[Fact]
		public void SubscribesTo_AnswerVoteDecreased_NotifiesSubscribers()
		{
			var questionSagaId = new QuestionSagaId($"questionsaga-question-{Guid.NewGuid()}");

			InitializeEventJournal(questionSagaId, new QuestionSagaCreated("Are you there?", ["Yes", "No"]));

			var questionSaga = Sys.ActorOf(Props.Create(() => new QuestionSaga(new QuestionSagaConfiguration(0))), questionSagaId.Value);
			var command = new SubscribeToQuestion(questionSagaId, _testProbe);
			questionSaga.Tell(command);
			var answerIncreased = new Actors.Question.Events.AnswerVoteDecreased("Yes");

			questionSaga.Tell(new DomainEvent<QuestionActor, QuestionId, Actors.Question.Events.AnswerVoteDecreased>(
									questionSagaId.ToQuestionId(), answerIncreased, new Metadata(), DateTimeOffset.Now, 1));

			AwaitAssert(
				() => _testProbe.ExpectMsg<QuestionStateUpdated>()
			);
		}

		private void InitializeEventJournal(QuestionSagaId aggregateId, params IAggregateEvent<QuestionSaga, QuestionSagaId>[] events)
		{
			var writerGuid = Guid.NewGuid().ToString();
			var writes = new AtomicWrite[events.Length];
			for (var i = 0; i < events.Length; i++)
			{
				var committedEvent = new CommittedEvent<QuestionSaga, QuestionSagaId, IAggregateEvent<QuestionSaga, QuestionSagaId>>(aggregateId, events[i], new Metadata(), DateTimeOffset.UtcNow, i + 1);
				writes[i] = new AtomicWrite(new Persistent(committedEvent, i + 1, aggregateId.Value, string.Empty, false, ActorRefs.NoSender, writerGuid));
			}
			var journal = Persistence.Instance.Apply(Sys).JournalFor(null);
			journal.Tell(new WriteMessages(writes, _aggregateEventTestProbe.Ref, 1));

			_aggregateEventTestProbe.ExpectMsg<WriteMessagesSuccessful>();

			for (var i = 0; i < events.Length; i++)
			{
				var seq = i;
				_aggregateEventTestProbe.ExpectMsg<WriteMessageSuccess>(x =>
					x.Persistent.PersistenceId == aggregateId.ToString() &&
					x.Persistent.Payload is CommittedEvent<QuestionSaga, QuestionSagaId, IAggregateEvent<QuestionSaga, QuestionSagaId>> &&
					x.Persistent.SequenceNr == (long)seq + 1);
			}
		}
	}
}
