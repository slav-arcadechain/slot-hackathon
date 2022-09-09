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
    [SerializeField] private Slot slot = null;

    private bool shouldUpdateWallet = true;
    private bool hidePanel = true;
    private Slider slider;
    private Button approveButton;
    private Button closeApporveButton;

    void Start()
    {
        slider = GameObject.Find("ApproveSlider")?.GetComponent<Slider>();
        Debug.Log("slider = " + slider);
        slider.onValueChanged.AddListener(delegate { HandleSlider(); });
        approveButton = GameObject.Find("ApproveButton").GetComponent<Button>();
        closeApporveButton = GameObject.Find("CloseApproveButton").GetComponent<Button>();

        blockChain = FindObjectOfType<BlockChain>();
        approveButton.onClick.AddListener(ApproveButtonHandler);
        closeApporveButton.onClick.AddListener(CloseApproveButtonHandler);
        user = FindObjectOfType<User>();
        user.OnWalletTokenBalanceUpdated += UpdateWalletTokens;
        user.OnTokenApprovalUpdated += UpdateTokenApproval;
        user.OnWinningsUpdated += UpdateWinnings;
        slot = FindObjectOfType<Slot>();
    }

    private void UpdateWinnings(decimal winnings)
    {
        Debug.Log("Winnings: " + winnings);
        if (GameObject.Find("WonAmount"))
        {
            GameObject.Find("WonAmount").GetComponent<Text>().text = winnings.ToString();
        }
    }

    private void CloseApproveButtonHandler()
    {
        GameController.ShowGame();
        GameController.HideApproval();
    }

    private void ApproveButtonHandler()
    {
        approveButton.interactable = false;
        UniTask.Create(async () => { await BlockChain.ApproveGameTokenSpent((int)slider.value * 10); });
        shouldUpdateWallet = true;
    }

    private void HandleSlider()
    {
        Debug.Log("in slider handle");
        var value = slider.value;
        GameObject.Find("ApproveButtonText").GetComponent<TextMeshProUGUI>().text = $"Approve {value}";
    }

    private void UpdateTokenApproval(decimal approvedAmount)
    {
        if (GameObject.Find("ApprovedAmount") != null)
        {
            if (approvedAmount >= Slot.GameFee)
            {
                approveButton.interactable = true;
                closeApporveButton.interactable = true;
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
        if (GameObject.Find("WalletBalance") != null)
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

    private async void Update()
    {
        if (shouldUpdateWallet)
        {
            shouldUpdateWallet = false;
            StartCoroutine(blockChain.HandleWallet(12));
            StartCoroutine(blockChain.HandleWallet(20));
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