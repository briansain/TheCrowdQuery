using Akka.Event;
using Akka.Persistence;

namespace Akka.Skeleton.Persistence.Actors
{
    internal class StatefulActor : ReceivePersistentActor
    {
        public override string PersistenceId => "single-stateful-actor";
        private readonly StatefulActorState state;

        public StatefulActor()
        {
            state = new StatefulActorState();
            Recover<string>(msg =>
            {
                HandleMessageString(msg);
                Context.GetLogger().Info($"StatefulActor: Recovering from EventJournal");
            });
            Command<string>(msg =>
            {
                Persist(msg, HandleMessageString);
                Context.GetLogger().Info($"StatefulActor: Received new message \"{msg}\"");
            });
        }
        private void HandleMessageString(string msg)
        {
            state.Messages.Add(msg);
            Context.GetLogger().Info($"state.Messages.Count = {state.Messages.Count}");
        }
    }

    internal class StatefulActorState
    {
        internal List<string> Messages = new List<string>();
    }
}
