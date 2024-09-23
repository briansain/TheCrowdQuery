using System;
using System.Linq.Expressions;
using Akka.Actor;
using Akkatecture.Aggregates;
using Akkatecture.Sagas;
using Akkatecture.Sagas.AggregateSaga;
using CrowdQuery.AS.Actors.Question;

namespace CrowdQuery.AS.Sagas.QuestionSaga;

public class QuestionSagaManager : AggregateSagaManager<QuestionSaga, QuestionSagaId, QuestionSagaLocator>
{
    public QuestionSagaManager(Expression<Func<QuestionSaga>> sagaFactory) : base(sagaFactory)
    {
    }

    public static Props PropsFor(Func<QuestionSaga> sagaFactory)
    {
        return Props.Create<QuestionSagaManager>(sagaFactory);
    }
}

public class QuestionSagaLocator : ISagaLocator<QuestionSagaId>
{
    public QuestionSagaId LocateSaga(IDomainEvent domainEvent)
    {
        if (domainEvent is IDomainEvent<QuestionActor, QuestionId> q)
        {
            return new QuestionSagaId(q.AggregateIdentity.Value);
        }

        throw new Exception($"Unable to location QuestionSagaId with message type {domainEvent.GetType()}");
    }
}
