using Akkatecture.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdQuery.Actors.Question.Commands
{
	public class CreateQuestion : Command<QuestionActor, QuestionId>
	{
		public string Question { get; set; }
		public List<string> Answers { get; set; }

		public CreateQuestion(QuestionId aggregateId, string question, List<string> answers) : base(aggregateId)
		{
			Question = question;
			Answers = answers;
		}
    }
}
