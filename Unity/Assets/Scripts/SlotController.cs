using System;
using System.Collections;
using System.Numerics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Kits.AuthenticationKit;
using MoralisUnity.Platform.Queries;
using MoralisUnity.Web3Api.Models;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine
{
    public class SlotController : MonoBehaviour
    {
        private GameObject _approvalPanel;
        private MoralisQuery<SlotGameEntered> _getEventsQuery;
        private MoralisLiveQueryCallbacks<SlotGameEntered> _queryCallbacks;

        [Header("Dependencies")] [SerializeField]
        private AuthenticationKit authenticationKit = null;

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

        [Header("UI Elements")] [Space] private GameObject _slotPanel;

        private string _roundId;

        private int _nextSlotIndex;

        // private bool rewardSelected;
        private bool _nextSlotSelected;
        private bool _gameStarted;
        private bool _roundPaidFor;
        private bool _gameWon;
        private bool _hidePanel = true;
        private int _gameResult;
        private Button _spinButton;
        private MoralisLiveQueryCallbacks<SlotGameEntered> _callbacks;

        private void Start()
        {
            authenticationKit = FindObjectOfType<AuthenticationKit>();
            authenticationKit.OnStateChanged.AddListener(AuthOnStateChangedListener);
            _callbacks = new MoralisLiveQueryCallbacks<SlotGameEntered>();
            _callbacks.OnUpdateEvent += HandleGameEnteredCallback;
            _spinButton = GameObject.Find("SpinButton").GetComponent<Button>();
            _spinButton.onClick.AddListener(SpinButtonListener);
            _slotPanel = GameObject.Find("SlotPanel");
        }

        private async void AuthOnStateChangedListener(AuthenticationKitState state)
        {
            switch (state)
            {
                case AuthenticationKitState.Disconnected:
                    Debug.Log("disconnected");
                    break;

                case AuthenticationKitState.MoralisLoggedIn:
                    Debug.Log("connected");
                    await SubscribeToDatabaseEvents();
                    break;
            }
        }

        private void HandleGameEnteredCallback(SlotGameEntered item, int requestid)
        {
            Debug.Log("current roundId: " + item.roundId);
            if (item.roundId == _roundId)
            {
                _roundPaidFor = true;
                _gameWon = item.gameWon;
                _gameResult = item.gameResult + 1;
                Debug.Log("item.gameResult = " +item.gameResult);
            }
        }

        private IEnumerator PlayRound()
        {
            Debug.Log("before game won");
            if (_gameWon)
            {
                Debug.Log("inside game won");
                _nextSlotSelected = true;
                StartCoroutine(SelectReward());
            }
            else
            {
                _nextSlotSelected = false;
                StartCoroutine(SpinSlots());
                    
            }

            yield return null;
        }
        private void FixedUpdate()
        {
            if (_hidePanel)
            {
                _hidePanel = false;
                _slotPanel.SetActive(false);
            }
        }

        private void SpinButtonListener()
        {
            Debug.Log("in listnere");
            CheckResults();
        }

        public int NextSlotIndex
        {
            get { return _nextSlotIndex; }
        }

        public bool NextSlotSelected
        {
            get { return _nextSlotSelected; }
        }

        private void Awake()
        {
            if (_ins == null)
                _ins = this;
        }

        public async void CheckResults()
        {
            _spinButton.interactable = false;
            await PayForGame();
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
            await BlockChain.EnterGameOnBlockchain(gameId);
            _roundId = gameId.ToString();
        }

        private async UniTask SubscribeToDatabaseEvents()
        {
            MoralisQuery<SlotGameEntered> q = await Moralis.GetClient().Query<SlotGameEntered>();
            // q.WhereEqualTo("user", (await Moralis.GetUserAsync()).accounts[0]);
            MoralisLiveQueryController.AddSubscription("SlotGameEnteredEvent", q, _callbacks);
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
}