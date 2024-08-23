using Akka.Actor;
using Akka.Event;
using Akkatecture.Aggregates;
using CrowdQuery.Actors.Question.Commands;
using CrowdQuery.Actors.Question.Events;

namespace CrowdQuery.Actors.Question
{
	public class QuestionActor : AggregateRoot<QuestionActor, QuestionId, QuestionState>,
		IExecute<CreateQuestion>
	{
		public ILoggingAdapter logging { get; set; }
		private QuestionState _state { get; set; }
		public QuestionActor(QuestionId aggregateId) : base(aggregateId)
		{
			logging = Context.GetLogger();
		}

		protected override bool AroundReceive(Receive receive, object message)
		{
			return base.AroundReceive(receive, message);
		}

		public bool Execute(CreateQuestion command)
		{
			// SANATIZE THE QUESTION FOR SQL INJECTION AND SILLY HACKERS
			logging.Info($"Creating new question: {command.Question}");
			var evnt = new QuestionCreated(command.AggregateId, command.Question, command.Answers);
			Emit(evnt);
			return true;
		}
	}
}
