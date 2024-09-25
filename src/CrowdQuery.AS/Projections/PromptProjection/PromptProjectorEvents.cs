
namespace CrowdQuery.AS.Projections.PromptProjection
{
    public interface IProjectorEvent {}
    public class ProjectionCreated : IProjectorEvent
    {
        public string Prompt { get; set; }
        public Dictionary<string, int> Answers { get; set; }
        public ProjectionCreated(string prompt, Dictionary<string, int> answers)
        {
            Prompt = prompt;
            Answers = answers;
        }
    }

    public class ProjectionAnswerIncreased : IProjectorEvent
    {
        public string Answer {get;set;}
        public long PromptSequenceNumber {get;set;}
        public ProjectionAnswerIncreased(string answer, long promptSequenceNumber)
        {
            Answer = answer;
            PromptSequenceNumber = promptSequenceNumber;
        }
    }

    public class ProjectionAnswerDecreased : IProjectorEvent
    {
        public string Answer {get;set;}
        public long PromptSequenceNumber {get;set;}
        public ProjectionAnswerDecreased(string answer, long promptSequenceNumber)
        {
            Answer = answer;
            PromptSequenceNumber = promptSequenceNumber;
        }
    }
}