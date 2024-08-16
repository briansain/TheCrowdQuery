using Akka.Actor;
using Akka.Event;

namespace Akka.Skeleton.Persistence.Actors
{
    internal class EchoActor : ReceiveActor
    {
        public EchoActor()
        {
            Receive<string>(msg =>
            {
                Context.GetLogger().Info($"EchoActor: Received {msg}");
                Sender.Tell(msg);
            });
            Receive<int>(msg =>
            {
                Context.GetLogger().Info($"EchoActor: Received {msg}");
                Sender.Tell(msg);
            });
            Receive<object>(msg =>
            {
                Context.GetLogger().Error($"EchoActor: Received Unsupported Type {msg.GetType().Name}");
                Self.Tell(PoisonPill.Instance);
            });

            Context.ActorOf(Props.Create<LoggingActor>(), "logging-1");
        }
    }

    internal class LoggingActor : ReceiveActor
    {
        public LoggingActor()
        {
			Receive<string>(msg =>
			{
				Context.GetLogger().Info($"LoggingActor: Received {msg}");
			});
			Receive<int>(msg =>
			{
				Context.GetLogger().Info($"LoggingActor: Received {msg}");
			});
			Receive<object>(msg =>
			{
				Context.GetLogger().Error($"LoggingActor: Received Unsupported Type {msg.GetType().Name}");
			});
		}
    }
}
