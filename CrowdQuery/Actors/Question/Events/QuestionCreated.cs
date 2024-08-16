using Akkatecture.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdQuery.Actors.Question.Events
{
	public class QuestionCreated : AggregateEvent<QuestionActor, QuestionId>
	{
		public QuestionId QuestionId { get; set; }
		public string Question { get; set; }
		public List<string> Answers { get; set; }

		public QuestionCreated(QuestionId questionId, string question, List<string> answers)
		{
			QuestionId = questionId;
			Question = question;
			Answers = answers;
		}
	}
}
