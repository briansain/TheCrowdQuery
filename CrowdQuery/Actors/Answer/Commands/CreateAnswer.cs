using Akkatecture.Commands;
using CrowdQuery.Actors.Question;

namespace CrowdQuery.Actors.Answer.Commands
{
	public class CreateAnswer : Command<AnswerActor, AnswerId>
	{
		public QuestionId QuestionId { get; set; }
		public string Answer { get; set; }

		public CreateAnswer(AnswerId aggregateId, QuestionId questionId, string answer): base(aggregateId)
		{
			Answer = answer;
			QuestionId = questionId;
		}
	}
}
