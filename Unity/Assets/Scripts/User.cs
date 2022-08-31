using System;
using System.Collections;
using System.Numerics;
using UnityEngine;


public delegate void WalletTokenBalanceUpdatedHandler(decimal walletBalance);
public delegate void TokenApprovalUpdatedHandler(decimal approvedAmount);
public delegate void WinningsUpdatedHandler(decimal winningsAmount);

public class User : MonoBehaviour
{
    #region Events

    public event WalletTokenBalanceUpdatedHandler OnWalletTokenBalanceUpdated;
    public event TokenApprovalUpdatedHandler OnTokenApprovalUpdated;
    public event WinningsUpdatedHandler OnWinningsUpdated;

    #endregion
    #region Internal Methods
    
    
    [Serializable]
    public struct UserStats
    {
        public decimal walletTokenBalance;
        public decimal approvedTokenBalance;
        public decimal winningsBalance;
    }
    
    public decimal walletTokenBalance;

    public decimal WalletTokenBalance
    {
        get => walletTokenBalance;
        set
        {
            Debug.Log("setting wallet balance");
            walletTokenBalance = value;
            OnWalletTokenBalanceUpdated?.Invoke(value);
        }
    }

    public decimal approvedTokenBalance;

    public decimal ApprovedTokenBalance
    {
        get => approvedTokenBalance;
        set
        {
            Debug.Log("setting appoval");
            approvedTokenBalance = value;
            OnTokenApprovalUpdated?.Invoke(value);
        }
    }

    public decimal winningsBalance;
    public decimal WinningsBalance
    {
        get => winningsBalance;
        set
        {
            Debug.Log("setting winnings");
            winningsBalance = value;
            OnWinningsUpdated?.Invoke(value);
        }
    }

    bool quitting = false;

    private void Awake()
    {
        StartCoroutine(LoadInternal());
    }
 
    private IEnumerator LoadInternal()
    {
        yield break;
    }
 
 
    private void OnApplicationQuit()
    {
        quitting = true;
    }
 
    private void OnDestroy()
    {
        if (!quitting)
        {
            instance = null;
            Init();
        }
    }
 
    #endregion
 
    #region Instance
 
    private static User instance;
    private static User Instance
    {
        get
        {
            Init();
            return instance;
        }
    }
 
    [RuntimeInitializeOnLoadMethod] // this enables eager loading
    private static void Init()
    {
        if (instance == null || instance.Equals(null))
        {
            var gameObject = new GameObject("User");
            // gameObject.hideFlags = HideFlags.HideAndDontSave; //hides from Unity editor
 
            instance = gameObject.AddComponent<User>();
            DontDestroyOnLoad(gameObject); //prevents destroy on changing scene 
        }
    }
 
    #endregion
}