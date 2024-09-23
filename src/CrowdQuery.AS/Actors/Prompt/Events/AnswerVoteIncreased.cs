using Akkatecture.Aggregates;

namespace CrowdQuery.AS.Actors.Prompt.Events
{
	public class AnswerVoteIncreased : AggregateEvent<PromptActor, PromptId>
	{
		public string Answer { get; set; }
		public AnswerVoteIncreased(string answer)
		{
			Answer = answer;
		}
	}
}
