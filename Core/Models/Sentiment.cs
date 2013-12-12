namespace HappiestProgrammer.Core.Models
{
    public class Sentiment
    {
        private const float NuetralThreshold = 0.1F;

        public float Score { get; private set; }

        public string Label
        {
            get
            {
                if (this.Score > NuetralThreshold)
                {
                    return "Positive";
                }

                if (this.Score < -NuetralThreshold)
                {
                    return "Negative";
                }

                return "Neutral";
            }
        }

        public Sentiment(float score)
        {
            this.Score = score;
        }
    }
}
