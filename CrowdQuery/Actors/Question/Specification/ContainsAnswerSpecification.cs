using Akkatecture.Specifications;

namespace CrowdQuery.Actors.Question.Specification
{
	internal class ContainsAnswerSpecification : Specification<string>
	{
		private readonly QuestionState _questionState;
		public ContainsAnswerSpecification(QuestionState questionState)
		{
			_questionState = questionState;
		}
		protected override IEnumerable<string> IsNotSatisfiedBecause(string answer)
		{
			if (!_questionState.AnswerVotes.ContainsKey(answer))
			{
				yield return $"Answers does not contain {answer}";
			}
		}
	}
}
