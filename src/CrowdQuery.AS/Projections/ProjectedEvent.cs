using System.Net;
using Akkatecture.Aggregates;
using Akkatecture.Core;

namespace CrowdQuery.AS.Projections
{
    public class ProjectedEvent<TAggregateEvent, TAggregateId>
        where TAggregateEvent : IAggregateEvent
        where TAggregateId : IIdentity
    {
        public TAggregateEvent AggregateEvent {get;set;}
        public TAggregateId AggregateId {get;set;} 
        public long SequenceNumber {get;set;}

        public ProjectedEvent(TAggregateEvent evnt, TAggregateId aggregateId, long sequenceNumber)
        {
            AggregateEvent = evnt;
            AggregateId = aggregateId;
            SequenceNumber = sequenceNumber;
        }
    }
}