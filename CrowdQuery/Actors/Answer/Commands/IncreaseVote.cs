using Akkatecture.Commands;

namespace CrowdQuery.Actors.Answer.Commands
{
	public class IncreaseVote : Command<AnswerActor, AnswerId>
	{
		public IncreaseVote(AnswerId aggregateId) : base(aggregateId)
		{

		}
	}
}
