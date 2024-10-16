using Akkatecture.Aggregates;
using CrowdQuery.AS.Actors.Prompt.Events;

namespace CrowdQuery.AS.Actors.Prompt
{
	public class PromptState : AggregateState<PromptActor, PromptId>,
		IApply<PromptCreated>,
		IApply<AnswerVoteIncreased>,
		IApply<AnswerVoteDecreased>
	{
		public string Prompt { get; private set; }
		public Dictionary<string, long> AnswerVotes { get; private set; }

		public PromptState()
		{
			AnswerVotes = new Dictionary<string, long>();
			Prompt = string.Empty;
		}

		public PromptState(string prompt, Dictionary<string, long> answers)
		{
			Prompt = prompt;
			AnswerVotes = answers;
		}

		public void Apply(PromptCreated aggregateEvent)
		{
			Prompt = aggregateEvent.Prompt;
			AnswerVotes = aggregateEvent.Answers.ToDictionary(x => x, y => (long)0);
		}

		public void Apply(AnswerVoteIncreased aggregateEvent)
		{
			AnswerVotes[aggregateEvent.Answer]++;
		}

		public void Apply(AnswerVoteDecreased aggregateEvent)
		{
			AnswerVotes[aggregateEvent.Answer]--;
		}

		public PromptState DeepCopy()
		{
			var newState = new PromptState();
			newState.Prompt = Prompt;
			newState.AnswerVotes = new Dictionary<string, long>(AnswerVotes);
			return newState;
		}
	}
}
