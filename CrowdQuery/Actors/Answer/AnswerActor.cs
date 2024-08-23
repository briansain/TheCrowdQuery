using Akkatecture.Aggregates;
using Akkatecture.Aggregates.CommandResults;
using Akkatecture.Aggregates.ExecutionResults;
using CrowdQuery.Actors.Answer.Commands;
using CrowdQuery.Actors.Answer.Events;
using CrowdQuery.Actors.Specification;

namespace CrowdQuery.Actors.Answer
{
	public class AnswerActor : AggregateRoot<AnswerActor, AnswerId, AnswerState>,
		IExecute<CreateAnswer>,
		IExecute<IncreaseVote>,
		IExecute<DecreaseVote>
	{
		private readonly IsNewSpecification IsNewSpec = new IsNewSpecification();
		private readonly IsNotNewSpecification IsNotNewSpec = new IsNotNewSpecification();

		private static FailedExecutionResult FailedExecutionResult = new FailedExecutionResult(["Aggregate is new"]);
		public AnswerActor(AnswerId aggregateId) : base(aggregateId)
		{
		}

		public bool Execute(CreateAnswer command)
		{
			if (IsNotNewSpec.IsSatisfiedBy(IsNew))
			{
				var evnt = new AnswerCreated(command.QuestionId, command.Answer);
				Emit(evnt);
			}
			else
			{
				Sender.Tell(CommandResult.FailWith(command, IsNotNewSpec.WhyIsNotSatisfiedBy(IsNew)), Self);
			}

			return true;
		}

		public bool Execute(IncreaseVote command)
		{
			if (!IsNewSpec.IsSatisfiedBy(IsNew))
			{
				Sender.Tell(CommandResult.FailWith(command, IsNewSpec.WhyIsNotSatisfiedBy(IsNew)), Self);
				return true;
			}
			var evnt = new VoteIncreased();
			Emit(evnt);
			return true;
		}

		public bool Execute(DecreaseVote command)
		{
			if (!IsNewSpec.IsSatisfiedBy(IsNew))
			{
				Sender.Tell(CommandResult.FailWith(command, IsNewSpec.WhyIsNotSatisfiedBy(IsNew)), Self);
				return true;
			}
			var evnt = new VoteDecreased();
			Emit(evnt);
			return true;
		}
	}
}
