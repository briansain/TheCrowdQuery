using Akkatecture.Commands;

namespace CrowdQuery.Akka.Actors.Question.Query
{
	public class QueryQuestionState : Command<QuestionActor, QuestionId>
	{
		public QueryQuestionState(QuestionId aggregateId) : base(aggregateId)
		{
		}
	}
}
