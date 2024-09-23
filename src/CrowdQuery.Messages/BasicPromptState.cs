namespace CrowdQuery.Messages
{
	public record BasicPromptState(string PromptId, string Prompt, int answerCount, int totalVotes)
	{

	}
}
