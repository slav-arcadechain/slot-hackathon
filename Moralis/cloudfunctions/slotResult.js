const contractAddress = "0xaF131deE7926CA18d2c68c4B924DA5F3EFadaCAF";
const operatorAddress = "0x588acF052631756422844856e1dc2Ef6066ce121";
const chain = "0x152";
const logger = Moralis.Cloud.getLogger();

Moralis.Cloud.beforeSave("SlotGameEntered", async (request) => {
    if(await wonSomething()) {
        request.object.set("gameWon", true);
        request.object.set("gameResult", await getReward());
    } else {
        request.object.set("gameWon", false);
    }
});


Moralis.Cloud.afterSave("SlotGameEntered", async (request) => {
        if (!request.object.get("gameWon")) {
            return;
        }
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

        logger.info("boom: " + request.object.get("roundId") + " reward:" + reward);
        const web3 = Moralis.web3ByChain(chain);
        const contract = new web3.eth.Contract(abi, contractAddress);
        const contractMethod = contract.methods.setRoundResult(request.object.get("roundId"), reward);
        logger.info("boom 2");
        await signAndSendTransaction(web3, contractMethod)
            .catch((e) => logger.error(`setRoundResult: ${e}${JSON.stringify(e, null, 2)}`))

        return request.params;
    }
);

Moralis.Cloud.define("setGameResult", async (request) => {

});

async function signAndSendTransaction(web3, cM) {
    logger.info("boom3");
    let price = '1048576'
    logger.info("boom4: " + price);
    fnCall = cM.encodeABI()

    const transaction = {
        to: contractAddress,
        data: fnCall,
        gas: price,
        gasPrice: web3.utils.toWei("500", "gwei"),
    };

    logger.info("transaction: " + JSON.stringify(transaction));

    const config = await Moralis.Config.get({useMasterKey: true});
    const key = config.get("operatorKey");
    logger.info("key + " + key);

    let signedTransaction = await web3.eth.accounts.signTransaction(
        transaction,
        key
    );
    logger.info("signed " + signedTransaction);

    logger.info("before sending transaction")
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

async function wonSomething() {
    const winningChance = 90;

    let randomNumber = Math.floor(Math.random() * 101);
    return randomNumber < winningChance;
}

async function getReward() {
    const rewardChances = [10, 20, 30, 40, 50, 75, 100];
    let randomNumber = Math.floor(Math.random() * 101);
    for (let i = 0; i < rewardChances.length; i++) {
        if (randomNumber <= rewardChances[i]) {
            return i;
        }
    }
}
