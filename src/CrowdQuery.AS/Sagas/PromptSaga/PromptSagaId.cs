using Akkatecture.Core;
using Akkatecture.Sagas;
using CrowdQuery.AS.Actors.Prompt;

namespace CrowdQuery.AS.Sagas.PromptSaga
{
	public class PromptSagaId : SagaId<PromptSagaId>
	{
		public static Guid Namespace => new Guid("c67a7d3e-0bf1-470f-a2af-6b1a6c18706f");

		public PromptSagaId(string value) : base(value)
		{
		}

		public PromptId ToPromptId()
		{
			return PromptId.With(Value.Replace("promptsaga-", ""));
		}
	}
}
