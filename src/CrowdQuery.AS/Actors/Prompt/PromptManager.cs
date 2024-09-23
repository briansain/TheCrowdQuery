using Akka.Actor;
using Akkatecture.Aggregates;
using Akkatecture.Commands;

namespace CrowdQuery.AS.Actors.Prompt
{
	public class PromptManager : AggregateManager<PromptActor, PromptId, Command<PromptActor, PromptId>>
	{
		protected override bool Dispatch(Command<PromptActor, PromptId> command)
		{
			return base.Dispatch(command);
		}

		protected override IActorRef FindOrCreate(PromptId aggregateId)
		{
			var b = base.FindOrCreate(aggregateId);
			return b;
		}

		public static Props PropsFor()
		{
			return Props.Create<PromptManager>();
		}
	}
}
