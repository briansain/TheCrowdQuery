using Akkatecture.Aggregates;
using CrowdQuery.Akka.Actors.Question.Events;

namespace CrowdQuery.Akka.Actors.Question
{
	public class QuestionState : AggregateState<QuestionActor, QuestionId>,
		IApply<QuestionCreated>,
		IApply<AnswerVoteIncreased>,
		IApply<AnswerVoteDecreased>
	{
		public string Question { get; private set; }
		public Dictionary<string, long> AnswerVotes { get; private set; }

		public QuestionState()
		{
			AnswerVotes = new Dictionary<string, long>();
			Question = string.Empty;
		}

		public QuestionState(string question, Dictionary<string, long> answers)
		{
			Question = question;
			AnswerVotes = answers;
		}

		public void Apply(QuestionCreated aggregateEvent)
		{
			Question = aggregateEvent.Question;
			AnswerVotes = aggregateEvent.Answers.ToDictionary(x => x, y => (long)0);
		}

		public void Apply(AnswerVoteIncreased aggregateEvent)
		{
			AnswerVotes[aggregateEvent.Answer]++;
		}

		public void Apply(AnswerVoteDecreased aggregateEvent)
		{
			AnswerVotes[aggregateEvent.Answer]--;
		}

		public QuestionState DeepCopy()
		{
			var newState = new QuestionState();
			newState.Question = Question;
			newState.AnswerVotes = new Dictionary<string, long>(AnswerVotes);
			return newState;
		}
	}
}
