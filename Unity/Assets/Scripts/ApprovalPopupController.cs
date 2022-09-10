using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ApprovalPopupController : MonoBehaviour

{
    [SerializeField] private BlockChain blockChain = null;
    [SerializeField] private User user = null;

    private Slider _slider;
    private Button _approveButton;
    private Button _closeApproveButton;
    private GameController _gameController;
    private bool _isViewActive;

    void Start()
    {
        _slider = GameObject.Find("ApproveSlider").GetComponent<Slider>();
        _slider.onValueChanged.AddListener(delegate { HandleSlider(); });
        _approveButton = GameObject.Find("ApproveButton").GetComponent<Button>();
        _closeApproveButton = GameObject.Find("CloseApproveButton").GetComponent<Button>();

        blockChain = FindObjectOfType<BlockChain>();
        _approveButton.onClick.AddListener(ApproveButtonHandler);
        _closeApproveButton.onClick.AddListener(CloseApproveButtonHandler);
        _gameController = FindObjectOfType<GameController>();
        _gameController.OnViewTransitioned += HandleViewTransitioned;
        user = FindObjectOfType<User>();
        user.OnWalletTokenBalanceUpdated += UpdateWalletTokens;
        user.OnTokenApprovalUpdated += UpdateTokenApproval;
    }

    private void HandleViewTransitioned(string viewName)
    {
        if (viewName == "approval")
        {
            _isViewActive = true;
            StartCoroutine(blockChain.HandleAllowance());
            StartCoroutine(blockChain.HandleGameTokens());
        }
        else
        {
            _isViewActive = false;
        }
    }

    private void CloseApproveButtonHandler()
    {
        _gameController.ShowGame();
        // GameController.HideApproval();
    }

    private void ApproveButtonHandler()
    {
        GameController.ins.ShowLoader();
        _approveButton.interactable = false;
        UniTask.Create(async () => { await BlockChain.ApproveGameTokenSpent((int)_slider.value * 10); });
    }

    private void HandleSlider()
    {
        GameObject.Find("ApproveButtonText").GetComponent<TextMeshProUGUI>().text = $"Approve {_slider.value}";
    }

    private void UpdateTokenApproval(decimal approvedAmount)
    {
        if (GameObject.Find("ApprovedAmount") != null && _isViewActive)
        {
            if (approvedAmount >= Slot.GameFee)
            {
                GameController.ins.HideLoader();
                _approveButton.interactable = true;
                _closeApproveButton.interactable = true;
                GameObject.Find("ApprovedAmount").GetComponent<Text>().text =
                    $"{Math.Round(approvedAmount / Slot.GameFee, 0)}";
                StartCoroutine(WaitForSecond());
            }
            else
            {
                GameObject.Find("ApprovedAmount").GetComponent<Text>().text = "0";
            }
        }
    }

    private void UpdateWalletTokens(decimal balance)
    {
        if (GameObject.Find("WalletBalance") != null && _isViewActive)
        {
            if (balance > 9999)
            {
                GameObject.Find("WalletBalance").GetComponent<Text>().text = ">10k";
            }
            else
            {
                GameObject.Find("WalletBalance").GetComponent<Text>().text = $"{Math.Round(balance, 0)}";
            }
        }
    }

    private IEnumerator WaitForSecond()
    {
        for (int a = 0; a < 50; a++)
        {
            yield return new WaitForSeconds(0.1f);
        }

        CloseApproveButtonHandler();
    }
}
