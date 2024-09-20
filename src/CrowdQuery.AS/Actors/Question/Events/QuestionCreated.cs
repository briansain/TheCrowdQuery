using Akkatecture.Aggregates;

namespace CrowdQuery.AS.Actors.Question.Events
{
	public class QuestionCreated : AggregateEvent<QuestionActor, QuestionId>
	{
		public string Question { get; set; }
		public List<string> Answers { get; set; }

		public QuestionCreated(string question, List<string> answers)
		{
			Question = question;
			Answers = answers;
		}
	}
}
