// SPDX-License-Identifier: MIT


pragma solidity >=0.7.0 <0.9.0;

import "witnet-solidity-bridge/contracts/interfaces/IWitnetRandomness.sol";

contract RandomTest {
    bytes32 public randomness;
    uint256 public latestRandomizingBlock;
    IWitnetRandomness immutable public witnet;

    /// @param _witnetRandomness Address of the WitnetRandomness contract.
    constructor (IWitnetRandomness _witnetRandomness) {
        assert(address(_witnetRandomness) != address(0));
        witnet = _witnetRandomness;
    }

    receive () external payable {}

    function requestRandomness() external payable {
        latestRandomizingBlock = block.number;
        uint _usedFunds = witnet.randomize{ value: msg.value }();
        if (_usedFunds < msg.value) {
            payable(msg.sender).transfer(msg.value - _usedFunds);
        }
    }

    function fetchRandomness() external {
        assert(latestRandomizingBlock > 0);
        randomness = witnet.getRandomnessAfter(latestRandomizingBlock);
    }

    function getRandomNo() external view returns (uint32) {

        uint32 luckyNumber = witnet.random(
            100,
            0,
            latestRandomizingBlock
        );
        return luckyNumber;
    }
}
