const TUSD = artifacts.require("TUSD");

module.exports = function (deployer) {
    deployer.deploy(TUSD, "1000000000000000000000000000");
};
