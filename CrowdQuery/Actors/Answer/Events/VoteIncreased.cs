using Akkatecture.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdQuery.Actors.Answer.Events
{
	public class VoteIncreased: AggregateEvent<AnswerActor, AnswerId>
	{
	}
}
