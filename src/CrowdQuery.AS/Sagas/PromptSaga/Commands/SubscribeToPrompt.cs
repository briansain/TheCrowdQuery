using Akka.Actor;
using Akkatecture.Commands;

namespace CrowdQuery.AS.Sagas.PromptSaga.Commands
{
	public class SubscribeToPrompt : Command<PromptSaga, PromptSagaId>
	{
		public IActorRef Subscriber { get; set; }
		public SubscribeToPrompt(PromptSagaId aggregateId, IActorRef subscriber) : base(aggregateId)
		{
			Subscriber = subscriber;
		}
	}
}
