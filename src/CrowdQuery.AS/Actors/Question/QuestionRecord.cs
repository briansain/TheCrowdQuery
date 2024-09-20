namespace CrowdQuery.AS.Actors.Question
{
	public record QuestionRecord(string question, Dictionary<string, long> answerVotes)
	{
	}
}
