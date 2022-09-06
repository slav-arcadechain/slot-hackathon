using System.Collections;
using System.Numerics;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using MoralisUnity;
using MoralisUnity.Kits.AuthenticationKit;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = System.Numerics.Vector3;

public class GameController : MonoBehaviour
{
    
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
    
    [Header("Dependencies")] [SerializeField]
    private AuthenticationKit authenticationKit = null;

    [SerializeField] private BlockChain blockChain = null;
    [SerializeField] private User user = null;
    [SerializeField] private Slot slot = null;

    private bool _shouldUpdateWallet = false;
    private bool _shouldTransitionView = false;
    private GameObject _approvalPopup;
    private GameObject _slotPanel;
    private GameObject _mainBackground;
    private GameObject _gameBackground;

    void Start()
    {
        authenticationKit = FindObjectOfType<AuthenticationKit>();
        authenticationKit.OnStateChanged.AddListener(AuthOnStateChangedListener);
        blockChain = FindObjectOfType<BlockChain>();
        user = FindObjectOfType<User>();
        user.OnTokenApprovalUpdated += UpdateTokenApproval;
        slot = FindObjectOfType<Slot>();
        _approvalPopup = GameObject.Find("ApprovalPopup");
        _slotPanel = GameObject.Find("SlotPanel");
        _mainBackground = GameObject.Find("MainBackground");
        _gameBackground = GameObject.Find("slotBackground");
        HideApproval();
        HideGame();
    }

    public static void HideGame()
    {
        GameObject.Find("SlotPanel").transform.position = UnityEngine.Vector3.back;
    }
    
    public static void ShowGame()
    {
        GameObject.Find("slotBackground").transform.position = UnityEngine.Vector3.forward;
        GameObject.Find("SlotPanel").transform.position = UnityEngine.Vector3.forward;
        GameObject.Find("MainBackground").transform.position = UnityEngine.Vector3.back;
    }
    public static void ShowApprovalPopup()
    {
        HideGame();
        GameObject.Find("ApprovalPopup").transform.position = UnityEngine.Vector3.forward;
 
    }

    public static void HideApproval()
    {
        GameObject.Find("ApprovalPopup").transform.position = UnityEngine.Vector3.back;
    }

    private void hideMainBackground()
    {
        _mainBackground.transform.position = UnityEngine.Vector3.back;
    }
    private void showMainBackground()
    {
        _mainBackground.transform.position = UnityEngine.Vector3.forward;
    }


    private void UpdateTokenApproval(decimal approvedAmount)
    {
        Debug.Log("in approval = " + approvedAmount);
        if (_shouldTransitionView && approvedAmount >= Slot.GameFee)
        {
            _shouldTransitionView = false;
            ShowGame();
        }
        else if (_shouldTransitionView && approvedAmount < Slot.GameFee)
        {
            _shouldTransitionView = false;
            ShowApprovalPopup();
        }
    }





    private void Update()
    {
        if (_shouldUpdateWallet)
        {
            _shouldUpdateWallet = false;
            StartCoroutine(blockChain.HandleWallet());
        }
    }

    private async void AuthOnStateChangedListener(AuthenticationKitState state)
    {
        switch (state)
        {
            case AuthenticationKitState.MoralisLoggedIn:
                Debug.Log("connected");
                GameObject.Find("BackgroundImage")?.SetActive(false);
                GameObject.Find("DisconnectButton")?.SetActive(false);
                _shouldUpdateWallet = true;
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
            _shouldUpdateWallet = true;
        });

        var q = await Moralis.GetClient().Query<TUSDCoinApprovalCronos>();
        q.WhereEqualTo("spender", (await Moralis.GetUserAsync()).accounts[0]);
        MoralisLiveQueryController.AddSubscription<TUSDCoinApprovalCronos>("TUSDCoinApprovalCronos", q, callbacks);
    }
}