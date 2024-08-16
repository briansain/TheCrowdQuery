using Akkatecture.Aggregates;
using CrowdQuery.Actors.Question.Events;

namespace CrowdQuery.Actors.Question
{
	public class QuestionState : AggregateState<QuestionActor, QuestionId>,
		IApply<QuestionCreated>
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

		public void Apply(QuestionCreated aggregateEvent)
		{
			Question = aggregateEvent.Question;
			Answers = aggregateEvent.Answers;
		}
	}
}
