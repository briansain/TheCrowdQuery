using Akka.TestKit.Xunit2;
using Akkatecture.Aggregates;
using Akkatecture.Aggregates.CommandResults;
using Akkatecture.TestFixture.Extensions;
using CrowdQuery.AS.Actors.Prompt;
using CrowdQuery.AS.Actors.Prompt.Commands;
using CrowdQuery.AS.Actors.Prompt.Events;
using Xunit;

namespace CrowdQuery.AS.Tests
{
	public class PromptAggregateTests : TestKit
	{
		private readonly PromptId PromptId = PromptId.New;
		[Fact]
		public void Command_CreatePrompt_EmitsPromptCreated()
		{
			this.FixtureFor<PromptActor, PromptId>(PromptId)
				.GivenNothing()
				.When(new CreatePrompt(PromptId, "Are you there?", new List<string>() { "Yes", "No" }))
				.ThenExpectReply<SuccessCommandResult>()
				.ThenExpectDomainEvent((IDomainEvent<PromptActor, PromptId, PromptCreated> evnt) => evnt.AggregateEvent.Prompt.Equals("Are you there?"));
		}

		[Fact]
		public void Command_CreatePrompt_FailureIsNotNew()
		{
			this.FixtureFor<PromptActor, PromptId>(PromptId)
				.Given(new PromptCreated("Are you there?", new List<string>() { "Yes", "No" }))
				.When(new CreatePrompt(PromptId, "Hello World", ["Yes", "No"]))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Aggregate is not new"));
		}

		[Fact]
		public void Command_IncreaseAnswerVote_EmitAnswerVoteIncreased()
		{
			this.FixtureFor<PromptActor, PromptId>(PromptId)
				.Given(new PromptCreated("Are you there?", ["Yes", "No"]))
				.When(new IncreaseAnswerVote(PromptId, "Yes"))
				.ThenExpectDomainEvent<AnswerVoteIncreased>(x => x.AggregateEvent.Answer == "Yes");
		}

		[Fact]
		public void Command_IncreaseAnswerVote_FailureIsNew()
		{
			this.FixtureFor<PromptActor, PromptId>(PromptId)
				.GivenNothing()
				.When(new IncreaseAnswerVote(PromptId, "Yes"))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Aggregate is new"));
		}

		[Fact]
		public void Command_IncreaseAnswerVote_NotContainsAnswer()
		{
			this.FixtureFor<PromptActor, PromptId>(PromptId)
				.Given(new PromptCreated("Are you there?", ["Yes", "No"]))
				.When(new IncreaseAnswerVote(PromptId, "Why?"))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Answers does not contain Why?"));
		}

		[Fact]
		public void Command_DecreaseAnswerVote_EmitAnswerVoteDecreased()
		{
			this.FixtureFor<PromptActor, PromptId>(PromptId)
				.Given(new PromptCreated("Are you there?", ["Yes", "No"]), new AnswerVoteIncreased("Yes"))
				.When(new DecreaseAnswerVote(PromptId, "Yes"))
				.ThenExpectDomainEvent<AnswerVoteDecreased>(x => x.AggregateEvent.Answer == "Yes");
		}

		[Fact]
		public void Command_DecreaseAnswerVote_FailureIsNew()
		{
			this.FixtureFor<PromptActor, PromptId>(PromptId)
				.GivenNothing()
				.When(new DecreaseAnswerVote(PromptId, "Yes"))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Aggregate is new"));
		}

		[Fact]
		public void Command_DecreaseAnswerVote_NotContainsAnswer()
		{
			this.FixtureFor<PromptActor, PromptId>(PromptId)
				.Given(new PromptCreated("Are you there?", ["Yes", "No"]))
				.When(new DecreaseAnswerVote(PromptId, "Why?"))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Answers does not contain Why?"));
		}

		[Fact]
		public void Command_DecreaseAnswerVote_NotHasVotes()
		{
			this.FixtureFor<PromptActor, PromptId>(PromptId)
				.Given(new PromptCreated("Are you there?", ["Yes", "No"]))
				.When(new DecreaseAnswerVote(PromptId, "Yes"))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Answer must have votes before decreasing the count"));

		}
	}
}
