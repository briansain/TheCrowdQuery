using Akkatecture.Aggregates;

namespace CrowdQuery.Akka.Actors.Question.Events
{
	public class AnswerVoteIncreased : AggregateEvent<QuestionActor, QuestionId>
	{
		public string Answer { get; set; }
		public AnswerVoteIncreased(string answer)
		{
			Answer = answer;
		}
	}
}
