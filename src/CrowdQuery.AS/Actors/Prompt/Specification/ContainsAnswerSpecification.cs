using Akkatecture.Specifications;

namespace CrowdQuery.AS.Actors.Prompt.Specification
{
	internal class ContainsAnswerSpecification : Specification<string>
	{
		private readonly PromptState _PromptState;
		public ContainsAnswerSpecification(PromptState PromptState)
		{
			_PromptState = PromptState;
		}
		protected override IEnumerable<string> IsNotSatisfiedBecause(string answer)
		{
			if (!_PromptState.AnswerVotes.ContainsKey(answer))
			{
				yield return $"Answers does not contain {answer}";
			}
		}
	}
}
