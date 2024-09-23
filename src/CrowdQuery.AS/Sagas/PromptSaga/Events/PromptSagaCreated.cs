using Akkatecture.Aggregates;

namespace CrowdQuery.AS.Sagas.PromptSaga.Events
{
	public class PromptSagaCreated : AggregateEvent<PromptSaga, PromptSagaId>
	{
		public string Prompt { get; set; }
		public List<string> Answers { get; set; }

		public PromptSagaCreated(string Prompt, List<string> answers)
		{
			Prompt = Prompt;
			Answers = answers;
		}
	}
}
