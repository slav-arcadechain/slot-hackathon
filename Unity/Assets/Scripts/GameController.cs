using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using MoralisUnity;
using MoralisUnity.Kits.AuthenticationKit;
using UnityEngine;
using UnityEngine.UI;

public delegate void ViewTransitionedHandler(string viewName);
public class GameController : MonoBehaviour
{
    public event ViewTransitionedHandler OnViewTransitioned;
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
    [SerializeField] private Image _backgroundImage = null;

    private bool _shouldUpdateWallet;
    private bool _shouldTransitionView;
    private bool _subscribedToApproval;
    private readonly int[] _allowedChainIds = { 338 };

    void Start()
    {
        authenticationKit = FindObjectOfType<AuthenticationKit>();
        authenticationKit.OnStateChanged.AddListener(AuthOnStateChangedListener);
        blockChain = FindObjectOfType<BlockChain>();
        user = FindObjectOfType<User>();
        user.OnTokenApprovalUpdated += UpdateTokenApproval;
        slot = FindObjectOfType<Slot>();
        HideApproval();
        HideGame();
    }

    public static void HideGame()
    {
        GameObject.Find("SlotPanel").transform.position = Vector3.back;
    }

    public void ShowGame()
    {
        HideApproval();
        GameObject.Find("slotBackground").transform.position = Vector3.forward;
        GameObject.Find("SlotPanel").transform.position = Vector3.forward;
        HideMainBackground();
        OnViewTransitioned?.Invoke("slot");
    }

    private static void HideMainBackground()
    {
        GameObject.Find("MainBackground").transform.position = Vector3.back;
    }

    public void ShowApprovalPopup()
    {
        ShowMainBackground();
        HideGame();
        GameObject.Find("ApprovalPopup").transform.position = Vector3.forward;
        OnViewTransitioned?.Invoke("approval");
    }

    public static void HideApproval()
    {
        GameObject.Find("ApprovalPopup").transform.position = Vector3.back;
    }

    private void ShowMainBackground()
    {
        GameObject.Find("MainBackground").transform.position = Vector3.forward;
    }
    
    public void ShowLoader()
    {
        _backgroundImage.gameObject.SetActive(true);
        var position = GameObject.Find("Loader").transform.position;
        GameObject.Find("Loader").transform.position = new Vector3(1000.0f, position.y, position.z);
    }

    public void HideLoader()
    {
        _backgroundImage.gameObject.SetActive(false);
        var position = GameObject.Find("Loader").transform.position;
        GameObject.Find("Loader").transform.position = new Vector3(10000.0f, position.y, position.z);
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
            StartCoroutine(blockChain.HandleWallet(0));
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
                CheckBlockChain();
                _shouldUpdateWallet = true;
                _shouldTransitionView = true;
                await SubscribeToDatabaseEvents();
                break;
        }
    }

    private void CheckBlockChain()
    {
        if (Moralis.CurrentChain == null || !_allowedChainIds.Contains(Moralis.CurrentChain.ChainId))
        {
            authenticationKit.Disconnect();
        }
    }

    private async UniTask SubscribeToDatabaseEvents()
    {
        if (!_subscribedToApproval)
        {
            _subscribedToApproval = true;
            var callbacks = new MoralisLiveQueryCallbacks<TUSDCoinApprovalCronos>();
            callbacks.OnUpdateEvent += ((item, requestId) =>
            {
                Debug.Log($"db update event received: {item}, id: {requestId}");
                // _shouldUpdateWallet = true;
                StartCoroutine(WaitForSecondsAndUpdateWallet(20));
            });

            var q = await Moralis.GetClient().Query<TUSDCoinApprovalCronos>();
            q.WhereEqualTo("spender", (await Moralis.GetUserAsync()).accounts[0]);
            MoralisLiveQueryController.AddSubscription<TUSDCoinApprovalCronos>("TUSDCoinApprovalCronos", q, callbacks);
        }
    }

    private IEnumerator WaitForSecondsAndUpdateWallet(int seconds)
    {
        Debug.Log("before");
        for (int a = 0; a < seconds * 10; a++)
        {
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("after");
        _shouldUpdateWallet = true;
    }
}