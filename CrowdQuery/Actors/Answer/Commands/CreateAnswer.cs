namespace CrowdQuery.Actors.Answer.Commands
{
	internal class CreateAnswer
	{
		public Guid QuestionId { get; set; }
		public Guid AnswerId { get; set; }
		public string Answer { get; set; }
		public int Votes { get; set; }

		public CreateAnswer()
		{
			Answer = string.Empty;
			QuestionId = Guid.Empty;
			AnswerId = Guid.Empty;
			Votes = 0;
		}

		public CreateAnswer(string answer, Guid answerId, Guid questionId, int votes = 0)
		{
			Answer = answer;
			AnswerId = answerId;
			Answer = answer;
			QuestionId = questionId;
			Votes = votes;
		}
	}
}
