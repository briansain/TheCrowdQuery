using Akkatecture.Aggregates;
using Akkatecture.Aggregates.Snapshot;
using Akkatecture.Sagas;
using CrowdQuery.Sagas.QuestionSaga.Events;

namespace CrowdQuery.Sagas.QuestionSaga
{
	public class QuestionSagaState : SagaState<QuestionSaga, QuestionSagaId, IMessageApplier<QuestionSaga, QuestionSagaId>>,
		IAggregateSnapshot<QuestionSaga, QuestionSagaId>,
		IApply<QuestionSagaCreated>,
		IApply<AnswerVoteIncreased>,
		IApply<AnswerVoteDecreased>,
		IHydrate<QuestionSagaState>
	{
		public string Question { get; private set; } = string.Empty;
		public Dictionary<string, long> Answers { get; private set; } = new Dictionary<string, long>();

		public void Hydrate(QuestionSagaState aggregateSnapshot)
		{
			Question = aggregateSnapshot.Question;
			Answers = new Dictionary<string, long>(aggregateSnapshot.Answers);
		}

		public void Apply(QuestionSagaCreated aggregateEvent)
		{
			Question = aggregateEvent.Question;
			Answers = aggregateEvent.Answers.ToDictionary(x => x, y => (long)0);
		}

		public void Apply(AnswerVoteIncreased aggregateEvent)
		{
			Answers[aggregateEvent.Answer]++;
		}

		public void Apply(AnswerVoteDecreased aggregateEvent)
		{
			Answers[aggregateEvent.Answer]--;
		}
	}
}
