using Akkatecture.Specifications;

namespace CrowdQuery.AS.Actors.Prompt.Specification
{
	public class AnswerHasVotesSpecification : Specification<string>
	{
		private readonly PromptState _PromptState;
		public AnswerHasVotesSpecification(PromptState PromptState)
		{
			_PromptState = PromptState;
		}

		protected override IEnumerable<string> IsNotSatisfiedBecause(string answer)
		{
			if (_PromptState.AnswerVotes.ContainsKey(answer) && _PromptState.AnswerVotes[answer] == 0)
			{
				yield return $"Answer must have votes before decreasing the count";
			}
		}
	}
}
