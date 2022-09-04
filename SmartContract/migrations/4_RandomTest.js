const RandomTest = artifacts.require("RandomTest");
const BN = require('bn.js');

module.exports = function (deployer, network, accounts) {
    deployer.deploy(RandomTest,
        '0x0017A464A86f48B342Cae3b8Fe29cFCDaA7b0643'
        );
};
