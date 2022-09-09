// SPDX-License-Identifier: MIT

pragma solidity ^0.8.7;

import "@openzeppelin/contracts/token/ERC1155/ERC1155.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/Strings.sol";

contract ACNFT is ERC1155, Ownable {

    uint256 public constant NORMAL = 0;
    uint256 public constant RARE= 1;
    uint256 public constant LEGENDARY= 2;

    mapping (uint256 => string) private _uris; // tokenId to uri string

    constructor() public ERC1155("https://bafybeibrzpsbhcsofogalqk2ghwx4qxdhiwdhagijd6stlv6ri2kdxej5e.ipfs.nftstorage.link/") {
        _mint(msg.sender, NORMAL, 100, "");
        _mint(msg.sender, RARE, 100, "");
        _mint(msg.sender, LEGENDARY, 100, "");
    }

    function uri(uint256 tokenId) override public view returns (string memory) {
        return(_uris[tokenId]);
    }

    function setTokenUri(uint256 tokenId, string memory uri) public onlyOwner {
        require(bytes(_uris[tokenId]).length == 0, "Cannot set uri twice");
        _uris[tokenId] = uri;
    }
}