using Akkatecture.Aggregates;
using CrowdQuery.Actors.Answer;
using CrowdQuery.Actors.Question;

namespace CrowdQuery.Saga.Events
{
	public class SagaStarted : AggregateEvent<QuestionAnswerSaga, QuestionAnswerSagaId>
	{
		public QuestionId QuestionId { get; set; }
		public List<AnswerId> AnswerIds { get; set; }

		public SagaStarted(QuestionId questionId, List<AnswerId> answerIds)
		{
			QuestionId = questionId;
			AnswerIds = answerIds;
		}
	}
}
