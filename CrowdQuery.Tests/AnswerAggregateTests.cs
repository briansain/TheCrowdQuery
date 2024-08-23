using Akka.TestKit.Xunit2;
using Akkatecture.Aggregates.CommandResults;
using Akkatecture.TestFixture.Extensions;
using CrowdQuery.Actors.Answer;
using CrowdQuery.Actors.Answer.Commands;
using CrowdQuery.Actors.Answer.Events;
using CrowdQuery.Actors.Question;
using Xunit;

namespace CrowdQuery.Tests
{
	public class AnswerAggregateTests: TestKit
	{
		public static AnswerId AnswerId => AnswerId.New;
		[Fact]
		public void AnswerAggregate_CommandCreateAnswer_EmitsAnswerCreated()
		{
			this.FixtureFor<AnswerActor, AnswerId>(AnswerId)
				.GivenNothing()
				.When(new CreateAnswer(AnswerId, QuestionId.New, "Yes"))
				.ThenExpect<AnswerCreated>(x => x.Answer.Equals("Yes"));
		}

		[Fact]
		public void AnswerAggregate_CommandCreateAnswer_FailureIsNotNew()
		{
			this.FixtureFor<AnswerActor, AnswerId>(AnswerId)
				.Given(new AnswerCreated(QuestionId.New, "Yes"))
				.When(new CreateAnswer(AnswerId, QuestionId.New, "Hello World"))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Aggregate is not new"));
		}

		[Fact]
		public void AnswerAggregate_CommandIncreaseVote_EmitVoteIncreased()
		{
			this.FixtureFor<AnswerActor, AnswerId>(AnswerId)
				.Given(new AnswerCreated(QuestionId.New, "Yes"))
				.When(new IncreaseVote(AnswerId))
				.ThenExpect<VoteIncreased>();
		}

		[Fact]
		public void AnswerAggregate_CommandIncreaseVote_FailureIsNew()
		{
			this.FixtureFor<AnswerActor, AnswerId>(AnswerId)
				.GivenNothing()
				.When(new IncreaseVote(AnswerId))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Aggregate is new"));
		}

		[Fact]
		public void AnswerAggregate_CommandDecreaseVote_EmitVoteDecreased()
		{
			this.FixtureFor<AnswerActor, AnswerId>(AnswerId)
				.Given(new AnswerCreated(QuestionId.New, "Yes"))
				.When(new DecreaseVote(AnswerId))
				.ThenExpect<VoteDecreased>();
		}

		[Fact]
		public void AnswerAggregate_CommandDecreaseVote_FailureIsNew()
		{
			this.FixtureFor<AnswerActor, AnswerId>(AnswerId)
				.GivenNothing()
				.When(new DecreaseVote(AnswerId))
				.ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Aggregate is new"));
		}
	}
}
