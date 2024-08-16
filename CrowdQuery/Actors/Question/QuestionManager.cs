using Akkatecture.Aggregates;
using Akkatecture.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdQuery.Actors.Question
{
	public class QuestionManager : AggregateManager<QuestionActor, QuestionId, Command<QuestionActor, QuestionId>>
	{
	}
}
