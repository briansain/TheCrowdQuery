using Akkatecture.Commands;

namespace CrowdQuery.Actors.Answer.Commands
{
	public class DecreaseVote: Command<AnswerActor, AnswerId>
	{
		public DecreaseVote(AnswerId answerId): base(answerId)
		{

		}
	}
}
