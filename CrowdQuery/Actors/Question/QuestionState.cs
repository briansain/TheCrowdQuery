using Akkatecture.Aggregates;
using CrowdQuery.Actors.Question.Events;

namespace CrowdQuery.Actors.Question
{
	public class QuestionState : AggregateState<QuestionActor, QuestionId>,
		IApply<QuestionCreated>,
		IApply<AnswerVoteIncreased>,
		IApply<AnswerVoteDecreased>
	{
		public string Question { get; set; }
		public Dictionary<string, int> Answers { get; set; }

		public QuestionState()
		{
			Answers = new Dictionary<string, int>();
			Question = string.Empty;
		}

		public QuestionState(string question, Dictionary<string, int> answers)
		{
			Question = question;
			Answers = answers;
		}

		public void Apply(QuestionCreated aggregateEvent)
		{
			Question = aggregateEvent.Question;
			Answers = aggregateEvent.Answers.ToDictionary(x => x, y => 0);
		}

		public void Apply(AnswerVoteIncreased aggregateEvent)
		{
			Answers[aggregateEvent.Answer]++;
		}

		public void Apply(AnswerVoteDecreased aggregateEvent)
		{
			Answers[aggregateEvent.Answer]--;
		}
	}
}
