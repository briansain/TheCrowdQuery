using Akka.Actor;
using Akkatecture.Commands;

namespace CrowdQuery.Sagas.QuestionSaga.Commands
{
	public class SubscribeToQuestion : Command<QuestionSaga, QuestionSagaId>
	{
		public IActorRef Subscriber { get; set; }
		public SubscribeToQuestion(QuestionSagaId aggregateId, IActorRef subscriber) : base(aggregateId)
		{
			Subscriber = subscriber;
		}
	}
}
