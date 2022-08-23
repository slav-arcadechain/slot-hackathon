using MoralisUnity.Kits.AuthenticationKit;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Dependencies")] [SerializeField]
    private AuthenticationKit authenticationKit = null;

    [SerializeField] private BlockChain blockChain = null;
    [SerializeField] private User user = null;

    private GameObject approvePanel;
    private bool shouldUpdateWallet = false;


    void Start()
    {
        approvePanel = GameObject.Find("ApprovePanel");
        approvePanel.SetActive(false);

        authenticationKit.OnStateChanged.AddListener(AuthOnStateChangedListener);
        user.OnWalletTokenBalanceUpdated += UpdateWalletTokens;
        user.OnTokenApprovalUpdated += UpdateTokenApproval;
    }

    private void UpdateTokenApproval(decimal approvedamount)
    {
        Debug.Log("approved: " + approvedamount);
    }

    private void UpdateWalletTokens(decimal walletbalance)
    {
        Debug.Log("wallet balance: " + walletbalance);
    }

    private async void FixedUpdate()
    {
        if (shouldUpdateWallet)
        {
            shouldUpdateWallet = false;
            await blockChain.HandleWallet2();
        }
    }

    private void AuthOnStateChangedListener(AuthenticationKitState state)
    {
        switch (state)
        {
            case AuthenticationKitState.Disconnected:
                Debug.Log("disconnected");
                approvePanel.SetActive(false);
                break;

            case AuthenticationKitState.MoralisLoggedIn:
                Debug.Log("connected");
                approvePanel.SetActive(true);
                shouldUpdateWallet = true;
                GameObject.Find("BackgroundImage").SetActive(false);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}