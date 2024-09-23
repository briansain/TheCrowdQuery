using Akkatecture.Aggregates;
using Akkatecture.Aggregates.Snapshot;
using Akkatecture.Sagas;
using CrowdQuery.AS.Sagas.PromptSaga.Events;

namespace CrowdQuery.AS.Sagas.PromptSaga
{
	public class PromptSagaState : SagaState<PromptSaga, PromptSagaId, IMessageApplier<PromptSaga, PromptSagaId>>,
		IAggregateSnapshot<PromptSaga, PromptSagaId>,
		IApply<PromptSagaCreated>,
		IApply<AnswerVoteIncreased>,
		IApply<AnswerVoteDecreased>,
		IHydrate<PromptSagaState>
	{
		public string Prompt { get; private set; } = string.Empty;
		public Dictionary<string, long> Answers { get; private set; } = new Dictionary<string, long>();

		public void Hydrate(PromptSagaState aggregateSnapshot)
		{
			Prompt = aggregateSnapshot.Prompt;
			Answers = new Dictionary<string, long>(aggregateSnapshot.Answers);
		}

		public void Apply(PromptSagaCreated aggregateEvent)
		{
			Prompt = aggregateEvent.Prompt;
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
