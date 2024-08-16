using Akkatecture.Aggregates;
using Akkatecture.Commands;

namespace CrowdQuery.Actors.Question
{
	public class QuestionManager : AggregateManager<QuestionActor, QuestionId, Command<QuestionActor, QuestionId>>
	{
	}
}
