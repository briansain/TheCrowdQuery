using Akka.Actor;
using Akka.TestKit.Xunit2;
using Akkatecture.Aggregates;
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
		public void DoesManagerCreateChild()
		{
			var testProbe = CreateTestProbe();
			Sys.EventStream.Subscribe(testProbe, typeof(IDomainEvent<QuestionActor, QuestionId, QuestionCreated>));
			var questionManager = Sys.ActorOf(Props.Create<QuestionManager>(), "question-manager-test");
			var questionId = QuestionId.New;
			var createQuestion = new CreateQuestion(questionId, "Are you there?", new List<string>() { "Yes", "No" });
			questionManager.Tell(createQuestion);
			ExpectNoMsg();

			// might need to do await assert
			testProbe.ExpectMsg<IDomainEvent<QuestionActor, QuestionId, QuestionCreated>>();
		}
	}
}
