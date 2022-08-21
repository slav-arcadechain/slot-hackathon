using System.Collections;
using System.Collections.Generic;
using MoralisUnity.Kits.AuthenticationKit;
using UnityEngine;

public class GameController : MonoBehaviour
{
        [Header("Dependencies")]
        [SerializeField] private AuthenticationKit authenticationKit = null;
       
        private GameObject approvePanel;
        private bool approvalUpdateAvailable = false;


        void Start()
    {
        approvePanel = GameObject.Find("ApprovePanel");
        approvePanel.SetActive(false);
        
        authenticationKit.OnStateChanged.AddListener(AuthOnStateChangedListener);
    }

    private void AuthOnStateChangedListener(AuthenticationKitState state)
    {
        switch (state)
        {
            case AuthenticationKitState.Disconnected:
                Debug.Log("disconnected");
                approvePanel.SetActive(false);
                break;
            
            case AuthenticationKitState.WalletSigned:
                Debug.Log("connected");
                approvePanel.SetActive(true);
                approvalUpdateAvailable = true;
                GameObject.Find("BackgroundImage").SetActive(false);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
