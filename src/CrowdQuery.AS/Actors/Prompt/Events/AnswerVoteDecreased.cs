using Akkatecture.Aggregates;

namespace CrowdQuery.AS.Actors.Prompt.Events
{
	public class AnswerVoteDecreased : AggregateEvent<PromptActor, PromptId>
	{
		public string Answer { get; set; }
		public AnswerVoteDecreased(string answer)
		{
			Answer = answer;
		}
	}
}
