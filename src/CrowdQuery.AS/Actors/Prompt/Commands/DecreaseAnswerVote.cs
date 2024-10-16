using Akkatecture.Commands;

namespace CrowdQuery.AS.Actors.Prompt.Commands
{
	public class DecreaseAnswerVote : Command<PromptActor, PromptId>
	{
		public string Answer { get; set; }
		public DecreaseAnswerVote(PromptId aggregateId, string answer) : base(aggregateId)
		{
			Answer = answer;
		}
	}
}
