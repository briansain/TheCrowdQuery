using Akkatecture.Aggregates;
using Akkatecture.Commands;

namespace CrowdQuery.Actors.Answer
{
	public class AnswerManager : AggregateManager<AnswerActor, AnswerId, Command<AnswerActor, AnswerId>>
	{
	}
}
