using Akka.Actor;

namespace CrowdQuery.AS.Projections
{

    public record AddSubscriber(IActorRef Subscriber);
    public record RemoveSubscriber(IActorRef Subscriber);
    internal class UpdateSubscribers
    {
        internal static UpdateSubscribers Instance = new();
    }
}