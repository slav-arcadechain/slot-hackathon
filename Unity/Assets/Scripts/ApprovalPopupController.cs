using System;
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
    private GameObject approvePopup;
    private GameObject mainBackground;
    private Slider slider;
    private Button approveButton;
    private Button closeApporveButton;

    void Start()
    {
        approvePopup = GameObject.Find("ApprovalPopup");
        slider = GameObject.Find("ApproveSlider")?.GetComponent<Slider>();
        Debug.Log("slider = " + slider);
        slider.onValueChanged.AddListener(delegate { HandleSlider(); });
        approveButton = GameObject.Find("ApproveButton").GetComponent<Button>();
        closeApporveButton = GameObject.Find("CloseApproveButton").GetComponent<Button>();
        mainBackground = GameObject.Find("MainBackground");

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
        Debug.Log("in close");
        approvePopup.SetActive(false);
        mainBackground.SetActive(false);
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (go.name == "SlotPanel")
            {
                // go.SetActive(true);
            }
        }
    }

    private void ApproveButtonHandler()
    {
        UniTask.Create(async () => { await BlockChain.ApproveGameTokenSpent((int)slider.value * 10); });
    }

    private void HandleSlider()
    {
        Debug.Log("in slider handle");
        var value = slider.value;
        GameObject.Find("ApproveButtonText").GetComponent<TextMeshProUGUI>().text = $"Approve {value}";
    }

    private void UpdateTokenApproval(decimal approvedAmount)
    {
        Debug.Log("UpdateTokenApproval: " + approvedAmount);
        if (GameObject.Find("ApprovedAmount") != null)
        {
            if (approvedAmount >= Slot.GameFee)
            {
                closeApporveButton.interactable = true; 
                GameObject.Find("ApprovedAmount").GetComponent<Text>().text =
                    $"{Math.Round(approvedAmount / Slot.GameFee, 0)}";
            }
            else
            {
                GameObject.Find("ApprovedAmount").GetComponent<Text>().text = "0";
            }
        }
    }

    private void UpdateWalletTokens(decimal balance)
    {
        Debug.Log("in wallet balance...");
        if (GameObject.Find("WalletBalance") != null)
        {
            Debug.Log($"---> {Math.Round(balance, 0)}");
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
            StartCoroutine(blockChain.HandleWallet());
        }
    }
}