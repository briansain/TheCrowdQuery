using Akka.Actor;
using Akka.Event;
using Akkatecture.Aggregates;
using Akkatecture.Aggregates.CommandResults;
using CrowdQuery.AS.Actors.Prompt.Commands;
using CrowdQuery.AS.Actors.Prompt.Events;
using CrowdQuery.AS.Actors.Prompt.Query;
using CrowdQuery.AS.Actors.Prompt.Specification;

namespace CrowdQuery.AS.Actors.Prompt
{
	public class PromptActor : AggregateRoot<PromptActor, PromptId, PromptState>,
		IExecute<CreatePrompt>,
		IExecute<IncreaseAnswerVote>,
		IExecute<DecreaseAnswerVote>,
		IExecute<QueryPromptState>
	{
		private static IsNewSpecification IsNewSpec => new IsNewSpecification();
		private static IsNotNewSpecification IsNotNewSpec => new IsNotNewSpecification();
		public ILoggingAdapter logging { get; set; }
		public PromptActor(PromptId aggregateId) : base(aggregateId)
		{
			logging = Context.GetLogger();
		}

		public bool Execute(CreatePrompt command)
		{
			if (IsNewSpec.IsSatisfiedBy(IsNew))
			{
				// SANATIZE THE Prompt FOR SQL INJECTION AND SILLY HACKERS
				logging.Info($"Creating new Prompt: {command.Prompt}");
				var evnt = new PromptCreated(command.Prompt, command.Answers);
				Emit(evnt);
				
				Sender.Tell(CommandResult.SucceedWith(command));
				return true;
			}

			Sender.Tell(CommandResult.FailWith(command, IsNewSpec.WhyIsNotSatisfiedBy(IsNew)), Self);
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

		public bool Execute(QueryPromptState command)
		{
			Sender.Tell(State.DeepCopy());
			return true;
		}
	}
}
