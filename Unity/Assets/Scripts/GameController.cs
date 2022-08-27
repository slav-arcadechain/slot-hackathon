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

    private bool shouldUpdateWallet = false;
    private bool hidePanel = true;
    private GameObject approvePanel;
    private Slider slider;
    private Button approveButton;
    private Button closeApporveButton;


    void Start()
    {
        approvePanel = GameObject.Find("ApprovePanel");
        slider = GameObject.Find("ApproveSlider").GetComponent<Slider>();
        slider.onValueChanged.AddListener(delegate { HandleSlider(); });
        approveButton = GameObject.Find("ApproveButton").GetComponent<Button>();
        closeApporveButton = GameObject.Find("CloseApproveButton").GetComponent<Button>();

        authenticationKit.OnStateChanged.AddListener(AuthOnStateChangedListener);
        approveButton.onClick.AddListener(ApproveButtonHandler);
        closeApporveButton.onClick.AddListener(CloseApproveButtonHandler);
        user.OnWalletTokenBalanceUpdated += UpdateWalletTokens;
        user.OnTokenApprovalUpdated += UpdateTokenApproval;
    }

    private void CloseApproveButtonHandler()
    {
        approvePanel.SetActive(false);
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
        var value = slider.value;
        GameObject.Find("ApproveButtonText").GetComponent<Text>().text = $"Approve {value * 10}";
    }

    private void UpdateTokenApproval(decimal approvedAmount)
    {
        GameObject.Find("ApprovedAmount").GetComponent<Text>().text = $"{approvedAmount}";
    }

    private void UpdateWalletTokens(decimal balance)
    {
        GameObject.Find("WalletBalance").GetComponent<Text>().text = balance.ToString();
    }

    private async void FixedUpdate()
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

    // Update is called once per frame
    void Update()
    {
    }
}