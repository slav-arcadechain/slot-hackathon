using Cysharp.Threading.Tasks;
using DefaultNamespace;
using MoralisUnity;
using MoralisUnity.Kits.AuthenticationKit;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Dependencies")] [SerializeField]
    private AuthenticationKit authenticationKit = null;

    [SerializeField] private BlockChain blockChain = null;
    [SerializeField] private User user = null;
    [SerializeField] private Slot slot = null;

    private bool shouldUpdateWallet = false;
    private bool _shouldTransitionView = false;
    private bool hidePanel = true;
    private GameObject approvePanel;
    private GameObject mainBackground;
    private Slider slider;
    private Button approveButton;
    private Button closeApporveButton;
    private Button claimButton;

    void Start()
    {
        approvePanel = GameObject.Find("ApprovePanel");
        slider = GameObject.Find("ApproveSlider").GetComponent<Slider>();
        Debug.Log("slider = " + slider);
        slider.onValueChanged.AddListener(delegate { HandleSlider(); });
        approveButton = GameObject.Find("ApproveButton").GetComponent<Button>();
        closeApporveButton = GameObject.Find("CloseApproveButton").GetComponent<Button>();
        claimButton = GameObject.Find("ClaimButton").GetComponent<Button>();
        mainBackground = GameObject.Find("MainBackground");

        authenticationKit = FindObjectOfType<AuthenticationKit>(); 
        authenticationKit.OnStateChanged.AddListener(AuthOnStateChangedListener);
        blockChain = FindObjectOfType<BlockChain>();
        approveButton.onClick.AddListener(ApproveButtonHandler);
        closeApporveButton.onClick.AddListener(CloseApproveButtonHandler);
        claimButton.onClick.AddListener(ClaimButtonHandler);
        user = FindObjectOfType<User>();
        user.OnWalletTokenBalanceUpdated += UpdateWalletTokens;
        user.OnTokenApprovalUpdated += UpdateTokenApproval;
        user.OnWinningsUpdated += UpdateWinnings;
        slot = FindObjectOfType<Slot>();
    }

    private void ClaimButtonHandler()
    {
        UniTask.Create(async () =>
        {
            await BlockChain.Claim();
            shouldUpdateWallet = true;
        });    
    }

    private void UpdateWinnings(decimal winnings)
    {
        Debug.Log("Winnings: " + winnings);
        if (GameObject.Find("WonAmount"))
        {
            GameObject.Find("WonAmount").GetComponent<Text>().text = winnings.ToString();
        }    
    }

    private void CloseApproveButtonHandler()
    {
        Debug.Log("in close");
        approvePanel.SetActive(false);
        mainBackground.SetActive(false);
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (go.name == "SlotPanel")
            {
                go.SetActive(true);
            }
        }
    }

    private void ApproveButtonHandler()
    {
        UniTask.Create(async () =>
        {
            await SubscribeToDatabaseEvents();
            await BlockChain.ApproveGameTokenSpent((int)slider.value * 10);
        });
    }

    private void HandleSlider()
    {
        Debug.Log("in slider handle");
        var value = slider.value;
        GameObject.Find("ApproveButtonText").GetComponent<Text>().text = $"Approve {value * 10}";
    }

    private void UpdateTokenApproval(decimal approvedAmount)
    {
        Debug.Log("UpdateTokenApproval: " + approvedAmount);
        if (GameObject.Find("ApprovedAmount") != null)
        {
            GameObject.Find("ApprovedAmount").GetComponent<Text>().text = $"{approvedAmount}";
        }
        if (_shouldTransitionView && approvedAmount >= Slot.GameFee)
        {
            Debug.Log("closing");
            _shouldTransitionView = false;
            CloseApproveButtonHandler();
        } else if (_shouldTransitionView && approvedAmount < Slot.GameFee)
        {


            Debug.Log("not closing");
            _shouldTransitionView = false;
            approvePanel.SetActive(true);
        }
    }

    private void UpdateWalletTokens(decimal balance)
    {
        if (GameObject.Find("WalletBalance"))
        {
            GameObject.Find("WalletBalance").GetComponent<Text>().text = balance.ToString();
        }
    }

    private async void Update()
    {
        if (hidePanel)
        {
            hidePanel = false;
            approvePanel.SetActive(false);
        }

        if (shouldUpdateWallet)
        {
            shouldUpdateWallet = false;
            await blockChain.HandleWallet();
        }
    }

    private async void AuthOnStateChangedListener(AuthenticationKitState state)
    {
        switch (state)
        {
            case AuthenticationKitState.Disconnected:
                Debug.Log("disconnected");
                approvePanel.SetActive(false);
                break;

            case AuthenticationKitState.MoralisLoggedIn:
                Debug.Log("connected");
                GameObject.Find("BackgroundImage")?.SetActive(false);
                shouldUpdateWallet = true;
                _shouldTransitionView = true;
                await SubscribeToDatabaseEvents();
                break;
        }
    }

    private async UniTask SubscribeToDatabaseEvents()
    {
        var callbacks = new MoralisLiveQueryCallbacks<TUSDCoinApprovalCronos>();
        callbacks.OnUpdateEvent += ((item, requestId) =>
        {
            Debug.Log($"db update event received: {item}, id: {requestId}");
            shouldUpdateWallet = true;
        });

        var q = await Moralis.GetClient().Query<TUSDCoinApprovalCronos>();
        q.WhereEqualTo("spender", (await Moralis.GetUserAsync()).accounts[0]);
        MoralisLiveQueryController.AddSubscription<TUSDCoinApprovalCronos>("TUSDCoinApprovalCronos", q, callbacks);
    }
}