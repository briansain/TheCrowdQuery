using Akkatecture.Aggregates;

namespace CrowdQuery.AS.Sagas.PromptSaga.Events
{
	public class AnswerVoteIncreased : AggregateEvent<PromptSaga, PromptSagaId>
	{
		public string Answer { get; set; }
		public AnswerVoteIncreased(string answer)
		{
			Answer = answer;
		}
	}
}
