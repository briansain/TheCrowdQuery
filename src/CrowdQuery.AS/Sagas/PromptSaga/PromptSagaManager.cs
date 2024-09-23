using System;
using System.Linq.Expressions;
using Akka.Actor;
using Akkatecture.Aggregates;
using Akkatecture.Sagas;
using Akkatecture.Sagas.AggregateSaga;
using CrowdQuery.AS.Actors.Prompt;

namespace CrowdQuery.AS.Sagas.PromptSaga;

public class PromptSagaManager : AggregateSagaManager<PromptSaga, PromptSagaId, PromptSagaLocator>
{
    public PromptSagaManager(Expression<Func<PromptSaga>> sagaFactory) : base(sagaFactory)
    {
    }

    public static Props PropsFor(Func<PromptSaga> sagaFactory)
    {
        return Props.Create<PromptSagaManager>(sagaFactory);
    }
}

public class PromptSagaLocator : ISagaLocator<PromptSagaId>
{
    public PromptSagaId LocateSaga(IDomainEvent domainEvent)
    {
        if (domainEvent is IDomainEvent<PromptActor, PromptId> q)
        {
            return new PromptSagaId(q.AggregateIdentity.Value);
        }

        throw new Exception($"Unable to location PromptSagaId with message type {domainEvent.GetType()}");
    }
}
