namespace CrowdQuery.Actors.Question
{
	public record QuestionRecord(string question, Dictionary<string, long> answerVotes)
	{
	}
}
