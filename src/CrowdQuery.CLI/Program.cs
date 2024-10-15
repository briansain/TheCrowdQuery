using Microsoft.Extensions.Hosting;
using CrowdQuery.AS;
using Serilog;
using static CrowdQuery.AS.ServiceCollectionExtension;
using Akka.Hosting;
using CrowdQuery.AS.Projections.PromptProjection;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Actor;


var builder = Host.CreateApplicationBuilder(args);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Services.AddSerilog();
builder.Services
    .AddCrowdQueryAkka(builder.Configuration, [ClusterConstants.ProjectionProxyNode]);
var host = builder.Build();
host.Start();

var actorSystem = (ActorSystem)host.Services.GetService(typeof(ActorSystem))!;

var distPubSub = DistributedPubSub.Get(actorSystem);
var requiredActor = host.Services.GetService(typeof(IRequiredActor<PromptProjector>)) as IRequiredActor<PromptProjector>;

// requiredActor.ActorRef.Tell(new ProjectedEvent<PromptCreated, PromptId>(new PromptCreated(), PromptId.With(""), 1))
Console.WriteLine("Hello World");


/*
var projectedEvent = new ProjectedEvent<AnswerVoteDecreased, PromptId>(evnt, Id, Version);
var pubSub = DistributedPubSub.Get(Context.System);
pubSub.Mediator.Tell(new Publish(ProjectionConstants.AnswerVoteDecreased, projectedEvent, true));
pubSub.Mediator.Tell(new Publish(ProjectionConstants.AnswerVoteDecreased, projectedEvent, false));
*/