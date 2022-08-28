using System.Collections;
using System.Linq;
using System.Numerics;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using UnityEngine;

public class BlockChain : MonoBehaviour
{
    public const string GameTokenContractAddress = "0x912aAEA32355DA6FeB20D98E73B9C81B5afd6A2e"; //cronos testnet
    public const string GameContractAddress = "0xaF131deE7926CA18d2c68c4B924DA5F3EFadaCAF"; //cronos testnet
    public const int GameTokenContractDecimals = 18;
    public const ChainList GameChain = ChainList.cronos_testnet;

    public const string ERC20Abi =
        "[{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Approval\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Transfer\",\"type\":\"event\"},{\"inputs\":[],\"name\":\"totalSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"transfer\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"}],\"name\":\"allowance\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"approve\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"transferFrom\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";

    public const string GameAbi =
        "[{\"type\":\"constructor\",\"stateMutability\":\"nonpayable\",\"inputs\":[{\"type\":\"address\",\"name\":\"_adminAddress\",\"internalType\":\"address\"},{\"type\":\"address\",\"name\":\"_operatorAddress\",\"internalType\":\"address\"},{\"type\":\"address\",\"name\":\"_gameTokenAddress\",\"internalType\":\"address\"},{\"type\":\"uint256\",\"name\":\"_gameFee\",\"internalType\":\"uint256\"}]},{\"type\":\"event\",\"name\":\"GameEntered\",\"inputs\":[{\"type\":\"uint256\",\"name\":\"roundId\",\"internalType\":\"uint256\",\"indexed\":false},{\"type\":\"address\",\"name\":\"user\",\"internalType\":\"address\",\"indexed\":false},{\"type\":\"uint256\",\"name\":\"gameFee\",\"internalType\":\"uint256\",\"indexed\":false}],\"anonymous\":false},{\"type\":\"event\",\"name\":\"GameFeeSet\",\"inputs\":[{\"type\":\"uint256\",\"name\":\"gameFee\",\"internalType\":\"uint256\",\"indexed\":false}],\"anonymous\":false},{\"type\":\"event\",\"name\":\"NewGameToken\",\"inputs\":[{\"type\":\"address\",\"name\":\"tokenAddress\",\"internalType\":\"address\",\"indexed\":false}],\"anonymous\":false},{\"type\":\"event\",\"name\":\"NewOperatorAddress\",\"inputs\":[{\"type\":\"address\",\"name\":\"operator\",\"internalType\":\"address\",\"indexed\":false}],\"anonymous\":false},{\"type\":\"event\",\"name\":\"OwnershipTransferred\",\"inputs\":[{\"type\":\"address\",\"name\":\"previousOwner\",\"internalType\":\"address\",\"indexed\":true},{\"type\":\"address\",\"name\":\"newOwner\",\"internalType\":\"address\",\"indexed\":true}],\"anonymous\":false},{\"type\":\"event\",\"name\":\"Paused\",\"inputs\":[{\"type\":\"address\",\"name\":\"account\",\"internalType\":\"address\",\"indexed\":false}],\"anonymous\":false},{\"type\":\"event\",\"name\":\"PlayerClaimed\",\"inputs\":[{\"type\":\"address\",\"name\":\"player\",\"internalType\":\"address\",\"indexed\":false},{\"type\":\"uint256\",\"name\":\"amount\",\"internalType\":\"uint256\",\"indexed\":false}],\"anonymous\":false},{\"type\":\"event\",\"name\":\"ResultUpdated\",\"inputs\":[{\"type\":\"uint256\",\"name\":\"roundId\",\"internalType\":\"uint256\",\"indexed\":false},{\"type\":\"uint256\",\"name\":\"amount\",\"internalType\":\"uint256\",\"indexed\":false}],\"anonymous\":false},{\"type\":\"event\",\"name\":\"TreasuryClaim\",\"inputs\":[{\"type\":\"uint256\",\"name\":\"amount\",\"internalType\":\"uint256\",\"indexed\":false}],\"anonymous\":false},{\"type\":\"event\",\"name\":\"Unpaused\",\"inputs\":[{\"type\":\"address\",\"name\":\"account\",\"internalType\":\"address\",\"indexed\":false}],\"anonymous\":false},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"address\",\"name\":\"\",\"internalType\":\"address\"}],\"name\":\"adminAddress\",\"inputs\":[]},{\"type\":\"function\",\"stateMutability\":\"nonpayable\",\"outputs\":[],\"name\":\"claim\",\"inputs\":[]},{\"type\":\"function\",\"stateMutability\":\"nonpayable\",\"outputs\":[],\"name\":\"claimTreasury\",\"inputs\":[{\"type\":\"uint256\",\"name\":\"value\",\"internalType\":\"uint256\"}]},{\"type\":\"function\",\"stateMutability\":\"nonpayable\",\"outputs\":[],\"name\":\"enterGame\",\"inputs\":[{\"type\":\"uint256\",\"name\":\"_roundId\",\"internalType\":\"uint256\"}]},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"uint256\",\"name\":\"\",\"internalType\":\"uint256\"}],\"name\":\"gameFee\",\"inputs\":[]},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"address\",\"name\":\"\",\"internalType\":\"address\"}],\"name\":\"gameToken\",\"inputs\":[]},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"tuple\",\"name\":\"\",\"internalType\":\"struct Slot.RoundInfo\",\"components\":[{\"type\":\"address\",\"name\":\"playerAddress\",\"internalType\":\"address\"},{\"type\":\"uint256\",\"name\":\"roundId\",\"internalType\":\"uint256\"},{\"type\":\"uint256\",\"name\":\"amount\",\"internalType\":\"uint256\"},{\"type\":\"bool\",\"name\":\"updated\",\"internalType\":\"bool\"},{\"type\":\"bool\",\"name\":\"claimed\",\"internalType\":\"bool\"}]}],\"name\":\"getLegerEntryForRoundId\",\"inputs\":[{\"type\":\"uint256\",\"name\":\"_roundId\",\"internalType\":\"uint256\"}]},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"uint256[]\",\"name\":\"\",\"internalType\":\"uint256[]\"}],\"name\":\"getUserRounds\",\"inputs\":[{\"type\":\"address\",\"name\":\"_address\",\"internalType\":\"address\"}]},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"uint256\",\"name\":\"\",\"internalType\":\"uint256\"}],\"name\":\"getUserWinnings\",\"inputs\":[{\"type\":\"address\",\"name\":\"_address\",\"internalType\":\"address\"}]},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"address\",\"name\":\"playerAddress\",\"internalType\":\"address\"},{\"type\":\"uint256\",\"name\":\"roundId\",\"internalType\":\"uint256\"},{\"type\":\"uint256\",\"name\":\"amount\",\"internalType\":\"uint256\"},{\"type\":\"bool\",\"name\":\"updated\",\"internalType\":\"bool\"},{\"type\":\"bool\",\"name\":\"claimed\",\"internalType\":\"bool\"}],\"name\":\"ledger\",\"inputs\":[{\"type\":\"uint256\",\"name\":\"\",\"internalType\":\"uint256\"}]},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"address\",\"name\":\"\",\"internalType\":\"address\"}],\"name\":\"operatorAddress\",\"inputs\":[]},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"address\",\"name\":\"\",\"internalType\":\"address\"}],\"name\":\"owner\",\"inputs\":[]},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"bool\",\"name\":\"\",\"internalType\":\"bool\"}],\"name\":\"paused\",\"inputs\":[]},{\"type\":\"function\",\"stateMutability\":\"nonpayable\",\"outputs\":[],\"name\":\"renounceOwnership\",\"inputs\":[]},{\"type\":\"function\",\"stateMutability\":\"nonpayable\",\"outputs\":[],\"name\":\"setGameFee\",\"inputs\":[{\"type\":\"uint256\",\"name\":\"_gameFee\",\"internalType\":\"uint256\"}]},{\"type\":\"function\",\"stateMutability\":\"nonpayable\",\"outputs\":[],\"name\":\"setGameToken\",\"inputs\":[{\"type\":\"address\",\"name\":\"tokenAddress\",\"internalType\":\"address\"}]},{\"type\":\"function\",\"stateMutability\":\"nonpayable\",\"outputs\":[],\"name\":\"setOperator\",\"inputs\":[{\"type\":\"address\",\"name\":\"_operatorAddress\",\"internalType\":\"address\"}]},{\"type\":\"function\",\"stateMutability\":\"nonpayable\",\"outputs\":[],\"name\":\"setRoundResult\",\"inputs\":[{\"type\":\"uint256\",\"name\":\"_roundId\",\"internalType\":\"uint256\"},{\"type\":\"uint256\",\"name\":\"_amount\",\"internalType\":\"uint256\"}]},{\"type\":\"function\",\"stateMutability\":\"nonpayable\",\"outputs\":[],\"name\":\"transferOwnership\",\"inputs\":[{\"type\":\"address\",\"name\":\"newOwner\",\"internalType\":\"address\"}]},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"uint256\",\"name\":\"\",\"internalType\":\"uint256\"}],\"name\":\"userRounds\",\"inputs\":[{\"type\":\"address\",\"name\":\"\",\"internalType\":\"address\"},{\"type\":\"uint256\",\"name\":\"\",\"internalType\":\"uint256\"}]},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"uint256\",\"name\":\"\",\"internalType\":\"uint256\"}],\"name\":\"userWinnings\",\"inputs\":[{\"type\":\"address\",\"name\":\"\",\"internalType\":\"address\"}]}]";

    #region Internal Methods

    private void Awake()
    {
        StartCoroutine(LoadInternal());
    }

    private IEnumerator LoadInternal()
    {
        yield break;
    }

    public async UniTask HandleWallet()
    {
        var address = (await Moralis.GetUserAsync()).accounts[0];
        var balance = await Moralis.Client.Web3Api.Account.GetTokenBalances(address.ToLower(), GameChain);
        var allowance = await Moralis.Client.Web3Api.Token.GetTokenAllowance(
            GameTokenContractAddress,
            address.ToLower(),
            GameContractAddress.ToLower(),
            GameChain);
        var user = FindObjectOfType<User>();
        foreach (var token in balance.Where(token =>
                     token.TokenAddress.ToLower().Equals(GameTokenContractAddress.ToLower())))
        {
            user.WalletTokenBalance = UnitConversion.Convert.FromWei(BigInteger.Parse(token.Balance));
            user.ApprovedTokenBalance = UnitConversion.Convert.FromWei(BigInteger.Parse(allowance.Allowance));
        }
    }

    public static async UniTask ApproveGameTokenSpent(int amount)
    {
        BigInteger gameTokenInWei = UnitConversion.Convert.ToWei(amount, GameTokenContractDecimals);
        object[] parameters =
        {
            GameContractAddress, gameTokenInWei
        };

        var value = new HexBigInteger(_zeroHex);
        var gas = getGas();
        var gasPrice = getGasPrice();

        // approve token spent
        await Moralis.ExecuteContractFunction(
            contractAddress: GameTokenContractAddress,
            abi: ERC20Abi,
            functionName: "approve",
            args: parameters,
            value: value,
            gas: gas,
            gasPrice: gasPrice);
    }

    public static async UniTask EnterGameOnBlockchain(BigInteger gameId)
    {
        // enter game on block chain
        object[] parameters =
        {
            gameId
        };

        var value = new HexBigInteger(_zeroHex);
        var gas = getGas();
        var gasPrice = getGasPrice();
        await Moralis.ExecuteContractFunction(
            contractAddress: GameContractAddress,
            abi: GameAbi,
            functionName: "enterGame",
            args: parameters,
            value: value,
            gas: gas,
            gasPrice: gasPrice);
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

    private static HexBigInteger getGasPrice()
    {
        return new HexBigInteger("300000000");
    }

    private static HexBigInteger getGas()
    {
        return new HexBigInteger("14500");
    }

    #endregion

    #region Instance

    private static BlockChain instance;
    private static string _zeroHex = "0x0";

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
            // gameObject.hideFlags = HideFlags.HideAndDontSave; //hides from Unity editor

            instance = gameObject.AddComponent<BlockChain>();
            DontDestroyOnLoad(gameObject); //prevents destroy on changing scene 
        }
    }

    #endregion
}