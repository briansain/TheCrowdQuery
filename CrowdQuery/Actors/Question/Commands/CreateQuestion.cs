using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdQuery.Actors.Question.Commands
{
	public class CreateQuestion
	{
		public Guid QuestionId { get; set; }
		public string Question { get; set; }
		public List<string> Answers { get; set; }

		public CreateQuestion(Guid questionId, string question, List<string> answers)
		{
			QuestionId = questionId;
			Question = question;
			Answers = answers;
		}
    }
}
