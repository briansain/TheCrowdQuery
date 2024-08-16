using Akka.Actor;
using Akka.Hosting;
using Akkatecture.Aggregates;
using Akkatecture.Sagas;
using Akkatecture.Sagas.AggregateSaga;
using CrowdQuery.Actors.Answer;
using CrowdQuery.Actors.Answer.Commands;
using CrowdQuery.Actors.Answer.Events;
using CrowdQuery.Actors.Question;
using CrowdQuery.Actors.Question.Commands;
using CrowdQuery.Actors.Question.Events;
using CrowdQuery.Saga.Events;

namespace CrowdQuery.Saga
{
	public class QuestionAnswerSaga : AggregateSaga<QuestionAnswerSaga, QuestionAnswerSagaId, QuestionAnswerSagaState>,
		ISagaHandles<QuestionActor, QuestionId, QuestionCreated>,
		ISagaHandles<AnswerActor, AnswerId, AnswerCreated>
	{
		private readonly IActorRef _questionManager;
		private readonly IActorRef _answerManager;

		public QuestionAnswerSaga(IRequiredActor<QuestionManager> questionManager, IRequiredActor<AnswerManager> answerManager)
		{
			Command((Func<Commands.StartSaga, bool>)Handle);
			_questionManager = questionManager.ActorRef;
			_answerManager = answerManager.ActorRef;
		}

		public bool Handle(Commands.StartSaga cmd)
		{
			var questionId = QuestionId.NewDeterministic(QuestionId.Namespace, cmd.Question);
			var createQuestion = new CreateQuestion(questionId, cmd.Question, cmd.Answers);
			var answerIds = new List<AnswerId>();
			_questionManager.Tell(createQuestion);
			foreach (var answer in cmd.Answers)
			{
				var answerId = AnswerId.NewDeterministic(AnswerId.Namespace, $"{questionId}-{answer}");
				var createAnswer = new CreateAnswer(answerId, questionId, answer);
				_answerManager.Tell(createAnswer);
				answerIds.Add(answerId);
			}
			var evnt = new SagaStarted(questionId, answerIds);
			Emit(evnt);
			return true;
		}

		public bool Handle(IDomainEvent<AnswerActor, AnswerId, AnswerCreated> domainEvent)
		{
			Emit(new SagaAnswerCreated(domainEvent.AggregateIdentity));
			return true;
		}

		public bool Handle(IDomainEvent<QuestionActor, QuestionId, QuestionCreated> domainEvent)
		{
			Emit(new SagaQuestionCreated(domainEvent.AggregateIdentity));
			return true;
		}
	}
}
