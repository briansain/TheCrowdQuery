using Akkatecture.Aggregates;
using CrowdQuery.Actors.Answer.Commands;
using CrowdQuery.Actors.Answer.Events;

namespace CrowdQuery.Actors.Answer
{
	public class AnswerActor : AggregateRoot<AnswerActor, AnswerId, AnswerState>,
		IExecute<CreateAnswer>,
		IExecute<IncreaseVote>,
		IExecute<DecreaseVote>
	{
		public AnswerActor(AnswerId aggregateId) : base(aggregateId)
		{
		}

		public bool Execute(CreateAnswer msg)
		{
			var evnt = new AnswerCreated(msg.QuestionId, msg.Answer);
			Emit(evnt);
			return true;
		}

		public bool Execute(IncreaseVote command)
		{
			var evnt = new VoteIncreased();
			Emit(evnt);
			return true;
		}

		public bool Execute(DecreaseVote command)
		{
			var evnt = new VoteDecreased();
			Emit(evnt);
			return true;
		}
	}
}
