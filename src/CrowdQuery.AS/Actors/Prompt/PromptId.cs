using Akkatecture.Core;

namespace CrowdQuery.AS.Actors.Prompt
{
	public class PromptId : Identity<PromptId>
	{
		public static Guid Namespace => new Guid("c67a7d3e-0bf1-470f-a2af-6b1a6c18706f");
		public PromptId(string value) : base(value)
		{
		}
	}
}
