using Akkatecture.Aggregates;

namespace CrowdQuery.Actors.Question.Events
{
	public class AnswerVoteDecreased : AggregateEvent<QuestionActor, QuestionId>
	{
		public string Answer { get; set; }
		public AnswerVoteDecreased(string answer)
		{
			Answer = answer;
		}
	}
}
