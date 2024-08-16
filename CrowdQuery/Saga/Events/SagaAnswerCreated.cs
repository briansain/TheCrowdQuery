using Akkatecture.Aggregates;
using CrowdQuery.Actors.Answer;

namespace CrowdQuery.Saga.Events
{
	public class SagaAnswerCreated : AggregateEvent<QuestionAnswerSaga, QuestionAnswerSagaId>
	{
		public AnswerId AnswerId { get; set; }
		public SagaAnswerCreated(AnswerId answerId)
		{
			AnswerId = answerId;
		}
	}
}
