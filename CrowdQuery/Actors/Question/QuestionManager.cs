using Akka.Actor;
using Akkatecture.Aggregates;
using Akkatecture.Commands;

namespace CrowdQuery.Actors.Question
{
	public class QuestionManager : AggregateManager<QuestionActor, QuestionId, Command<QuestionActor, QuestionId>>
	{
		protected override bool Dispatch(Command<QuestionActor, QuestionId> command)
		{
			return base.Dispatch(command);
		}

		protected override IActorRef FindOrCreate(QuestionId aggregateId)
		{
			var b = base.FindOrCreate(aggregateId);
			return b;
		}
	}
}
