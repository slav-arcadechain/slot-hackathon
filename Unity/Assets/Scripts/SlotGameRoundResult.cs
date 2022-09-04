using MoralisUnity.Platform.Objects;

namespace SlotMachine
{
    public class SlotGameRoundResult : MoralisObject
    {
        public string roundId { get; set; }
        public int bracket { get; set; }
        public SlotGameRoundResult() : base("SlotGameRoundResult")
        {
            
        } 
    }
}