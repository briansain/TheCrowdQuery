using Akkatecture.Commands;

namespace CrowdQuery.AS.Actors.Prompt.Commands
{
	public class IncreaseAnswerVote : Command<PromptActor, PromptId>
	{
		public string Answer { get; set; }
		public IncreaseAnswerVote(PromptId aggregateId, string answer) : base(aggregateId)
		{
			Answer = answer;
		}
	}
}
