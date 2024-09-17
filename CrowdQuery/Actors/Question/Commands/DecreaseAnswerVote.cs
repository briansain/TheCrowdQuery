using Akkatecture.Commands;

namespace CrowdQuery.Actors.Question.Commands
{
	public class DecreaseAnswerVote : Command<QuestionActor, QuestionId>
	{
		public string Answer { get; set; }
		public DecreaseAnswerVote(QuestionId aggregateId, string answer) : base(aggregateId)
		{
			Answer = answer;
		}
	}
}
