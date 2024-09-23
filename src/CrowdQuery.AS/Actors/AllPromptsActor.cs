using Akka.Actor;
using Akka.DistributedData;
using Akka.Event;
using CrowdQuery.Messages;

namespace CrowdQuery.AS.Actors
{
	public class AllPromptsActor : ReceiveActor
	{
		public static LWWDictionaryKey<string, BasicPromptState> AllPromptsBasicKey = new("all-Prompts-basics");

		private ILoggingAdapter logger => Context.GetLogger();
		private LWWDictionary<string, BasicPromptState> _state =
				LWWDictionary<string, BasicPromptState>.Empty;
		public AllPromptsActor()
		{
			Receive<IGetResponse>(HandleGetResponse);
			Receive<Changed>(msg =>
			{
				logger.Debug($"AllPromptsActor: Received changed");
				var newData = msg.Get(AllPromptsBasicKey);
				_state.Merge(newData);
			});
			Receive<RequestBasicPromptState>(msg =>
			{
				logger.Debug($"AllPromptsActor: Received requestBasicPromptState");
				Sender.Tell(_state.Values.ToList());
			});
		}

		public static Props PropsFor()
		{
			return Props.Create<AllPromptsActor>();
		}

		public bool HandleGetResponse(IGetResponse response)
		{
			switch (response)
			{
				case GetSuccess resp:
					if (resp.IsSuccessful)
					{
						logger.Debug("AllPromptsActor: successfully updated BasicPromptState");

						var newData = resp.Get(AllPromptsBasicKey);
						_state.Merge(newData);
					}
					else
					{
						logger.Warning("AllPromptsActor: failed to update BasicPromptState");
					}
					break;
				case NotFound notFound:
					logger.Info("AllPromptsActor: recieved NotFound");
					break;
				case GetFailure failure:
					logger.Warning("AllPromptsActor: received GetFailure");
					break;
				case DataDeleted dataDeleted:
					logger.Warning("AllPromptsActor: received DataDeleted");
					break;
			}

			return true;
		}

		protected override void PreStart()
		{
			var replicator = DistributedData.Get(Context.System).Replicator;
			replicator.Tell(Dsl.Get(AllPromptsBasicKey, ReadLocal.Instance));
			replicator.Tell(Dsl.Subscribe(AllPromptsBasicKey, Self));
			base.PreStart();
		}
	}
}
