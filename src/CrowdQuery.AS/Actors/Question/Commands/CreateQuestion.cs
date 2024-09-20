using Akkatecture.Commands;

namespace CrowdQuery.AS.Actors.Question.Commands
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
