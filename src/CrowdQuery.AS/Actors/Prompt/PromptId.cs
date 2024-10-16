using System.Text;
using Akkatecture.Core;
using Akkatecture.Extensions;

namespace CrowdQuery.AS.Actors.Prompt
{
	public class PromptId : Identity<PromptId>
	{
		public static Guid Namespace => new Guid("c67a7d3e-0bf1-470f-a2af-6b1a6c18706f");
		public PromptId(string value) : base(value)
		{
		}
	}

	public static class PromptIdExtensions
	{
		public static string ToBase64(this PromptId input)
		{
			var encoded = Convert.ToBase64String(input.GetBytes());
			return encoded.Replace('+', '-').Replace('/', '_').TrimEnd('=');
		}

		public static PromptId ToPromptId(this string input)
		{
			var encoded = input.Replace('-', '+').Replace('_', '/');
			switch(encoded.Length % 4)
			{
				case 2: encoded += "=="; break;
				case 3: encoded += "="; break;
			}
			var decoded = Convert.FromBase64String(encoded);
			return PromptId.With(Encoding.UTF8.GetString(decoded));
		}
	}
}
