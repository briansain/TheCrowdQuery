namespace CrowdQuery.AS.Projections.PromptProjection
{
    public class Configuration
    {
        public int DebouceTimerMilliseconds { get; set; }
        public Configuration(int debounceTimerSeconds = 500)
        {
            DebouceTimerMilliseconds = debounceTimerSeconds;
        }
    }
}