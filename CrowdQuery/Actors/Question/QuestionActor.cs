using Akka.Actor;
using Akka.Event;
using Akkatecture.Aggregates;
using Akkatecture.Aggregates.CommandResults;
using CrowdQuery.Actors.Question.Commands;
using CrowdQuery.Actors.Question.Events;
using CrowdQuery.Actors.Question.Specification;

namespace CrowdQuery.Actors.Question
{
    public class QuestionActor : AggregateRoot<QuestionActor, QuestionId, QuestionState>,
		IExecute<CreateQuestion>,
		IExecute<IncreaseAnswerVote>, IExecute<DecreaseAnswerVote>
	{
		private static IsNewSpecification IsNewSpec => new IsNewSpecification();
		private static IsNotNewSpecification IsNotNewSpec => new IsNotNewSpecification();
		public ILoggingAdapter logging { get; set; }
		public QuestionActor(QuestionId aggregateId) : base(aggregateId)
		{
			logging = Context.GetLogger();
		}

		public bool Execute(CreateQuestion command)
		{
			if (IsNewSpec.IsSatisfiedBy(base.IsNew))
			{
				// SANATIZE THE QUESTION FOR SQL INJECTION AND SILLY HACKERS
				logging.Info($"Creating new question: {command.Question}");
				var evnt = new QuestionCreated(command.AggregateId, command.Question, command.Answers);
				base.Emit(evnt);
				return true;
			}

			Sender.Tell(CommandResult.FailWith(command, IsNewSpec.WhyIsNotSatisfiedBy(base.IsNew)), Self);
			return true;
		}

		public bool Execute(IncreaseAnswerVote command)
		{
			var containsAnswerSpec = new ContainsAnswerSpecification(State);
			if (IsNotNewSpec.IsSatisfiedBy(IsNew) && containsAnswerSpec.IsSatisfiedBy(command.Answer))
			{
				logging.Info($"Increasing Answer Vote");
				var evnt = new AnswerVoteIncreased(command.Answer);
				Emit(evnt);
			}
			else
			{
				var errors = new List<string>();
				errors.AddRange(IsNotNewSpec.WhyIsNotSatisfiedBy(IsNew));
				errors.AddRange(containsAnswerSpec.WhyIsNotSatisfiedBy(command.Answer));
				Sender.Tell(CommandResult.FailWith(command, errors));
			}
			return true;
		}

		public bool Execute(DecreaseAnswerVote command)
		{
			var containsAnswerSpec = new ContainsAnswerSpecification(State);
			var hasVotesSpec = new AnswerHasVotesSpecification(State);
			if (IsNotNewSpec.IsSatisfiedBy(IsNew) && containsAnswerSpec.IsSatisfiedBy(command.Answer) && hasVotesSpec.IsSatisfiedBy(command.Answer))
			{
				logging.Info($"Increasing Answer Vote");
				var evnt = new AnswerVoteDecreased(command.Answer);
				Emit(evnt);
			}
			else
			{
				var errors = new List<string>();
				errors.AddRange(IsNotNewSpec.WhyIsNotSatisfiedBy(IsNew));
				errors.AddRange(containsAnswerSpec.WhyIsNotSatisfiedBy(command.Answer));
				errors.AddRange(hasVotesSpec.WhyIsNotSatisfiedBy(command.Answer));
				Sender.Tell(CommandResult.FailWith(command, errors));
			}
			return true;
		}
	}
}
