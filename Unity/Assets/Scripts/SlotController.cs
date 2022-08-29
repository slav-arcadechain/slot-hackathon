using System;
using System.Numerics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Platform.Queries;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine
{
    public class SlotController : MonoBehaviour
    {
        private GameObject _approvalPanel;
        private MoralisQuery<SlotGameEntered> _getEventsQuery;
        private MoralisLiveQueryCallbacks<SlotGameEntered> _queryCallbacks;

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

        private void Start()
        {
            _spinButton = GameObject.Find("SpinButton").GetComponent<Button>();
            _spinButton.onClick.AddListener(SpinButtonListener);
            _slotPanel = GameObject.Find("SlotPanel");
        }

        private async void FixedUpdate()
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
            // rewardSelected = false;
            await PayForGame();
            SubscribeToDatabaseEvents();
            await WaitUntil(IsRoundPaidFor);

            if (_gameWon)
            {
                _nextSlotSelected = true;
                SelectReward();
            }
            else
            {
                _nextSlotSelected = false;
                SpinSlots();
            }
        }

        public void SelectReward()
        {
            _nextSlotIndex = _gameResult;
            SpinSlots();
        }

        private async UniTask PayForGame()
        {
            var g = Guid.NewGuid();
            var gameId = BigInteger.Abs(new BigInteger(g.ToByteArray()));
            await BlockChain.EnterGameOnBlockchain(gameId);
            _roundId = gameId.ToString();
        }

        private async void SubscribeToDatabaseEvents()
        {
            MoralisLiveQueryCallbacks<SlotGameEntered> callbacks =
                new MoralisLiveQueryCallbacks<SlotGameEntered>();

            callbacks.OnUpdateEvent += ((item, requestId) =>
            {
                Debug.Log("current roundId: " + item.roundId);
                if (item.roundId == _roundId)
                {
                    _roundPaidFor = true;
                    _gameWon = item.gameWon;
                    _gameResult = item.gameResult + 1;
                }

                Debug.Log(
                    $"Updated event:  gameId: {item.roundId}, gameWon: {item.gameWon}, gameResult: {item.gameResult}");
            });

            MoralisQuery<SlotGameEntered> q = await Moralis.GetClient().Query<SlotGameEntered>();
            MoralisLiveQueryController.AddSubscription<SlotGameEntered>("SlotGameEnteredEvent", q, callbacks);
        }

        public async void SpinSlots()
        {
            if (_roundPaidFor)
            {
                if (rows[0].ColumnStopped && rows[1].ColumnStopped && rows[2].ColumnStopped)
                {
                    _gameStarted = true;

                    rows[0].GetComponent<Column>().StartRotating();
                    rows[1].GetComponent<Column>().StartRotating();
                    rows[2].GetComponent<Column>().StartRotating();
                }
            }

            _roundPaidFor = false;
        }

        private bool IsRoundPaidFor()
        {
            return _roundPaidFor;
        }

        public static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (!condition()) await Task.Delay(frequency);
            });

            if (waitTask != await Task.WhenAny(waitTask,
                    Task.Delay(timeout)))
                throw new TimeoutException();
        }

        private void Update()
        {
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