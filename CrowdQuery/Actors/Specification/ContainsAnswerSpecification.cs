using Akkatecture.Specifications;
using CrowdQuery.Actors.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdQuery.Actors.Specification
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
			if (!_questionState.Answers.ContainsKey(answer))
			{
				yield return $"Answers does not contain {answer}";
			}
		}
	}
}
