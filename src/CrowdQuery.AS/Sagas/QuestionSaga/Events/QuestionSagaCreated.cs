using Akkatecture.Aggregates;

namespace CrowdQuery.AS.Sagas.QuestionSaga.Events
{
	public class QuestionSagaCreated : AggregateEvent<QuestionSaga, QuestionSagaId>
	{
		public string Question { get; set; }
		public List<string> Answers { get; set; }

		public QuestionSagaCreated(string question, List<string> answers)
		{
			Question = question;
			Answers = answers;
		}
	}
}
