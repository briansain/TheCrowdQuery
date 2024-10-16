using Akkatecture.Aggregates;

namespace CrowdQuery.AS.Actors.Prompt.Events
{
	public class PromptCreated : AggregateEvent<PromptActor, PromptId>
	{
		public string Prompt { get; set; }
		public List<string> Answers { get; set; }

		public PromptCreated(string prompt, List<string> answers)
		{
			Prompt = prompt;
			Answers = answers;
		}
	}
}
