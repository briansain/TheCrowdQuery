using Akkatecture.Aggregates;
using CrowdQuery.Actors.Question;

namespace CrowdQuery.Saga.Events
{
	public class SagaQuestionCreated : AggregateEvent<QuestionAnswerSaga, QuestionAnswerSagaId>
	{
		public QuestionId QuestionId { get; set; }
		public SagaQuestionCreated(QuestionId questionId)
		{
			QuestionId = questionId;
		}
	}
}
