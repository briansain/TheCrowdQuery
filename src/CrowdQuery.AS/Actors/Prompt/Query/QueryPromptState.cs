using Akkatecture.Commands;

namespace CrowdQuery.AS.Actors.Prompt.Query
{
	public class QueryPromptState : Command<PromptActor, PromptId>
	{
		public QueryPromptState(PromptId aggregateId) : base(aggregateId)
		{
		}
	}
}
