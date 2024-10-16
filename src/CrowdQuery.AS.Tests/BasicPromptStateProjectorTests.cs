using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Akka.DistributedData;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using CrowdQuery.AS.Actors.Prompt;
using CrowdQuery.AS.Projections.BasicPromptStateProjection;
using FluentAssertions;
using Xunit;

namespace CrowdQuery.AS.Tests
{
    public class BasicPromptStateProjectorTests : TestKit
    {
        private readonly TestProbe _testProbe;
        private BasicPromptStateConfiguration _config;
        public BasicPromptStateProjectorTests() : base(ConfigurationFactory.ParseString(
            @"	akka.loglevel = DEBUG
            	akka.actor.provider = cluster")
            .WithFallback(DistributedData.DefaultConfig()))
        {
            _testProbe = CreateTestProbe();
            _config = new BasicPromptStateConfiguration(1);
        }

        [Fact]
        public async Task Receives_GetBasicPromptState_Responds_EmptyDictionary()
        {
            var basicProjector = Sys.ActorOf(BasicPromptStateProjector.PropsFor(_config), "basic-projector");
            var response = await basicProjector.Ask(new GetBasicPromptState());
            response.Should().BeOfType(typeof(Dictionary<string, BasicPromptState>));
            var responseData = (Dictionary<string, BasicPromptState>)response;
            responseData.Count.Should().Be(0);
        }

        [Fact]
        public async Task With_UpdatedDistributedData_Receives_GetBasicPromptState_Responds_UpdatedState()
        {
            var promptId = PromptId.New;
            var basicProjector = Sys.ActorOf(BasicPromptStateProjector.PropsFor(_config), "basic-projector");
            var replicator = DistributedData.Get(Sys).Replicator;
            replicator.Tell(Dsl.Update(
                BasicPromptStateProjector.Key,
                LWWDictionary<string, BasicPromptState>.Empty,
                WriteLocal.Instance,
                state => state.SetItem(
                    Cluster.Get(Sys),
                    promptId.ToBase64(),
                    new BasicPromptState("Are you there?", 2, 0))));

            await AwaitAssertAsync(async () =>
            {
                var response = await basicProjector.Ask(new GetBasicPromptState());
                response.Should().BeOfType(typeof(Dictionary<string, BasicPromptState>));
                var responseData = (Dictionary<string, BasicPromptState>)response;
                responseData.Count.Should().Be(1);
                var projectedData = responseData[promptId.ToBase64()];
                projectedData.prompt.Should().Be("Are you there?");
                projectedData.answerCount.Should().Be(2);
                projectedData.totalVotes.Should().Be(0);
            });
        }
    }
}