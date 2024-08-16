using Akkatecture.Aggregates;
using Akkatecture.Sagas;
using CrowdQuery.Actors.Answer;
using CrowdQuery.Actors.Answer.Events;
using CrowdQuery.Actors.Question;
using CrowdQuery.Actors.Question.Events;

namespace CrowdQuery.Saga
{
	public class QuestionAnswerSagaLocator : ISagaLocator<QuestionAnswerSagaId>
	{
		public QuestionAnswerSagaId LocateSaga(IDomainEvent domainEvent)
		{
			switch (domainEvent)
			{
				case DomainEvent<QuestionActor, QuestionId, QuestionCreated> evnt:
					return new QuestionAnswerSagaId($"questionanswer-{evnt.AggregateIdentity}");
				case DomainEvent<AnswerActor, AnswerId, AnswerCreated> evnt:
					return new QuestionAnswerSagaId($"questionanswer-{evnt.AggregateEvent.QuestionId}");
				default:
					throw new ArgumentException($"Could not find QuestionAnswerSagaId with type {domainEvent.GetType()}");
			}
		}
	}
}
