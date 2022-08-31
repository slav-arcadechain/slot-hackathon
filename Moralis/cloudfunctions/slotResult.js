const contractAddress = "0xaF131deE7926CA18d2c68c4B924DA5F3EFadaCAF";
const operatorAddress = "0x588acF052631756422844856e1dc2Ef6066ce121";
const chain = "0x152";
const logger = Moralis.Cloud.getLogger();


Moralis.Cloud.afterSave("SlotGameEntered", async (request) => {
        let reward = await getRewardInWei(request);

        const abi = [{
            "type": "function",
            "stateMutability": "nonpayable",
            "outputs": [],
            "name": "setRoundResult",
            "inputs": [
                {
                    "type": "uint256",
                    "name": "_roundId",
                    "internalType": "uint256"
                },
                {
                    "type": "uint256",
                    "name": "_amount",
                    "internalType": "uint256"
                }
            ]
        }];

        const web3 = Moralis.web3ByChain(chain);
        const contract = new web3.eth.Contract(abi, contractAddress);
        const contractMethod = contract.methods.setRoundResult(request.object.get("roundId"), reward);
        await signAndSendTransaction(web3, contractMethod)
            .catch((e) => logger.error(`setRoundResult: ${e}${JSON.stringify(e, null, 2)}`))

        return request.params;
    }
);

Moralis.Cloud.define("setGameResult", async (request) => {

});

async function signAndSendTransaction(web3, cM) {
    let price = await cM.estimateGas(
        {
            from: operatorAddress,
            gasPrice: web3.utils.toWei("2000", "gwei"),
        }
    );

    fnCall = cM.encodeABI()

    const transaction = {
        to: contractAddress,
        data: fnCall,
        gas: price,
        gasPrice: web3.utils.toWei("2000", "gwei"),
    };

    const config = await Moralis.Config.get({useMasterKey: true});
    const key = config.get("operatorKey");
    let signedTransaction = await web3.eth.accounts.signTransaction(
        transaction,
        key
    );

    return await web3.eth.sendSignedTransaction(
        signedTransaction.raw || signedTransaction.rawTransaction
    );
}

async function getRewardInWei(request) {
    let rewardWei;
    const SlotGameResultToWei = Moralis.Object.extend("SlotGameResultToWei");
    const query = new Moralis.Query(SlotGameResultToWei);
    query.equalTo("gameResult", request.object.get("gameResult"))
    const results = await query.find();
    rewardWei = results[0].get("rewardWeiString");
    return rewardWei;
}
