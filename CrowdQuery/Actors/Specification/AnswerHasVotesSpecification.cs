using Akkatecture.Commands;
using Akkatecture.Specifications;
using CrowdQuery.Actors.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdQuery.Actors.Specification
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
			if (_questionState.Answers.ContainsKey(answer) && _questionState.Answers[answer] == 0)
			{
				yield return $"Answer must have votes before decreasing the count";
			}
		}
	}
}
