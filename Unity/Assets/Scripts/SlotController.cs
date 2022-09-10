using System;
using System.Collections;
using System.Numerics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Queries;
using SlotMachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotController : MonoBehaviour
{
    [SerializeField] private BlockChain blockChain = null;
    [SerializeField] private User user = null;
    private static SlotController _ins;

    public static SlotController ins
    {
        get { return _ins; }
    }

    public enum RewardEnum
    {
        None,
        Coin,
        Diamond
    };

    [Serializable]
    public class MultiDimensionalArray
    {
        public RewardEnum rewardCategory;
        public int rewardValue;
        public int rewardChance;
        public Sprite slotIcon;
    }

    [Header("Rewards Custom Settings")] [Space]
    public MultiDimensionalArray[] SlotTypes;

    [Header("Game Inputs")] [Space] public Column[] rows;

    private string _roundId;
    private int _nextSlotIndex;
    private bool _nextSlotSelected;
    private bool _gameStarted;
    private bool _roundPaidFor;
    private bool _gameWon;
    private int _gameResult;
    private Button _spinButton;
    private Button _musicButton;
    private Button _claimButton;
    private MoralisLiveQueryCallbacks<SlotGameRoundResult> _callbacks;
    private MoralisLiveQueryCallbacks<SlotClaimed> _slotClaimedCallbacks;
    private bool _shouldSpin = false;
    private decimal _approvedAmount = 0;
    private bool _shouldUpdateWinnings = false;
    private bool _subscribed;
    private GameController _gameController;
    private GameObject _confetti;
    private bool _subscribedToClaim;
    private bool _shouldHandleClaim;

    private void Start()
    {
        _callbacks = new MoralisLiveQueryCallbacks<SlotGameRoundResult>();
        _callbacks.OnUpdateEvent += HandleGameRoundResultCallback;
        _slotClaimedCallbacks = new MoralisLiveQueryCallbacks<SlotClaimed>();
        _slotClaimedCallbacks.OnUpdateEvent += HandleClaim;
        _spinButton = GameObject.Find("SpinButton").GetComponent<Button>();
        _spinButton.onClick.AddListener(SpinButtonListener);
        _musicButton = GameObject.Find("MusicToggle").GetComponent<Button>();
        _musicButton.onClick.AddListener(ToggleMusic);
        _claimButton = GameObject.Find("ClaimButton").GetComponent<Button>();
        _claimButton.onClick.AddListener(ClaimButtonListener);
        _gameController = FindObjectOfType<GameController>();
        _confetti = GameObject.Find("Confetti01");
        user = FindObjectOfType<User>();
        user.OnTokenApprovalUpdated += UpdateTokenApproval;
        user.OnWinningsUpdated += UpdateWinnings;
    }

    private async void HandleClaim(SlotClaimed item, int requestId)
    {
        _shouldHandleClaim = true;
    }

    private async void ClaimButtonListener()
    {
        if (!_subscribedToClaim)
        {
            _subscribedToClaim = true;
            await SubscribeToClaimEvents();
        }

        GameController.ins.ShowLoader();
        _spinButton.interactable = false;
        await BlockChain.Claim();
    }

    private void ToggleMusic()
    {
        var isMuted = GameObject.Find("BackgroundMusic").GetComponent<AudioSource>().mute;
        GameObject.Find("BackgroundMusic").GetComponent<AudioSource>().mute = !isMuted;
        var colors = GameObject.Find("MusicToggle").GetComponent<Image>().color = isMuted ? Color.white : Color.grey;
    }

    private void UpdateWinnings(decimal winningsAmount)
    {
        StartCoroutine(UpdateWithDelay(winningsAmount));
    }

    private IEnumerator UpdateWithDelay(decimal winningsAmount)
    {
        yield return WaitUntil(() => _shouldUpdateWinnings);
        _shouldUpdateWinnings = false;
        for (int a = 0; a < 80; a++)
        {
            yield return new WaitForSeconds(0.1f);
        }

        GameObject.Find("WonText").GetComponent<TextMeshProUGUI>().text = $"{winningsAmount}";

        _claimButton.interactable = winningsAmount > 1;
    }

    private void UpdateTokenApproval(decimal approvedAmount)
    {
        GameObject.Find("SpinsText").GetComponent<TextMeshProUGUI>().text = $"{approvedAmount / 10}";

        _approvedAmount = approvedAmount;

        if (approvedAmount < 10)
        {
            _spinButton.interactable = false;
            StartCoroutine(WaitForSecondsAndClose(10));
        }
        else if (approvedAmount >= 10 && ColumnsStopped())
        {
            _spinButton.interactable = true;
        }
    }

    private void Awake()
    {
        if (_ins == null)
            _ins = this;
    }

    private void HandleGameRoundResultCallback(SlotGameRoundResult item, int requestid)
    {
        Debug.Log("item.roundId = " + item.roundId + " _roundId: " + _roundId);
        if (item.roundId == _roundId)
        {
            _roundPaidFor = true;
            _shouldSpin = true;
            _gameWon = item.bracket != 100;
            _gameResult = item.bracket + 1;
            Debug.Log("item.gameResult = " + item.bracket);
        }
    }

    private async void SpinButtonListener()
    {
        _confetti.SetActive(false);

        if (_approvedAmount < 10)
        {
            _gameController.ShowApprovalPopup();
            return;
        }

        GameController.ins.ShowLoader();
        _spinButton.interactable = false;
        await PayForGame();
    }

    public int NextSlotIndex
    {
        get { return _nextSlotIndex; }
    }

    public bool NextSlotSelected
    {
        get { return _nextSlotSelected; }
    }

    public IEnumerator SelectReward()
    {
        _nextSlotIndex = _gameResult;
        StartCoroutine(SpinSlots());
        yield return null;
    }

    private async UniTask PayForGame()
    {
        var g = Guid.NewGuid();
        var gameId = BigInteger.Abs(new BigInteger(g.ToByteArray()));
        Debug.Log("play for game: " + gameId);
        _roundId = gameId.ToString();
        await SubscribeToDatabaseEvents();
        await BlockChain.EnterGameOnBlockchain(gameId);
    }

    private async UniTask SubscribeToDatabaseEvents()
    {
        if (!_subscribed)
        {
            _subscribed = true;
            Debug.Log("SubscribeToGameResultEvents");
            MoralisQuery<SlotGameRoundResult> q = await Moralis.GetClient().Query<SlotGameRoundResult>();
            q.WhereEqualTo("user", (await Moralis.GetUserAsync()).accounts[0])
                // .WhereEqualTo("roundId", _roundId)
                .WhereEqualTo("confirmed", false);
            MoralisLiveQueryController.AddSubscription("SlotGameRoundResult", q, _callbacks);
        }
    }

    private async UniTask SubscribeToClaimEvents()
    {
        MoralisQuery<SlotClaimed> q = await Moralis.GetClient().Query<SlotClaimed>();
        q.WhereEqualTo("user", (await Moralis.GetUserAsync()).accounts[0])
            .WhereEqualTo("confirmed", false);
        MoralisLiveQueryController.AddSubscription("SlotClaimed", q, _slotClaimedCallbacks);
        Debug.Log("subscirbed");
    }

    public IEnumerator SpinSlots()
    {
        if (_roundPaidFor)
        {
            GameController.ins.HideLoader();
            StartCoroutine(blockChain.HandleAllowance());

            if (rows[0].ColumnStopped && rows[1].ColumnStopped && rows[2].ColumnStopped)
            {
                _gameStarted = true;
                StartCoroutine(StartSpinForRow(0));
                yield return null;
                StartCoroutine(StartSpinForRow(1));
                yield return null;
                StartCoroutine(StartSpinForRow(2));
                yield return null;
                Debug.Log("should update winnings");
                _shouldUpdateWinnings = true;
            }
        }

        _roundPaidFor = false;
    }

    private IEnumerator StartSpinForRow(int index)
    {
        rows[index].GetComponent<Column>().StartRotating();
        yield return null;
    }

    public static async Task WaitUntil(Func<bool> condition, int frequency = 100, int timeout = -1)
    {
        var waitTask = Task.Run(async () =>
        {
            while (!condition())
            {
                await Task.Delay(frequency);
            }
        });

        if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
            throw new TimeoutException();
    }

    private void Update()
    {
        if (_shouldSpin)
        {
            _shouldSpin = false;
            StartCoroutine(blockChain.HandleWallet(2));
            if (_gameWon)
            {
                _nextSlotSelected = true;
                StartCoroutine(SelectReward());
            }
            else
            {
                _nextSlotSelected = false;
                StartCoroutine(SpinSlots());
            }

            StartCoroutine(blockChain.HandleWallet(10));
        }

        if (_gameStarted)
        {
            if (ColumnsStopped())
            {
                _gameStarted = false;
                _spinButton.interactable = true;
                if (_gameWon)
                {
                    _confetti.SetActive(true);
                }
            }
        }

        if (_shouldHandleClaim)
        {
            _shouldHandleClaim = false;
            StartCoroutine(blockChain.HandleWinnings(0));
            _spinButton.interactable = true;
            StartCoroutine(WaitForWinningsCleared());
        }
    }

    private IEnumerator WaitForWinningsCleared()
    {
        yield return new WaitUntil(() => user.winningsBalance == 0);
        GameController.ins.HideLoader();
    }

    public static GameObject FindConfetti(GameObject parent, string name)
    {
        GameObject[] trs = parent.GetComponentsInChildren<GameObject>(true);
        foreach (GameObject t in trs)
        {
            if (t.name == name)
            {
                return t.gameObject;
            }
        }

        return null;
    }

    private bool ColumnsStopped()
    {
        return rows[0].ColumnStopped && rows[1].ColumnStopped && rows[2].ColumnStopped;
    }

    private IEnumerator WaitForSecondsAndClose(int seconds)
    {
        for (int a = 0; a < seconds * 10; a++)
        {
            yield return new WaitForSeconds(0.1f);
        }

        _gameController.ShowApprovalPopup();
    }
}