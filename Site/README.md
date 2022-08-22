# pre-requisites
- metamask installed in the browser
```
npm install -g truffle
npm install -g ganache-cli
```
For Ethereum Networks outside of local:
- set your metamask account mnemonic to `.secret` file (do not commit it!)
- account loaded with ETH for gas fees


## installation
```
npm install
```

# contract development workflow
## Run a local ethereum network, and deploy your token contract to this local network
- Run the command `truffle develop` (to run a local ethereum network)
    - Run  the command `compile` (to compile your solidity contract files)
    - Run the command `migrate --reset` (to deploy your contract to the locally running ethereum network)
    - Run the command `test` (to deploy your contract to the locally [i.e. withing truffle environemnt] running ethereum network)

- Run the command `truffle migrate --reset --network rinkeby` (to deploy your contract to the rinkeby ethereum network)
- Run the command `truffle migrate --network cronos_testnet` (to deploy your contract to the Cronos test network)
- Run command `truffle migrate -f 5 --to 5 --network cronos_testnet` to deploy only migration file 5
