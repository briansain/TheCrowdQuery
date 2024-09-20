using Akkatecture.Sagas;
using CrowdQuery.AS.Actors.Question;

namespace CrowdQuery.AS.Sagas.QuestionSaga
{
	public class QuestionSagaId : SagaId<QuestionSagaId>
	{
		public static Guid Namespace => new Guid("c67a7d3e-0bf1-470f-a2af-6b1a6c18706f");

		public QuestionSagaId(string value) : base(value)
		{
		}

		public QuestionId ToQuestionId()
		{
			return QuestionId.With(Value.Replace("questionsaga-", ""));
		}
	}
}
