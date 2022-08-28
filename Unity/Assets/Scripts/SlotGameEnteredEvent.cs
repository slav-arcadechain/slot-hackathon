using MoralisUnity.Platform.Objects;

namespace SlotMachine
{
    public class SlotGameEnteredEvent : MoralisObject
    {
        public string address { get; set; }
        public string gameFee { get; set; }
        public string roundId { get; set; }
        public bool confirmed { get; set; }
        public bool gameWon { get; set; }
        public int gameResult { get; set; }

        public SlotGameEnteredEvent() : base("SlotGameEnteredEvent")
        {
            
        } 
    }
}