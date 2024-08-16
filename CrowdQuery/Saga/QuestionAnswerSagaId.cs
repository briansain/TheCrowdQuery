using Akkatecture.Sagas;

namespace CrowdQuery.Saga
{
	public class QuestionAnswerSagaId : SagaId<QuestionAnswerSagaId>
	{
		public static Guid Namespace => new Guid("c67a7d3e-0bf1-470f-a2af-6b1a6c18706f");

		public QuestionAnswerSagaId(string aggregateId) : base(aggregateId)
		{

		}
	}
}
