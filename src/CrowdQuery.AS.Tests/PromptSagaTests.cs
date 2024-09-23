using Akka.Actor;
using Akka.Configuration;
using Akka.DistributedData;
using Akka.Persistence;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using Akkatecture.Aggregates;
using CrowdQuery.AS.Actors;
using CrowdQuery.AS.Actors.Prompt;
using CrowdQuery.AS.Actors.Prompt.Events;
using CrowdQuery.AS.Sagas.PromptSaga;
using CrowdQuery.AS.Sagas.PromptSaga.Commands;
using CrowdQuery.AS.Sagas.PromptSaga.Events;
using CrowdQuery.AS.Sagas.PromptSaga.ResponseModels;
using Xunit;
using AnswerVoteDecreased = CrowdQuery.AS.Sagas.PromptSaga.Events.AnswerVoteDecreased;
using AnswerVoteIncreased = CrowdQuery.AS.Sagas.PromptSaga.Events.AnswerVoteIncreased;

namespace CrowdQuery.AS.Tests
{
	public class PromptSagaTests : TestKit
	{
		private readonly TestProbe _testProbe;
		private readonly TestProbe _aggregateEventTestProbe;
		public PromptSagaTests() : base(ConfigurationFactory.ParseString(
			@"	akka.loglevel = DEBUG
            	akka.actor.provider = cluster")
			.WithFallback(DistributedData.DefaultConfig()))
		{
			_testProbe = CreateTestProbe();
			_aggregateEventTestProbe = CreateTestProbe("aggregate-event-test-probe");
		}

		[Fact]
		public void SubscribesTo_PromptCreated_EmitsPromptCreated()
		{
			var PromptSaga = Sys.ActorOf(Props.Create<PromptSaga>(), "Prompt-saga");
			Sys.EventStream.Subscribe(_testProbe, typeof(IDomainEvent<PromptSaga, PromptSagaId, PromptSagaCreated>));
			var PromptCreated = new PromptCreated("Are you there?", ["Yes", "No"]);
			PromptSaga.Tell(new DomainEvent<PromptActor, PromptId, PromptCreated>(
									PromptId.New, PromptCreated, new Metadata(), DateTimeOffset.Now, 1));
			_testProbe.ExpectMsg<IDomainEvent<PromptSaga, PromptSagaId, PromptSagaCreated>>();
		}

		[Fact]
		public void SubscribesTo_AnswerVoteIncreased_EmitsAnswerVoteIncreased()
		{
			var PromptSagaId = new PromptSagaId($"promptsaga-prompt-{Guid.NewGuid()}");

			InitializeEventJournal(PromptSagaId, new PromptSagaCreated("Are you there?", ["Yes", "No"]));

			var PromptSaga = Sys.ActorOf(Props.Create<PromptSaga>(), PromptSagaId.Value);
			Sys.EventStream.Subscribe(_testProbe, typeof(IDomainEvent<PromptSaga, PromptSagaId, AnswerVoteIncreased>));
			var answerIncreased = new Actors.Prompt.Events.AnswerVoteIncreased("Yes");

			PromptSaga.Tell(new DomainEvent<PromptActor, PromptId, Actors.Prompt.Events.AnswerVoteIncreased>(
									PromptSagaId.ToPromptId(), answerIncreased, new Metadata(), DateTimeOffset.Now, 1));
			_testProbe.ExpectMsg<IDomainEvent<PromptSaga, PromptSagaId, AnswerVoteIncreased>>();
		}

		[Fact]
		public void SubscribesTo_AnswerVoteDecreased_EmitsAnswerVoteDecreased()
		{
			var PromptSagaId = new PromptSagaId($"promptsaga-prompt-{Guid.NewGuid()}");

			InitializeEventJournal(PromptSagaId, new PromptSagaCreated("Are you there?", ["Yes", "No"]));

			var PromptSaga = Sys.ActorOf(Props.Create<PromptSaga>(), PromptSagaId.Value);
			Sys.EventStream.Subscribe(_testProbe, typeof(IDomainEvent<PromptSaga, PromptSagaId, AnswerVoteDecreased>));
			var answerVoteDecreased = new Actors.Prompt.Events.AnswerVoteDecreased("Yes");


			PromptSaga.Tell(new DomainEvent<PromptActor, PromptId, Actors.Prompt.Events.AnswerVoteDecreased>(
									PromptSagaId.ToPromptId(), answerVoteDecreased, new Metadata(), DateTimeOffset.Now, 1));
			_testProbe.ExpectMsg<IDomainEvent<PromptSaga, PromptSagaId, AnswerVoteDecreased>>();
		}

		[Fact]
		public void Command_SubscribeToPrompt_RepliesPromptStateUpdated()
		{
			var PromptSagaId = new PromptSagaId($"promptsaga-prompt-{Guid.NewGuid()}");

			InitializeEventJournal(PromptSagaId, new PromptSagaCreated("Are you there?", ["Yes", "No"]));

			var PromptSaga = Sys.ActorOf(Props.Create<PromptSaga>(), PromptSagaId.Value);
			var command = new SubscribeToPrompt(PromptSagaId, _testProbe);
			PromptSaga.Tell(command);
			ExpectMsg<PromptStateUpdated>();

		}

		[Fact]
		public void Command_SubscribeToPrompt_UpdatesSubscribers()
		{
			var PromptSagaId = new PromptSagaId($"promptsaga-prompt-{Guid.NewGuid()}");

			InitializeEventJournal(PromptSagaId, new PromptSagaCreated("Are you there?", ["Yes", "No"]));

			var PromptSaga = ActorOfAsTestActorRef<PromptSaga>(Props.Create<PromptSaga>(), PromptSagaId.Value);
			var command = new SubscribeToPrompt(PromptSagaId, _testProbe);
			PromptSaga.Tell(command);
			AwaitAssert(
				() => Assert.Contains(_testProbe, PromptSaga.UnderlyingActor.Subscribers));
		}

		[Fact]
		public void Command_SubscriberTerminated_RemovesFromSubscribers()
		{
			var PromptSagaId = new PromptSagaId($"promptsaga-prompt-{Guid.NewGuid()}");

			InitializeEventJournal(PromptSagaId, new PromptSagaCreated("Are you there?", ["Yes", "No"]));

			var PromptSaga = ActorOfAsTestActorRef<PromptSaga>(Props.Create<PromptSaga>(), PromptSagaId.Value);
			var command = new SubscribeToPrompt(PromptSagaId, _testProbe);
			PromptSaga.Tell(command);
			_testProbe.Tell(PoisonPill.Instance);

			AwaitAssert(
				() => Assert.DoesNotContain(_testProbe, PromptSaga.UnderlyingActor.Subscribers));

		}

		[Fact]
		public void SubscribesTo_AnswerVoteIncreased_NotifiesSubscribers()
		{
			var PromptSagaId = new PromptSagaId($"promptsaga-prompt-{Guid.NewGuid()}");

			InitializeEventJournal(PromptSagaId, new PromptSagaCreated("Are you there?", ["Yes", "No"]));

			var PromptSaga = Sys.ActorOf(Props.Create(() => new PromptSaga(new PromptSagaConfiguration(0))), PromptSagaId.Value);
			var command = new SubscribeToPrompt(PromptSagaId, _testProbe);
			PromptSaga.Tell(command);
			var answerIncreased = new Actors.Prompt.Events.AnswerVoteIncreased("Yes");

			PromptSaga.Tell(new DomainEvent<PromptActor, PromptId, Actors.Prompt.Events.AnswerVoteIncreased>(
									PromptSagaId.ToPromptId(), answerIncreased, new Metadata(), DateTimeOffset.Now, 1));

			AwaitAssert(
				() => _testProbe.ExpectMsg<PromptStateUpdated>()
			);
		}

		[Fact]
		public void SubscribesTo_AnswerVoteDecreased_NotifiesSubscribers()
		{
			var PromptSagaId = new PromptSagaId($"promptsaga-prompt-{Guid.NewGuid()}");

			InitializeEventJournal(PromptSagaId, new PromptSagaCreated("Are you there?", ["Yes", "No"]));

			var PromptSaga = Sys.ActorOf(Props.Create(() => new PromptSaga(new PromptSagaConfiguration(0))), PromptSagaId.Value);
			var command = new SubscribeToPrompt(PromptSagaId, _testProbe);
			PromptSaga.Tell(command);
			var answerIncreased = new Actors.Prompt.Events.AnswerVoteDecreased("Yes");

			PromptSaga.Tell(new DomainEvent<PromptActor, PromptId, Actors.Prompt.Events.AnswerVoteDecreased>(
									PromptSagaId.ToPromptId(), answerIncreased, new Metadata(), DateTimeOffset.Now, 1));

			AwaitAssert(
				() => _testProbe.ExpectMsg<PromptStateUpdated>()
			);
		}

				[Fact]
		public void SubscribesTo_PromptCreated_UpdatesDistributedData()
		{
			var PromptSaga = Sys.ActorOf(Props.Create<PromptSaga>(), "prompt-saga");
			var replicator = DistributedData.Get(Sys).Replicator;
			replicator.Tell(Dsl.Subscribe(AllPromptsActor.AllPromptsBasicKey, _testProbe));

			var PromptCreated = new PromptCreated("Are you there?", ["Yes", "No"]);
			PromptSaga.Tell(new DomainEvent<PromptActor, PromptId, PromptCreated>(
									PromptId.New, PromptCreated, new Metadata(), DateTimeOffset.Now, 1));

			_testProbe.ExpectMsg<Changed>();
		}

		private void InitializeEventJournal(PromptSagaId aggregateId, params IAggregateEvent<PromptSaga, PromptSagaId>[] events)
		{
			var writerGuid = Guid.NewGuid().ToString();
			var writes = new AtomicWrite[events.Length];
			for (var i = 0; i < events.Length; i++)
			{
				var committedEvent = new CommittedEvent<PromptSaga, PromptSagaId, IAggregateEvent<PromptSaga, PromptSagaId>>(aggregateId, events[i], new Metadata(), DateTimeOffset.UtcNow, i + 1);
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
					x.Persistent.Payload is CommittedEvent<PromptSaga, PromptSagaId, IAggregateEvent<PromptSaga, PromptSagaId>> &&
					x.Persistent.SequenceNr == (long)seq + 1);
			}
		}
	}
}
