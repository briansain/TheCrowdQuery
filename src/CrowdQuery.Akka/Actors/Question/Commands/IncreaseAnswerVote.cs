using Akkatecture.Commands;

namespace CrowdQuery.Akka.Actors.Question.Commands
{
	public class IncreaseAnswerVote : Command<QuestionActor, QuestionId>
	{
		public string Answer { get; set; }
		public IncreaseAnswerVote(QuestionId aggregateId, string answer) : base(aggregateId)
		{
			Answer = answer;
		}
	}
}
