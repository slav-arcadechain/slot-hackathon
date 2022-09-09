const Acnft = artifacts.require("ACNFT");
const BN = require('bn.js');

module.exports = function (deployer, network, accounts) {
    deployer.deploy(Acnft);
};
