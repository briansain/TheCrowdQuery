using Akka.Event;
using Akka.Persistence;
using CrowdQuery.Actors.Question.Commands;
using CrowdQuery.Actors.Question.Events;

namespace CrowdQuery.Actors.Question
{
	//CONVERT TO AKKATECTURE
	public class QuestionActor : ReceivePersistentActor
	{
		private readonly string _persistenceId;
		public override string PersistenceId => _persistenceId;
		public ILoggingAdapter logging { get; set; }
		private QuestionState _state { get; set; }
		public QuestionActor(string persistenceId)
		{
			_persistenceId = persistenceId;
			logging = Context.GetLogger();
			_state = new QuestionState();
			Command<CreateQuestion>(msg =>
			{
				// SANATIZE THE QUESTION
				logging.Info($"Creating new question: {msg.Question}");
				var evnt = new QuestionCreated(msg.QuestionId, msg.Question, msg.Answers);
				Persist(evnt, HandleQuestionCreated);
			});
			Recover<QuestionCreated>(HandleQuestionCreated);
		}

		public void HandleQuestionCreated(QuestionCreated evnt)
		{
			_state = new QuestionState(evnt.Question, evnt.Answers);
		}
	}
}
