namespace ScorePredictor.Models
{
    public class Goal
    {
        public int minute { get; set; }

        public string scorer { get; set; }

        public string assist { get; set; } = "";
    }
}