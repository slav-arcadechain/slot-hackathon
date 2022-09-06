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
        hideApproval();
        hideGame();
    }

    private void hideGame()
    {
        _slotPanel.transform.position = UnityEngine.Vector3.back;
    }

    private void hideApproval()
    {
        _approvalPopup.transform.position = UnityEngine.Vector3.back;
    }

    private void hideMainBackground()
    {
        _mainBackground.transform.position = UnityEngine.Vector3.back;
    }
    private void showMainBackground()
    {
        _mainBackground.transform.position = UnityEngine.Vector3.forward;
    }
    private void showGame()
    {
        _gameBackground.transform.position = UnityEngine.Vector3.forward;
        _slotPanel.transform.position = UnityEngine.Vector3.forward;
        hideMainBackground();

    }

    private void UpdateTokenApproval(decimal approvedAmount)
    {
        Debug.Log("in approval = " + approvedAmount);
        if (_shouldTransitionView && approvedAmount >= Slot.GameFee)
        {
            _shouldTransitionView = false;
            showGame();
        }
        else if (_shouldTransitionView && approvedAmount < Slot.GameFee)
        {
            _shouldTransitionView = false;
            ShowApprovalPopup();
        }
    }



    private void ShowApprovalPopup()
    {
        hideGame();
        _approvalPopup.transform.position = UnityEngine.Vector3.forward;
 
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
            StartCoroutine(WaitForSecond());
        });

        var q = await Moralis.GetClient().Query<TUSDCoinApprovalCronos>();
        q.WhereEqualTo("spender", (await Moralis.GetUserAsync()).accounts[0]);
        MoralisLiveQueryController.AddSubscription<TUSDCoinApprovalCronos>("TUSDCoinApprovalCronos", q, callbacks);
    }

    private IEnumerator WaitForSecond()
    {
        for (int a = 0; a < 20; a++)
        {
            yield return new WaitForSeconds(0.1f);
        }

        _shouldUpdateWallet = true;
    }
}