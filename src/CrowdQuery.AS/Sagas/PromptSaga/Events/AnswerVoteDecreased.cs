using Akkatecture.Aggregates;

namespace CrowdQuery.AS.Sagas.PromptSaga.Events
{
	public class AnswerVoteDecreased : AggregateEvent<PromptSaga, PromptSagaId>
	{
		public string Answer { get; set; }
		public AnswerVoteDecreased(string answer)
		{
			Answer = answer;
		}
	}
}
