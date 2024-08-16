using Akkatecture.Aggregates;
using Akkatecture.Aggregates.Snapshot;
using CrowdQuery.Actors.Answer.Events;
using CrowdQuery.Actors.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdQuery.Actors.Answer
{
	public class AnswerState : AggregateState<AnswerActor, AnswerId>,
		IApply<AnswerCreated>,
		IApply<VoteIncreased>,
		IApply<VoteDecreased>
	{
		public QuestionId QuestionId { get; private set; }
		public string Answer { get; private set; }
		public int Votes { get; private set; }

		public void Apply(AnswerCreated aggregateEvent)
		{
			QuestionId = aggregateEvent.QuestionId;
			Answer = aggregateEvent.Answer;
			Votes = 0;
		}

		public void Apply(VoteIncreased aggregateEvent)
		{
			Votes++;
		}

		public void Apply(VoteDecreased aggregateEvent)
		{
			Votes--;
		}
	}
}
