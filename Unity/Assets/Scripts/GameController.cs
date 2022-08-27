using System;
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

    [Serializable]
    public class MultiDimensionalArray
    {
        public RewardEnum
            rewardCategory; //You should select reward category at inspector that determines the given reward when this slot selected

        public int rewardValue; //All 3 rows same reward
        public int rewardChance; //Chance to give this slot as result reward
        public Sprite slotIcon; //This is aoutomaticaly using as this rows icon
    }

    [Header("Rewards Custom Settings")] [Space]
    public MultiDimensionalArray[] SlotTypes;

    [Serializable]
    public class RewardTable
    {
        public Image[] rewardImages;
        public Text rewardText;
        public Image rewardCurrencyIcon;
    }

    public enum RewardEnum
    {
        None,
        Coin,
        Diamond
    };


    private bool shouldUpdateWallet = false;
    private bool hidePanel = true;
    private GameObject approvePanel;
    private GameObject slotMachine;
    private Slider slider;
    private Button approveButton;
    private Button closeApporveButton;
    private bool nextSlotSelected;

    public bool NextSlotSelected
    {
        get { return nextSlotSelected; }
    }

    private int nextSlotIndex;

    public int NextSlotIndex
    {
        get { return nextSlotIndex; }
    }

    private static GameController _ins;

    public static GameController ins
    {
        get { return _ins; }
    }

    private void Awake()
    {
        if (_ins == null)
            _ins = this;
    }

    void Start()
    {
        approvePanel = GameObject.Find("ApprovePanel");
        slotMachine = GameObject.Find("SlotMachine");
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
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (go.name == "SlotMachine")
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