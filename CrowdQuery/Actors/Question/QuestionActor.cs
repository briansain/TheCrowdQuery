using Akka.Event;
using Akkatecture.Aggregates;
using CrowdQuery.Actors.Question.Commands;
using CrowdQuery.Actors.Question.Events;

namespace CrowdQuery.Actors.Question
{
	public class QuestionActor : AggregateRoot<QuestionActor, QuestionId, QuestionState>,
		IExecute<CreateQuestion>
	{
		private readonly string _persistenceId;
		public override string PersistenceId => _persistenceId;
		public ILoggingAdapter logging { get; set; }
		private QuestionState _state { get; set; }
		public QuestionActor(QuestionId aggregateId) : base(aggregateId)
		{
			logging = Context.GetLogger();
		}

		public bool Execute(CreateQuestion command)
		{
			// SANATIZE THE QUESTION
			logging.Info($"Creating new question: {command.Question}");
			var evnt = new QuestionCreated(command.AggregateId, command.Question, command.Answers);
			Emit(evnt);
			return true;
		}
	}
}
