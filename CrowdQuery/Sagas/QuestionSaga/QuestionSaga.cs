using Akka.Actor;
using Akka.Hosting;
using Akkatecture.Aggregates;
using Akkatecture.Sagas;
using Akkatecture.Sagas.AggregateSaga;
using CrowdQuery.Actors.Question;
using CrowdQuery.Actors.Question.Events;
using CrowdQuery.Actors.Question.Query;
using CrowdQuery.Sagas.QuestionSaga.Commands;

namespace CrowdQuery.Sagas.QuestionSaga
{
	public class QuestionSaga : AggregateSaga<QuestionSaga, QuestionSagaId, QuestionSagaState>,
		ISagaIsStartedBy<QuestionActor, QuestionId, QuestionCreated>,
		ISagaHandles<QuestionActor, QuestionId, AnswerVoteIncreased>,
		ISagaHandles<QuestionActor, QuestionId, AnswerVoteDecreased>,
		IExecute<SubscribeToQuestion>
	{
		private List<IActorRef> _subscribers = new List<IActorRef>();
		private readonly IActorRef _questionManager;
		private QuestionStateProjection _softState = new QuestionStateProjection();
		public QuestionSaga(IRequiredActor<QuestionManager> questionManager)
		{
			_questionManager = questionManager.ActorRef;
			// set settings to not initialize with auto receives
			Command<QuestionState>(msg =>
			{
				_softState.Question = msg.Question;
				_softState.AnswerVotes = msg.AnswerVotes;


				InitReceives();
				InitAsyncReceives();
				return true;
			});
		}
		protected override void PreStart()
		{
			_questionManager.Tell(new QueryQuestionState(Id.ToQuestionId()));
			base.PreStart();
		}

		public bool Execute(SubscribeToQuestion command)
		{
			_subscribers.Add(command.Subscriber);
			command.Subscriber.Tell(_softState);
			// watch for subscribers to terminate
			return true;
		}

		public bool Handle(IDomainEvent<QuestionActor, QuestionId, QuestionCreated> domainEvent)
		{
			// update soft state
			// tell subscribers of the change
			throw new NotImplementedException();
		}
		public bool Handle(IDomainEvent<QuestionActor, QuestionId, AnswerVoteIncreased> domainEvent)
		{
			// update soft state
			// tell subscribers of the change
			throw new NotImplementedException();
		}

		public bool Handle(IDomainEvent<QuestionActor, QuestionId, AnswerVoteDecreased> domainEvent)
		{
			// update soft state
			// tell subscribers of the change
			throw new NotImplementedException();
		}
	}
}
