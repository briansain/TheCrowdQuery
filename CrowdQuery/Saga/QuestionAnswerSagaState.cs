using Akkatecture.Aggregates;
using Akkatecture.Sagas;
using CrowdQuery.Actors.Answer;
using CrowdQuery.Actors.Question;
using CrowdQuery.Saga.Events;

namespace CrowdQuery.Saga
{
	public class QuestionAnswerSagaState : SagaState<QuestionAnswerSaga, QuestionAnswerSagaId, IMessageApplier<QuestionAnswerSaga, QuestionAnswerSagaId>>,
		IApply<SagaStarted>,
		IApply<SagaQuestionCreated>
	{
		public QuestionId QuestionId { get; private set; }
		public Dictionary<AnswerId, bool> AnswersCreated { get; private set; }
		public bool IsQuestionCreated { get; private set; }

		public void Apply(SagaStarted aggregateEvent)
		{
			QuestionId = aggregateEvent.QuestionId;
			AnswersCreated = aggregateEvent.AnswerIds.ToDictionary(a => a, a => false);
			IsQuestionCreated = false;
		}

		public void Apply(SagaQuestionCreated aggregateEvent)
		{
			IsQuestionCreated = true;
		}
	}
}
