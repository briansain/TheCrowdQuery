using Akkatecture.TestFixture.Extensions;
using CrowdQuery.Actors.Question;
using Xunit;
using Akka.TestKit.Xunit2;
using CrowdQuery.Actors.Question.Commands;
using CrowdQuery.Actors.Question.Events;
using Akkatecture.Aggregates;

namespace CrowdQuery.Tests
{
	public class QuestionActorTests : TestKit
	{
		[Fact]
		public void AggregateCreation()
		{
			var questionId = QuestionId.New;

			this.FixtureFor<QuestionActor, QuestionId>(questionId)
				.GivenNothing()
				.When(new CreateQuestion(questionId, "Are you there?", new List<string>() { "Yes", "No" }))
				.ThenExpectDomainEvent((IDomainEvent<QuestionActor, QuestionId, QuestionCreated> evnt) => evnt.AggregateEvent.Question.Equals("Are you there?"));
		}
	}
}
