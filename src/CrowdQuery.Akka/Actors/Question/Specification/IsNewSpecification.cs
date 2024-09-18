using Akkatecture.Specifications;

namespace CrowdQuery.Akka.Actors.Question.Specification
{
	public class IsNewSpecification : Specification<bool>
	{
		protected override IEnumerable<string> IsNotSatisfiedBecause(bool aggregate)
		{
			if (!aggregate)
			{
				yield return "Aggregate is not new";
			}
		}
	}
}
