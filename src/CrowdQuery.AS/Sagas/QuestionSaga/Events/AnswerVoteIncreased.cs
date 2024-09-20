using Akkatecture.Aggregates;

namespace CrowdQuery.AS.Sagas.QuestionSaga.Events
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
