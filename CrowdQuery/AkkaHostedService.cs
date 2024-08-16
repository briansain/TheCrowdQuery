using Akka.Actor;
using Akka.Hosting;
using Akka.Skeleton.Persistence.Actors;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Akka.Skeleton.Persistence
{
    internal class AkkaHostedService : IHostedService
    {
        private ActorRegistry _registry;
        private ActorSystem _system;
        public AkkaHostedService(ActorSystem actorSystem, ActorRegistry actorRegistry)
        {
            _registry = actorRegistry;
            _system = actorSystem;
        }
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            Log.Error("Entered StopAsync");
            return Task.CompletedTask;
        }
    }
}
