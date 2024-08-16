using Akkatecture.Aggregates;
using CrowdQuery.Actors.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdQuery.Actors.Answer.Events
{
	public class AnswerCreated: AggregateEvent<AnswerActor, AnswerId>
	{
		public QuestionId QuestionId { get; set; }
		public string Answer { get; set; }

		public AnswerCreated(QuestionId questionId, string answer)
		{
			Answer = answer;
			QuestionId = questionId;
		}
	}
}
