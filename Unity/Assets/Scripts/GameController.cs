using System.Numerics;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Kits.AuthenticationKit;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Dependencies")] [SerializeField]
    private AuthenticationKit authenticationKit = null;

    [SerializeField] private BlockChain blockChain = null;
    [SerializeField] private User user = null;

    private GameObject approvePanel;
    private bool shouldUpdateWallet = false;
    private bool hidePanel = true;
    private Slider slider;
    private Button approveButton;



    void Start()
    {
        approvePanel = GameObject.Find("ApprovePanel");
        // walletBalance = GameObject.Find("WalletBalance").GetComponent<Text>();
        slider = GameObject.Find("ApproveSlider").GetComponent<Slider>();
        slider.onValueChanged.AddListener(delegate { HandleSlider(); });
        approveButton = GameObject.Find("ApproveButton").GetComponent<Button>();


        authenticationKit.OnStateChanged.AddListener(AuthOnStateChangedListener);
        approveButton.onClick.AddListener(ApproveButtonHandler);
        user.OnWalletTokenBalanceUpdated += UpdateWalletTokens;
        user.OnTokenApprovalUpdated += UpdateTokenApproval;
    }

    private void ApproveButtonHandler()
    {
        Debug.Log("apporoving");
        UniTask.Create(async () =>
        {
            await BlockChain.ApproveGameTokenSpent((int)slider.value * 10);
            // await SubscribeToDatabaseEvents();
        });
    }


    private void HandleSlider()
    {
        var value = slider.value;
        GameObject.Find("ApproveButtonText").GetComponent<Text>().text = $"Approve {value * 10}";
    }

    private void UpdateTokenApproval(decimal approvedamount)
    {
        Debug.Log("approved: " + approvedamount);
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

    // Update is called once per frame
    void Update()
    {
    }
}