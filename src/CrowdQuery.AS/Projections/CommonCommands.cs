using Akka.Actor;
using Akkatecture.Core;

namespace CrowdQuery.AS.Projections
{
    public record AddSubscriber(IActorRef Subscriber, string ProjectorId);
    public record RemoveSubscriber(IActorRef Subscriber, string ProjectorId);
    internal class UpdateSubscribers
    {
        internal static UpdateSubscribers Instance = new();
    }
}