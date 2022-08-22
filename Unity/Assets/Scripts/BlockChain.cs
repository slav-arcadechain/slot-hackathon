    using System.Collections;
    using MoralisUnity.Web3Api.Models;
    using UnityEngine;

    public class BlockChain: MonoBehaviour
    {
        public const string GameTokenContractAddress = "0xCF4Fd69742f30c65a5adBE2D3b4F606aF2C6C92f"; //cronos testnet
        public const int GameTokenContractDecimals = 18;
        public const ChainList GameChain = ChainList.cronos_testnet;
        public const string ERC20Abi =
            "[{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Approval\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Transfer\",\"type\":\"event\"},{\"inputs\":[],\"name\":\"totalSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"transfer\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"}],\"name\":\"allowance\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"approve\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"transferFrom\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";

        #region Internal Methods
 
        private void Awake()
        {
            Debug.Log("Loading planets...");
            StartCoroutine(LoadInternal());
        }
 
        private IEnumerator LoadInternal()
        {
            yield break;
            
            // var path = Application.streamingAssetsPath + "/planets.json";
            // using (var www = new WWW(path))
            // {
            //     yield return new WaitForSeconds(5); // Pretend the network is slow
            //     yield return www;
            //     planets = JsonUtility.FromJson<Planets>(www.text).planets;
            // }
        }
 
        bool quitting = false;
 
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
 
        private static BlockChain instance;
        private static BlockChain Instance
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
                var gameObject = new GameObject("BlockChain");
                gameObject.hideFlags = HideFlags.HideAndDontSave; //hides from Unity editor
 
                instance = gameObject.AddComponent<BlockChain>();
                DontDestroyOnLoad(gameObject); //prevents destroy on changing scene 
            }
        }
 
        #endregion 
    }