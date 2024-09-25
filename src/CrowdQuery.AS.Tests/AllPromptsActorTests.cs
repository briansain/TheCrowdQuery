// using System;
// using System.Collections.Immutable;
// using Akka.Actor;
// using Akka.Cluster;
// using Akka.Configuration;
// using Akka.DistributedData;
// using Akka.TestKit;
// using Akka.TestKit.Xunit2;
// using CrowdQuery.AS.Actors.AllPromptsActor;
// using CrowdQuery.AS.Actors.Prompt;
// using CrowdQuery.Messages;
// using FluentAssertions;
// using Xunit;

// namespace CrowdQuery.AS.Tests;

// public class AllPromptsActorTests: TestKit
// {
//     private readonly TestProbe _testProbe;
//     public AllPromptsActorTests() : base(ConfigurationFactory.ParseString(
// 			@"	akka.loglevel = DEBUG
//             	akka.actor.provider = cluster")
// 			.WithFallback(DistributedData.DefaultConfig()))
//     {
//         _testProbe = CreateTestProbe();
//     }

//     [Fact]
//     public void SubscribesTo_DDataUpdate_Returns_NewData()
//     {
//         var promptId = PromptId.New;
//         var allPromptsActor = Sys.ActorOf(AllPromptsActor.PropsFor(), "all-prompts-actor");
//         var replicator = DistributedData.Get(Sys).Replicator;
//         replicator.Tell(Dsl.Update(AllPromptsActor.AllPromptsBasicKey, new WriteAll(TimeSpan.FromSeconds(3)), 
//             d => d.SetItem(Cluster.Get(Sys), promptId.ToBase64(), new BasicPromptState("Are you there", 2, 100))), _testProbe);

//         var expected = new Dictionary<string, BasicPromptState>()
//         {
//             {promptId.ToBase64(), new BasicPromptState("Are you there", 2, 100)}
//         }.ToImmutableDictionary();

//         AwaitAssertAsync(async () => {
//             var data = await allPromptsActor.Ask(new RequestBasicPromptState());
//             data.Should().BeOfType<ImmutableDictionary<string, BasicPromptState>>();
//             data.Should().Equals(expected);
//         });
//     }

// }
