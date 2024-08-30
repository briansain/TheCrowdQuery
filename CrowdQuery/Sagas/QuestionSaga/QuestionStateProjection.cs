namespace CrowdQuery.Sagas.QuestionSaga
{
	public class QuestionStateProjection
	{
		public string Question { get; set; }
		public Dictionary<string, long> AnswerVotes { get; set; }
		public QuestionStateProjection()
		{
			Question = string.Empty;
			AnswerVotes = new Dictionary<string, long>();
		}
	}
}
