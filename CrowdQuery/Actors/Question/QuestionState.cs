using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdQuery.Actors.Question
{
	internal class QuestionState
	{
		public string Question { get; set; }
		public List<string> Answers { get; set; }

		public QuestionState()
		{
			Answers = new List<string>();
			Question = string.Empty;
		}
		public QuestionState(string question, List<string> answers)
		{
			Question = question;
			Answers = answers;
		}
	}
}
