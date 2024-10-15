using Akka.Actor;
using Akka.Configuration;
using Akka.DistributedData;
using Akka.Persistence;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using Akkatecture.Aggregates;
using CrowdQuery.AS.Actors.Prompt;
using CrowdQuery.AS.Actors.Prompt.Events;
using CrowdQuery.AS.Projections;
using CrowdQuery.AS.Projections.BasicPromptStateProjection;
using CrowdQuery.AS.Projections.PromptProjection;
using FluentAssertions;
using Xunit;

namespace CrowdQuery.AS.Tests
{
	public class PromptProjectorTests : TestKit
	{
		private readonly TestProbe _testProbe;
		private Configuration _config;
		public PromptProjectorTests() : base(ConfigurationFactory.ParseString(
			@"	akka.loglevel = DEBUG
            	akka.actor.provider = cluster")
			.WithFallback(DistributedData.DefaultConfig()))
		{
			_testProbe = CreateTestProbe();
			_config = new Configuration(1);
		}

		[Fact]
		public void Receives_AddSubscriber_Responds_PromptProjectionState()
		{
			var promptProjector = Sys.ActorOf(PromptProjector.PropsFor("persistence-id"));
			promptProjector.Tell(new AddSubscriber(_testProbe));
			_testProbe.ExpectMsg<PromptProjectionState>();
		}

		[Fact]
		public void Receives_RemoveSubscriber()
		{
			var promptProjector = Sys.ActorOf(PromptProjector.PropsFor("persistence-id"));
			promptProjector.Tell(new AddSubscriber(_testProbe));
			promptProjector.Tell(new RemoveSubscriber(_testProbe));
		}

		[Fact]
		public void Receives_PromptCreated_Updates_DistributedData()
		{
			var promptId = PromptId.New;
			var promptProjector = Sys.ActorOf(PromptProjector.PropsFor(promptId.ToPromptProjectorId(), _config));
			DistributedData.Get(Sys).Replicator.Tell(Dsl.Subscribe(BasicPromptStateProjector.Key, _testProbe));
			var evnt = new ProjectedEvent<PromptCreated, PromptId>(new PromptCreated("Are you there?", ["yes", "no"]), promptId, 1);
			promptProjector.Tell(evnt);

			var changed = _testProbe.ExpectMsg<Changed>();
			var changedData = changed.Get(BasicPromptStateProjector.Key);
			changedData.ContainsKey(promptId.ToBase64()).Should().BeTrue();
			
			var projectedData = changedData[promptId.ToBase64()];
			projectedData.prompt.Should().Be("Are you there?");
			projectedData.answerCount.Should().Be(2);
			projectedData.totalVotes.Should().Be(0);
		}

		[Fact]
		public void Receives_PromptCreated_Updates_Subscribers()
		{
			var promptId = PromptId.New;
			var promptProjector = Sys.ActorOf(PromptProjector.PropsFor(promptId.ToPromptProjectorId(), _config));
			promptProjector.Tell(new AddSubscriber(_testProbe));
			_testProbe.ExpectMsg<PromptProjectionState>();
			var evnt = new ProjectedEvent<PromptCreated, PromptId>(new PromptCreated("Are you there?", ["yes", "no"]), promptId, 1);

			promptProjector.Tell(evnt);
			var state = _testProbe.ExpectMsg<PromptProjectionState>();
			state.Prompt.Should().Be("Are you there?");
			state.Answers.Should().HaveCount(2);
			state.LastSequenceNumber.Should().Be(1);
		}

		[Fact]
		public void Receives_VoteIncreased_Updates_DistributedData()
		{
			var promptId = PromptId.New;
			InitializeEventJournal(promptId.ToPromptProjectorId(), new ProjectionCreated("Are you there?", new Dictionary<string, int>() { { "Yes", 0 }, { "No", 0 } }));
			var promptProjector = Sys.ActorOf(PromptProjector.PropsFor(promptId.ToPromptProjectorId(), _config));
			DistributedData.Get(Sys).Replicator.Tell(Dsl.Subscribe(BasicPromptStateProjector.Key, _testProbe));
			var evnt = new ProjectedEvent<AnswerVoteIncreased, PromptId>(new AnswerVoteIncreased("Yes"), promptId, 2);

			promptProjector.Tell(evnt);
			var changed = _testProbe.ExpectMsg<Changed>();
			var changedData = changed.Get(BasicPromptStateProjector.Key);
			changedData.ContainsKey(promptId.ToBase64()).Should().BeTrue();
			var projectedData = changedData[promptId.ToBase64()];
			projectedData.prompt.Should().Be("Are you there?");
			projectedData.answerCount.Should().Be(2);
			projectedData.totalVotes.Should().Be(1);
		}

		[Fact]
		public void Receives_VoteIncreased_Updates_Subscribers()
		{
			var promptId = PromptId.New;
			InitializeEventJournal(promptId.ToPromptProjectorId(), new ProjectionCreated("Are you there?", new Dictionary<string, int>() { { "Yes", 0 }, { "No", 0 } }));
			var promptProjector = Sys.ActorOf(PromptProjector.PropsFor(promptId.ToPromptProjectorId(), _config));
			promptProjector.Tell(new AddSubscriber(_testProbe));
			_testProbe.ExpectMsg<PromptProjectionState>();
			var evnt = new ProjectedEvent<AnswerVoteIncreased, PromptId>(new AnswerVoteIncreased("Yes"), promptId, 2);

			promptProjector.Tell(evnt);
			var state = _testProbe.ExpectMsg<PromptProjectionState>();
			state.Prompt.Should().Be("Are you there?");
			state.Answers.Should().HaveCount(2);
			state.Answers["Yes"].Should().Be(1);
			state.LastSequenceNumber.Should().Be(2);
		}

		[Fact]
		public void Receives_VoteDecreased_Updates_DistributedData()
		{
			var promptId = PromptId.New;
			InitializeEventJournal(promptId.ToPromptProjectorId(),
				new ProjectionCreated("Are you there?", new Dictionary<string, int>() { { "Yes", 0 }, { "No", 0 } }),
				new ProjectionAnswerIncreased("Yes", 2));
			var promptProjector = Sys.ActorOf(PromptProjector.PropsFor(promptId.ToPromptProjectorId(), _config));
			DistributedData.Get(Sys).Replicator.Tell(Dsl.Subscribe(BasicPromptStateProjector.Key, _testProbe));
			var evnt = new ProjectedEvent<AnswerVoteDecreased, PromptId>(new AnswerVoteDecreased("Yes"), promptId, 3);

			promptProjector.Tell(evnt);
			var changed = _testProbe.ExpectMsg<Changed>();
			var changedData = changed.Get(BasicPromptStateProjector.Key);
			changedData.ContainsKey(promptId.ToBase64()).Should().BeTrue();
			var projectedData = changedData[promptId.ToBase64()];
			projectedData.prompt.Should().Be("Are you there?");
			projectedData.answerCount.Should().Be(2);
			projectedData.totalVotes.Should().Be(0);
		}

		[Fact]
		public void Receives_VoteDecreased_Updates_Subscribers()
		{
			var promptId = PromptId.New;
			InitializeEventJournal(promptId.ToPromptProjectorId(),
				new ProjectionCreated("Are you there?", new Dictionary<string, int>() { { "Yes", 0 }, { "No", 0 } }),
				new ProjectionAnswerIncreased("Yes", 2));
			var promptProjector = Sys.ActorOf(PromptProjector.PropsFor(promptId.ToPromptProjectorId(), _config));
			promptProjector.Tell(new AddSubscriber(_testProbe));
			_testProbe.ExpectMsg<PromptProjectionState>();
			var evnt = new ProjectedEvent<AnswerVoteDecreased, PromptId>(new AnswerVoteDecreased("Yes"), promptId, 3);

			promptProjector.Tell(evnt);
			var state = _testProbe.ExpectMsg<PromptProjectionState>();
			state.Prompt.Should().Be("Are you there?");
			state.Answers.Should().HaveCount(2);
			state.Answers["Yes"].Should().Be(0);
			state.LastSequenceNumber.Should().Be(3);
		}

		private void InitializeEventJournal(string aggregateId, params object[] events)
		{
			var writerGuid = Guid.NewGuid().ToString();
			var writes = new AtomicWrite[events.Length];
			var aggregateTestProbe = CreateTestProbe();
			for (var i = 0; i < events.Length; i++)
			{
				writes[i] = new AtomicWrite(new Persistent(events[i], i + 1, aggregateId, string.Empty, false, ActorRefs.NoSender, writerGuid));
			}
			var journal = Persistence.Instance.Apply(Sys).JournalFor(null);
			journal.Tell(new WriteMessages(writes, aggregateTestProbe.Ref, 1));

			aggregateTestProbe.ExpectMsg<WriteMessagesSuccessful>();

			for (var i = 0; i < events.Length; i++)
			{
				aggregateTestProbe.ExpectMsg<WriteMessageSuccess>();
			}
		}
	}
}
