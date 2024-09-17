using Akkatecture.Specifications;

namespace CrowdQuery.Actors.Question.Specification
{
	public class AnswerHasVotesSpecification : Specification<string>
	{
		private readonly QuestionState _questionState;
		public AnswerHasVotesSpecification(QuestionState questionState)
		{
			_questionState = questionState;
		}

		protected override IEnumerable<string> IsNotSatisfiedBecause(string answer)
		{
			if (_questionState.AnswerVotes.ContainsKey(answer) && _questionState.AnswerVotes[answer] == 0)
			{
				yield return $"Answer must have votes before decreasing the count";
			}
		}
	}
}
