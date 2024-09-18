using Akka.Actor;

namespace CrowdQuery.Akka.Sagas.QuestionSaga.Commands
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
