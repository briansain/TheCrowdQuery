namespace CrowdQuery.AS.Actors.Prompt
{
	public record PromptRecord(string Prompt, Dictionary<string, long> answerVotes)
	{
	}
}
