using Akka.Actor;
using Akkatecture.Sagas.AggregateSaga;
using System.Linq.Expressions;

namespace CrowdQuery.Saga
{
	public class QuestionAnswerManager : AggregateSagaManager<QuestionAnswerSaga, QuestionAnswerSagaId, QuestionAnswerSagaLocator>
	{
		public QuestionAnswerManager(Expression<Func<QuestionAnswerSaga>> sagaFactory) : base(sagaFactory)
		{
			Receive((Func<Commands.StartSaga, bool>)Handle);
		}

		public bool Handle(Commands.StartSaga cmd)
		{
			var childId = FindOrSpawn(cmd.SagaId);
			childId.Forward(cmd);
			return true;
		}
	}
}
