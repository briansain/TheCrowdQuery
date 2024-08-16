namespace CrowdQuery.Saga.Commands
{
	public class StartSaga
	{
		public QuestionAnswerSagaId SagaId { get; set; }
		public string Question { get; set; }
		public List<string> Answers { get; set; }
		public StartSaga(QuestionAnswerSagaId aggregateId, string question, List<string> answers)
		{
			SagaId = aggregateId;
			Question = question;
			Answers = answers;
		}
	}
}
