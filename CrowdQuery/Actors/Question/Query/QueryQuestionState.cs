﻿using Akkatecture.Commands;

namespace CrowdQuery.Actors.Question.Query
{
	public class QueryQuestionState : Command<QuestionActor, QuestionId>
	{
		public QueryQuestionState(QuestionId aggregateId) : base(aggregateId)
		{
		}
	}
}
