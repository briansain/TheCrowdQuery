using Akkatecture.Core;

namespace CrowdQuery.Actors.Answer
{
	public class AnswerId : Identity<AnswerId>
	{
		public static Guid Namespace => new Guid("c67a7d3e-0bf1-470f-a2af-6b1a6c18706f");
		public AnswerId(string aggregateId) : base(aggregateId)
		{
		}
	}
}
