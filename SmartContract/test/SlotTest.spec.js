const Slot = artifacts.require("Slot");
const {assert} = require('chai');
const BigNumber = require('bignumber.js');
const Tusd = artifacts.require("TUSD");


require('chai')
    .use(require('chai-as-promised'))
    .should();

let accounts;
// let contract;

contract('Slot', (accs) => {
    accounts = accs;
    const admin = accounts[1];
    const operator = accounts[2];
    const user = accounts[3];
    const gas = 5000000;
    const gameFee = 10;
    const gasPrice = 500000000;
    let contract;
    let token;
    let userTusdInitalBalance = 100;

    beforeEach('should setup the contract instance', async () => {
        token = await Tusd.new(1000);
        await token.transfer(user, userTusdInitalBalance);

        contract = await Slot.new(admin, operator, token.address, gameFee);
    });

    describe('Admin', function () {

        it('can deploy', async () => {
            let instance = await Slot.deployed();

            assert(instance);
        });

        it('should set operator', async () => {

            await contract.setOperator(user, {from: admin, gas: gas, gasPrice: 500000000})

            let currentOperator = await contract.operatorAddress.call();
            assert.equal(user, currentOperator);

        });

        it('user should not set operator', async () => {

            try {
                await contract.setOperator(user, {from: user, gas: gas, gasPrice: 500000000})
            } catch (error) {
                assert.equal("Not admin", error.reason)
                let currentOperator = await contract.operatorAddress.call();
                assert.notEqual(user, currentOperator);
                return;
            }

            expect.fail("should not get here");
        });

        it('should be able to claim treasury', async () => {

            const userStartBalance = await token.balanceOf(user);

            await token.approve(contract.address, gameFee, {from: user});
            await contract.enterGame(1, {from: user, gas: gas, gasPrice: gasPrice});
            const userEndBalance = await token.balanceOf(user);

            assert.equal(userStartBalance - userEndBalance, gameFee)

            //check contract got the funds from the game
            let contractBalance = await token.balanceOf(contract.address);
            assert.equal(contractBalance, gameFee);

            // check admin account credited when claimed
            const adminStartBalance = await token.balanceOf(admin);
            await contract.claimTreasury(gameFee / 2, {from: admin, gas: gas, gasPrice: gasPrice});
            const adminEndBalance = await token.balanceOf(admin);

            assert.equal(adminEndBalance - adminStartBalance, gameFee / 2);

            //check contract founds are less after claimed
            contractBalance = await token.balanceOf(contract.address);
            assert.equal(contractBalance, gameFee / 2);
        });

    });

    describe("User Game play", async () => {

        it('player should not be able to play if not enough money', async () => {
            try {
                await token.approve(contract.address, gameFee - 1, {from: user});
                await contract.enterGame(1, {from: user, gas: gas, gasPrice: gasPrice});
            } catch (e) {
                assert.equal("ERC20: insufficient allowance", e.reason)
                return;
            }

            expect.fail("should not get here");
        });

        it('player should not be able to pay more than set fee', async () => {
            await token.approve(contract.address, gameFee + 1, {from: user});
            await contract.enterGame(1, {from: user, gas: gas, gasPrice: gasPrice});
            let userBalance = await token.balanceOf(user);
            assert.equal(userBalance, userTusdInitalBalance - gameFee);
        });

        it('player should not be able to reuse roundId', async () => {
            await token.approve(contract.address, gameFee, {from: user});
            await contract.enterGame(1, {from: user, gas: gas, gasPrice: gasPrice});
            try {
                await token.approve(contract.address, gameFee, {from: user});
                await contract.enterGame(1, {from: user, gas: gas, gasPrice: gasPrice});
            } catch (e) {
                assert.equal("existing roundId", e.reason)
                return;
            }
            expect.fail("should not get here");
        });

        it('player can play for a fee', async () => {
            await token.approve(contract.address, gameFee, {from: user});
            await contract.enterGame(1, {from: user, gas: gas, gasPrice: gasPrice});

            let userContractBalance = await contract.getUserWinnings(user);
            assert.equal(userContractBalance, 0);

            let userRounds = await contract.getUserRounds(user);
            assert.equal(userRounds.length, 1)
            assert.equal(userRounds[0], 1)

            let legerEntry = await contract.getLegerEntryForRoundId(1);
            assert.isFalse(legerEntry.updated)
            assert.equal(legerEntry.amount, gameFee)
            assert.equal(legerEntry.playerAddress, user)

            const contractBalance = await token.balanceOf(contract.address);
            assert.equal(contractBalance, gameFee)
        });

        it('player should be able to claim winnings', async () => {
            await token.approve(contract.address, gameFee * 3, {from: user});
            await contract.enterGame(1, {from: user, gas: gas, gasPrice: gasPrice});
            await contract.enterGame(2, {from: user, gas: gas, gasPrice: gasPrice});
            await contract.enterGame(3, {from: user, gas: gas, gasPrice: gasPrice});

            await contract.setRoundResult(1, gameFee * 1.1, {from: operator, gas: gas, gasPrice: gasPrice});
            await contract.setRoundResult(3, gameFee / 2, {from: operator, gas: gas, gasPrice: gasPrice});

            // check admin account credited when claimed
            const userStartBalance = await token.balanceOf(user);
            let claim = await contract.claim({from: user, gas: gas, gasPrice: gasPrice})
            const userEndBalance = await token.balanceOf(user);

            // winnings transferred
            assert.equal(userEndBalance - userStartBalance, (gameFee * 1.1) + (gameFee / 2));

            // balance set to zero
            let userContractBalance = await contract.getUserWinnings(user);
            assert.equal(userContractBalance, 0);

            // user rounds updated
            let userRounds = await contract.getUserRounds(user, {from: user, gas: gas, gasPrice: gasPrice});
            assert.equal(userRounds.length, 3)
            assert.equal(userRounds[0], 1)
            assert.equal(userRounds[1], 2)
            assert.equal(userRounds[2], 3)

            // updated in ledger
            let legerEntry = await contract.getLegerEntryForRoundId(1);
            assert.isTrue(legerEntry.updated)
            assert.isTrue(legerEntry.claimed)
            assert.equal(legerEntry.amount, gameFee * 1.1)
            assert.equal(legerEntry.playerAddress, user)
            assert.equal(legerEntry.roundId, 1)

            legerEntry = await contract.getLegerEntryForRoundId(2);
            assert.isFalse(legerEntry.updated)
            assert.isFalse(legerEntry.claimed)
            assert.equal(legerEntry.amount, gameFee)
            assert.equal(legerEntry.playerAddress, user)
            assert.equal(legerEntry.roundId, 2);

            legerEntry = await contract.getLegerEntryForRoundId(3);
            assert.isTrue(legerEntry.updated)
            assert.isTrue(legerEntry.claimed)
            assert.equal(legerEntry.amount, gameFee / 2)
            assert.equal(legerEntry.playerAddress, user)
            assert.equal(legerEntry.roundId, 3)
        });
    });

    describe('Game operations', async () => {
        it('should set game result for roundId', async () => {
            await token.approve(contract.address, gameFee, {from: user});
            await contract.enterGame(1, {from: user, gas: gas, gasPrice: gasPrice});

            await contract.setRoundResult(1, gameFee * 2, {from: operator, gas: gas, gasPrice: gasPrice});

            let userBalance = new BigNumber(await contract.getUserWinnings(user));
            assert.equal(userBalance.toNumber(), gameFee * 2);
        });

        it('should not be able to set result on unknown roundId', async () => {
            try {
                await contract.setRoundResult(1, gameFee * 2, {from: operator, gas: gas, gasPrice: gasPrice});
            } catch (e) {
                assert.equal(e.reason, "not existing roundId");
                return;
            }
            expect.fail("should not get here");
        });
    })
});

async function advanceBlock() {
    return new Promise((resolve, reject) => {
        web3.currentProvider.send({
            jsonrpc: '2.0',
            method: 'evm_mine',
            id: new Date().getTime()
        }, (err, result) => {
            if (err) {
                return reject(err)
            }
            const newBlockHash = web3.eth.getBlock('latest').hash

            return resolve(newBlockHash)
        });
    });
};