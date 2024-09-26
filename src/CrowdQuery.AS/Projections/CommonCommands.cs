using Akka.Actor;
using Akkatecture.Core;

namespace CrowdQuery.AS.Projections
{

    public record AddSubscriber(IActorRef Subscriber, string EntityId);
    public record RemoveSubscriber(IActorRef Subscriber, string EntityId);
    internal class UpdateSubscribers
    {
        internal static UpdateSubscribers Instance = new();
    }
}