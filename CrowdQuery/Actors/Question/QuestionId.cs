using Akkatecture.Core;

namespace CrowdQuery.Actors.Question
{
	public class QuestionId : Identity<QuestionId>
	{
		public static Guid Namespace => new Guid("c67a7d3e-0bf1-470f-a2af-6b1a6c18706f");
		public QuestionId(string value) : base(value)
		{
		}
	}
}
