using Akka.Actor;
using Akka.Event;
using Akkatecture.Aggregates;
using Akkatecture.Aggregates.CommandResults;
using CrowdQuery.Actors.Question.Commands;
using CrowdQuery.Actors.Question.Events;
using CrowdQuery.Actors.Specification;

namespace CrowdQuery.Actors.Question
{
	public class QuestionActor : AggregateRoot<QuestionActor, QuestionId, QuestionState>,
		IExecute<CreateQuestion>
	{
		public IsNotNewSpecification IsNotNew => new IsNotNewSpecification();
		public ILoggingAdapter logging { get; set; }
		private QuestionState _state { get; set; }
		public QuestionActor(QuestionId aggregateId) : base(aggregateId)
		{
			logging = Context.GetLogger();
		}

		public bool Execute(CreateQuestion command)
		{
			if (IsNotNew.IsSatisfiedBy(IsNew))
			{
				// SANATIZE THE QUESTION FOR SQL INJECTION AND SILLY HACKERS
				logging.Info($"Creating new question: {command.Question}");
				var evnt = new QuestionCreated(command.AggregateId, command.Question, command.Answers);
				Emit(evnt);
				return true;
			}

			Sender.Tell(CommandResult.FailWith(command, IsNotNew.WhyIsNotSatisfiedBy(IsNew)), Self);
			return true;
		}
	}
}
