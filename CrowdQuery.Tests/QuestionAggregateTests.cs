using Akka.Actor;
using Akka.TestKit.Xunit2;
using Akkatecture.Aggregates;
using Akkatecture.Aggregates.CommandResults;
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

namespace CrowdQuery.Tests
{
    public class QuestionAggregateTests : TestKit
    {
        private readonly QuestionId QuestionId = QuestionId.New;
        [Fact]
        public void QuestionActor_CommandCreateQuestion_EmitsQuestionCreated()
        {
            this.FixtureFor<QuestionActor, QuestionId>(QuestionId)
                .GivenNothing()
                .When(new CreateQuestion(QuestionId, "Are you there?", new List<string>() { "Yes", "No" }))
                .ThenExpectDomainEvent((IDomainEvent<QuestionActor, QuestionId, QuestionCreated> evnt) => evnt.AggregateEvent.Question.Equals("Are you there?"));
        }

        [Fact]
        public void QuestionActor_CommandCreateQuestion_FailureIsNew()
        {
            this.FixtureFor<QuestionActor, QuestionId>(QuestionId)
                .Given(new QuestionCreated(QuestionId, "Are you there?", new List<string>() { "Yes", "No" }))
                .When(new CreateQuestion(QuestionId, "Hello World", ["Yes", "No"]))
                .ThenExpectReply<FailedCommandResult>(x => x.Errors.Contains("Aggregate is not new"));
        }
    }
}
