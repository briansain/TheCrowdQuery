using Akkatecture.Specifications;

namespace CrowdQuery.AS.Actors.Prompt.Specification
{
	public class IsNotNewSpecification : Specification<bool>
	{
		protected override IEnumerable<string> IsNotSatisfiedBecause(bool aggregate)
		{
			if (aggregate)
			{
				yield return "Aggregate is new";
			}
		}
	}
}
