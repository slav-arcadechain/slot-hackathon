using MoralisUnity.Platform.Objects;

public class TUSDCoinApprovalCronos : MoralisObject
{
    public string spender { get; set; }
    public bool confirmed { get; set; }

    public TUSDCoinApprovalCronos() : base("TUSDCoinApprovalCronos")
    {
    }
}