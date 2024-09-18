using Akkatecture.Aggregates;

namespace CrowdQuery.Akka.Sagas.QuestionSaga.Events
{
	public class AnswerVoteIncreased : AggregateEvent<QuestionSaga, QuestionSagaId>
	{
		public string Answer { get; set; }
		public AnswerVoteIncreased(string answer)
		{
			Answer = answer;
		}
	}
}
