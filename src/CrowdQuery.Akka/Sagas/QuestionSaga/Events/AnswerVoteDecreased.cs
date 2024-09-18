using Akkatecture.Aggregates;

namespace CrowdQuery.Akka.Sagas.QuestionSaga.Events
{
	public class AnswerVoteDecreased : AggregateEvent<QuestionSaga, QuestionSagaId>
	{
		public string Answer { get; set; }
		public AnswerVoteDecreased(string answer)
		{
			Answer = answer;
		}
	}
}
