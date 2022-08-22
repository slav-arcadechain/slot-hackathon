const HDWalletProvider = require('@truffle/hdwallet-provider');
const fs = require('fs');
const mnemonic = fs.readFileSync(".secret").toString().trim();
const path = require("path");

module.exports = {
  contracts_build_directory: path.join(__dirname, "app/src/contracts"),
  networks: {
    development: {
      host: "127.0.0.1",     // Localhost (default: none)
      port: 8545,            // Standard Ethereum port (default: none)
      network_id: "*",       // Any network (default: none)
    },
    local: {
      host: "127.0.0.1",     // Localhost (default: none)
      port: 7545,            // Standard Ethereum port (default: none)
      network_id: "*",       // Any network (default: none)
    }, bsc_testnet: {
      provider: () => new HDWalletProvider(mnemonic, `https://data-seed-prebsc-1-s1.binance.org:8545`),
      network_id: 97,
      confirmations: 10,
      timeoutBlocks: 200,
      skipDryRun: true
    }, cronos_testnet: {
      provider: () => new HDWalletProvider(mnemonic, `https://cronos-testnet-3.crypto.org:8545`),
      network_id: 338,
      confirmations: 10,
      timeoutBlocks: 200,
      skipDryRun: true
    }
  },
  // Set default mocha options here, use special reporters, etc.
  mocha: {
    // timeout: 100000
  },
  compilers: {
    solc: {
      version: "0.8.14",      // Fetch exact version from solc-bin (default: truffle's version)
      settings: {          // See the solidity docs for advice about optimization and evmVersion
        optimizer: {
          enabled: true,
          runs: 2000
        },
      }
    }
  },
};
