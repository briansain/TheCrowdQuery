using Akkatecture.Aggregates;
using Akkatecture.Sagas;

namespace CrowdQuery.Sagas.QuestionSaga
{
	public class QuestionSagaState : SagaState<QuestionSaga, QuestionSagaId, IMessageApplier<QuestionSaga, QuestionSagaId>>
	{

	}
}
