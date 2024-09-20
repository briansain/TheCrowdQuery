using Akka.Actor;
using Akka.DistributedData;
using Akka.Event;
using CrowdQuery.Messages;

namespace CrowdQuery.AS.Actors
{
	public class AllQuestionsActor : ReceiveActor
	{
		public static LWWDictionaryKey<string, BasicQuestionState> AllQuestionsBasicKey = new("all-questions-basics");

		private ILoggingAdapter logger => Context.GetLogger();
		private LWWDictionary<string, BasicQuestionState> _state =
				LWWDictionary<string, BasicQuestionState>.Empty;
		public AllQuestionsActor()
		{
			Receive<IGetResponse>(HandleGetResponse);
			Receive<Changed>(msg =>
			{
				logger.Debug($"AllQuestionsActor: Received changed");
				var newData = msg.Get(AllQuestionsBasicKey);
				_state.Merge(newData);
			});
			Receive<RequestBasicQuestionState>(msg =>
			{
				logger.Debug($"AllQuestionsActor: Received requestBasicQuestionState");
				Sender.Tell(_state.Values.ToList());
			});
		}

		public bool HandleGetResponse(IGetResponse response)
		{
			switch (response)
			{
				case GetSuccess resp:
					if (resp.IsSuccessful)
					{
						logger.Debug("AllQuestionsActor: successfully updated BasicQuestionState");

						var newData = resp.Get(AllQuestionsBasicKey);
						_state.Merge(newData);
					}
					else
					{
						logger.Warning("AllQuestionsActor: failed to update BasicQuestionState");
					}
					break;
				case NotFound notFound:
					logger.Info("AllQuestionsActor: recieved NotFound");
					break;
				case GetFailure failure:
					logger.Warning("AllQuestionsActor: received GetFailure");
					break;
				case DataDeleted dataDeleted:
					logger.Warning("AllQuestionsActor: received DataDeleted");
					break;
			}

			return true;
		}

		protected override void PreStart()
		{
			var replicator = DistributedData.Get(Context.System).Replicator;
			replicator.Tell(Dsl.Get(AllQuestionsBasicKey, ReadLocal.Instance));
			replicator.Tell(Dsl.Subscribe(AllQuestionsBasicKey, Self));
			base.PreStart();
		}
	}
}
