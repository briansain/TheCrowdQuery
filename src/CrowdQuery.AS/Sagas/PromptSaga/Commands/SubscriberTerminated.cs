using Akka.Actor;

namespace CrowdQuery.AS.Sagas.PromptSaga.Commands
{
	public class SubscriberTerminated
	{
		public IActorRef Subscriber;
		public SubscriberTerminated(IActorRef subscriber)
		{
			Subscriber = subscriber;
		}
	}
}
