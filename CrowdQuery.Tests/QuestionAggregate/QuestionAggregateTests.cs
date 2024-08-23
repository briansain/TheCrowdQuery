using Akka.Actor;
using Akka.TestKit.Xunit2;
using Akkatecture.Aggregates;
using Akkatecture.TestFixture.Extensions;
using CrowdQuery.Actors.Question;
using CrowdQuery.Actors.Question.Commands;
using CrowdQuery.Actors.Question.Events;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CrowdQuery.Tests.QuestionAggregate
{
	public class QuestionAggregateTests : TestKit
	{
		[Fact]
		public void QuestionActor_CommandCreateQuestion_ProducesQuestionCreated()
		{
			var questionId = QuestionId.New;

			this.FixtureFor<QuestionActor, QuestionId>(questionId)
				.GivenNothing()
				.When(new CreateQuestion(questionId, "Are you there?", new List<string>() { "Yes", "No" }))
				.ThenExpectDomainEvent((IDomainEvent<QuestionActor, QuestionId, QuestionCreated> evnt) => evnt.AggregateEvent.Question.Equals("Are you there?"));
		}
	}
}
