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
    private MoralisLiveQueryCallbacks<SlotGameRoundResult> _callbacks;
    private bool init = true;
    private bool _shouldSpin = false;
    private decimal _approvedAmount = 0;


    private void Start()
    {
        _callbacks = new MoralisLiveQueryCallbacks<SlotGameRoundResult>();
        _callbacks.OnUpdateEvent += HandleGameRoundResultCallback;
        _spinButton = GameObject.Find("SpinButton").GetComponent<Button>();
        _spinButton.onClick.AddListener(SpinButtonListener);
        _musicButton = GameObject.Find("MusicToggle").GetComponent<Button>();
        _musicButton.onClick.AddListener(ToggleMusic);
        
        user = FindObjectOfType<User>();
        user.OnTokenApprovalUpdated += UpdateTokenApproval;
        user.OnWinningsUpdated += UpdateWinnings;
    }

    private  void ToggleMusic()
    {
        var isMuted = GameObject.Find("BackgroundMusic").GetComponent<AudioSource>().mute;
        GameObject.Find("BackgroundMusic").GetComponent<AudioSource>().mute = !isMuted;
        var colors = _musicButton.colors;
        if (!isMuted)
        {

            colors.normalColor = new Color(255, 255, 255, 255);
        }
        else
        {
            colors.normalColor = new Color(255, 255, 255, 130);
 
        }
    }

    private void UpdateWinnings(decimal winningsAmount)
    {
        GameObject.Find("WonText").GetComponent<TextMeshProUGUI>().text = $"{winningsAmount}";
    }

    private void UpdateTokenApproval(decimal approvedAmount)
    {
        GameObject.Find("SpinsText").GetComponent<TextMeshProUGUI>().text = $"{approvedAmount}";
   
        _approvedAmount = approvedAmount;
    }


    private void Awake()
    {
        if (_ins == null)
            _ins = this;
    }

    private void HandleGameRoundResultCallback(SlotGameRoundResult item, int requestid)
    {
        Debug.Log("current roundId: " + item.roundId);
        if (item.roundId == _roundId)
        {
            Debug.Log("in loop: " + item.bracket);
            _roundPaidFor = true;
            _shouldSpin = true;
            _gameWon = item.bracket != 100;
            _gameResult = item.bracket + 1;
            Debug.Log("item.gameResult = " + item.bracket);
        }
    }

    private async void SpinButtonListener()
    {
        if (_approvedAmount < 10)
        {
            GameController.HideGame();
            GameController.ShowApprovalPopup();
        }
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
        MoralisQuery<SlotGameRoundResult> q = await Moralis.GetClient().Query<SlotGameRoundResult>();
        q.WhereEqualTo("user", (await Moralis.GetUserAsync()).accounts[0])
            .WhereEqualTo("roundId", _roundId)
            .WhereEqualTo("confirmed", false);
        MoralisLiveQueryController.AddSubscription("SlotGameRoundResult", q, _callbacks);
        Debug.Log("subscirbed");
    }

    public IEnumerator SpinSlots()
    {
        if (_roundPaidFor)
        {
            if (rows[0].ColumnStopped && rows[1].ColumnStopped && rows[2].ColumnStopped)
            {
                _gameStarted = true;
                StartCoroutine(StartSpinForRow(0));
                yield return null;
                StartCoroutine(StartSpinForRow(1));
                yield return null;
                StartCoroutine(StartSpinForRow(2));
                yield return null;
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
        if (init)
        {
            init = false;
            // _spinButton = GameObject.Find("SpinButton")?.GetComponent<Button>();
            // _spinButton?.onClick.AddListener(SpinButtonListener);
        }

        if (_shouldSpin)
        {
            _shouldSpin = false;
            if (_gameWon)
            {
                StartCoroutine(blockChain.HandleWallet());
                _nextSlotSelected = true;
                StartCoroutine(SelectReward());
            }
            else
            {
                _nextSlotSelected = false;
                StartCoroutine(SpinSlots());
            }
        }


        if (_gameStarted)
        {
            if (rows[0].ColumnStopped && rows[1].ColumnStopped && rows[2].ColumnStopped)
            {
                _gameStarted = false;

                if (rows[0].currentSlot == rows[1].currentSlot &&
                    rows[0].currentSlot == rows[2].currentSlot)
                {
                    _spinButton.interactable = true;
                }
                else
                {
                    ActivateSpinButton();
                }
            }
        }
    }

    private void ActivateSpinButton()
    {
        _spinButton.interactable = true;
    }
}