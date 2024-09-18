using Akka.TestKit.Xunit2;
using Akkatecture.Aggregates;
using Akkatecture.Aggregates.CommandResults;
using Akkatecture.TestFixture.Extensions;
using CrowdQuery.Akka.Actors.Question;
using CrowdQuery.Akka.Actors.Question.Commands;
using CrowdQuery.Akka.Actors.Question.Events;
using Xunit;

namespace CrowdQuery.Akka.Tests
{
	public class QuestionAggregateTests : TestKit
	{
		private readonly QuestionId QuestionId = QuestionId.New;
		[Fact]
		public void Command_CreateQuestion_EmitsQuestionCreated()
		{
			this.FixtureFor<QuestionActor, QuestionId>(QuestionId)
				.GivenNothing()
				.When(new CreateQuestion(QuestionId, "Are you there?", new List<string>() { "Yes", "No" }))
				.ThenExpectDomainEvent((IDomainEvent<QuestionActor, QuestionId, QuestionCreated> evnt) => evnt.AggregateEvent.Question.Equals("Are you there?"));
		}

		[Fact]
		public void Command_CreateQuestion_FailureIsNotNew()
		{
			this.FixtureFor<QuestionActor, QuestionId>(QuestionId)
				.Given(new QuestionCreated("Are you there?", new List<string>() { "Yes", "No" }))
				.When(new CreateQuestion(QuestionId, "Hello World", ["Yes", "No"]))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Aggregate is not new"));
		}

		[Fact]
		public void Command_IncreaseAnswerVote_EmitAnswerVoteIncreased()
		{
			this.FixtureFor<QuestionActor, QuestionId>(QuestionId)
				.Given(new QuestionCreated("Are you there?", ["Yes", "No"]))
				.When(new IncreaseAnswerVote(QuestionId, "Yes"))
				.ThenExpectDomainEvent<AnswerVoteIncreased>(x => x.AggregateEvent.Answer == "Yes");
		}

		[Fact]
		public void Command_IncreaseAnswerVote_FailureIsNew()
		{
			this.FixtureFor<QuestionActor, QuestionId>(QuestionId)
				.GivenNothing()
				.When(new IncreaseAnswerVote(QuestionId, "Yes"))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Aggregate is new"));
		}

		[Fact]
		public void Command_IncreaseAnswerVote_NotContainsAnswer()
		{
			this.FixtureFor<QuestionActor, QuestionId>(QuestionId)
				.Given(new QuestionCreated("Are you there?", ["Yes", "No"]))
				.When(new IncreaseAnswerVote(QuestionId, "Why?"))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Answers does not contain Why?"));
		}

		[Fact]
		public void Command_DecreaseAnswerVote_EmitAnswerVoteDecreased()
		{
			this.FixtureFor<QuestionActor, QuestionId>(QuestionId)
				.Given(new QuestionCreated("Are you there?", ["Yes", "No"]), new AnswerVoteIncreased("Yes"))
				.When(new DecreaseAnswerVote(QuestionId, "Yes"))
				.ThenExpectDomainEvent<AnswerVoteDecreased>(x => x.AggregateEvent.Answer == "Yes");
		}

		[Fact]
		public void Command_DecreaseAnswerVote_FailureIsNew()
		{
			this.FixtureFor<QuestionActor, QuestionId>(QuestionId)
				.GivenNothing()
				.When(new DecreaseAnswerVote(QuestionId, "Yes"))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Aggregate is new"));
		}

		[Fact]
		public void Command_DecreaseAnswerVote_NotContainsAnswer()
		{
			this.FixtureFor<QuestionActor, QuestionId>(QuestionId)
				.Given(new QuestionCreated("Are you there?", ["Yes", "No"]))
				.When(new DecreaseAnswerVote(QuestionId, "Why?"))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Answers does not contain Why?"));
		}

		[Fact]
		public void Command_DecreaseAnswerVote_NotHasVotes()
		{
			this.FixtureFor<QuestionActor, QuestionId>(QuestionId)
				.Given(new QuestionCreated("Are you there?", ["Yes", "No"]))
				.When(new DecreaseAnswerVote(QuestionId, "Yes"))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Answer must have votes before decreasing the count"));

		}
	}
}
