using MoralisUnity.Platform.Objects;

public class SlotGameRoundResult : MoralisObject
{
    public string roundId { get; set; }
    public int bracket { get; set; }

    public SlotGameRoundResult() : base("SlotGameRoundResult")
    {
    }
}