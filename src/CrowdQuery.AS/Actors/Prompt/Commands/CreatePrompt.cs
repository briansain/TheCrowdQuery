using Akkatecture.Commands;

namespace CrowdQuery.AS.Actors.Prompt.Commands
{
	public class CreatePrompt : Command<PromptActor, PromptId>
	{
		public string Prompt { get; set; }
		public List<string> Answers { get; set; }

		public CreatePrompt(PromptId aggregateId, string prompt, List<string> answers) : base(aggregateId)
		{
			Prompt = prompt;
			Answers = answers;
		}
	}
}
