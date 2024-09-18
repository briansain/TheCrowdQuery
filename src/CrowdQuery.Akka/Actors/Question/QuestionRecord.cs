namespace CrowdQuery.Akka.Actors.Question
{
	public record QuestionRecord(string question, Dictionary<string, long> answerVotes)
	{
	}
}
