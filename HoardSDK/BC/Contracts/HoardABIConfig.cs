class HoardABIConfig
{
	public const string AdministrableABI = "[{'constant':true,'inputs':[{'name':'','type':'address'}],'name':'admins','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'owner','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'newOwner','type':'address'}],'name':'transferOwnership','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'anonymous':false,'inputs':[{'indexed':true,'name':'previousOwner','type':'address'},{'indexed':true,'name':'newOwner','type':'address'}],'name':'OwnershipTransferred','type':'event'},{'constant':false,'inputs':[{'name':'_adminAddr','type':'address'}],'name':'addAdmin','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_adminAddr','type':'address'}],'name':'removeAdmin','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'}]";
	public const string ContractReceiverABI = "[{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_value','type':'uint256'},{'name':'_data','type':'bytes'}],'name':'tokenFallback','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'}]";
	public const string ERC165ABI = "[{'constant':true,'inputs':[{'name':'_interfaceId','type':'bytes4'}],'name':'supportsInterface','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'}]";
	public const string ERC223TokenABI = "[{'constant':true,'inputs':[{'name':'_interfaceId','type':'bytes4'}],'name':'supportsInterface','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'InterfaceId_ERC165','outputs':[{'name':'','type':'bytes4'}],'payable':false,'stateMutability':'view','type':'function'},{'inputs':[{'name':'_name','type':'string'},{'name':'_symbol','type':'string'}],'payable':false,'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'name':'from','type':'address'},{'indexed':true,'name':'to','type':'address'},{'indexed':false,'name':'value','type':'uint256'},{'indexed':false,'name':'data','type':'bytes'}],'name':'Transfer','type':'event'},{'constant':true,'inputs':[],'name':'name','outputs':[{'name':'_name','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'symbol','outputs':[{'name':'_symbol','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'decimals','outputs':[{'name':'_decimals','type':'uint8'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'totalSupply','outputs':[{'name':'_totalSupply','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'tokenState','outputs':[{'name':'','type':'bytes32'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'tokenStateType','outputs':[{'name':'','type':'bytes32'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_value','type':'uint256'}],'name':'transfer','outputs':[{'name':'success','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_value','type':'uint256'},{'name':'_data','type':'bytes'}],'name':'transfer','outputs':[{'name':'success','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_value','type':'uint256'},{'name':'_data','type':'bytes'},{'name':'_custom_fallback','type':'string'}],'name':'transfer','outputs':[{'name':'success','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'_owner','type':'address'}],'name':'balanceOf','outputs':[{'name':'balance','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'}]";
	public const string ERC223TokenMockABI = "[{'constant':true,'inputs':[{'name':'_interfaceId','type':'bytes4'}],'name':'supportsInterface','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'name','outputs':[{'name':'_name','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'totalSupply','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'InterfaceId_ERC165','outputs':[{'name':'','type':'bytes4'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'decimals','outputs':[{'name':'','type':'uint8'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_owner','type':'address'}],'name':'balanceOf','outputs':[{'name':'balance','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'symbol','outputs':[{'name':'_symbol','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'tokenStateType','outputs':[{'name':'','type':'bytes32'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_value','type':'uint256'}],'name':'transfer','outputs':[{'name':'success','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_value','type':'uint256'},{'name':'_data','type':'bytes'}],'name':'transfer','outputs':[{'name':'success','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'gameContract','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'tokenState','outputs':[{'name':'','type':'bytes32'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_value','type':'uint256'},{'name':'_data','type':'bytes'},{'name':'_custom_fallback','type':'string'}],'name':'transfer','outputs':[{'name':'success','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'inputs':[{'name':'_totalSupply','type':'uint256'},{'name':'_gameContract','type':'address'},{'name':'_symbol','type':'string'},{'name':'_tokenState','type':'bytes32'}],'payable':false,'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'name':'from','type':'address'},{'indexed':true,'name':'to','type':'address'},{'indexed':false,'name':'value','type':'uint256'},{'indexed':false,'name':'data','type':'bytes'}],'name':'Transfer','type':'event'}]";
	public const string ERC721TokenABI = "[{'constant':true,'inputs':[{'name':'_interfaceId','type':'bytes4'}],'name':'supportsInterface','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_tokenId','type':'uint256'}],'name':'getApproved','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_tokenId','type':'uint256'}],'name':'approve','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'InterfaceId_ERC165','outputs':[{'name':'','type':'bytes4'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_to','type':'address'},{'name':'_tokenId','type':'uint256'}],'name':'transferFrom','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_to','type':'address'},{'name':'_tokenId','type':'uint256'}],'name':'safeTransferFrom','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'_tokenId','type':'uint256'}],'name':'exists','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_tokenId','type':'uint256'}],'name':'ownerOf','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_owner','type':'address'}],'name':'balanceOf','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_approved','type':'bool'}],'name':'setApprovalForAll','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_to','type':'address'},{'name':'_tokenId','type':'uint256'},{'name':'_data','type':'bytes'}],'name':'safeTransferFrom','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'_owner','type':'address'},{'name':'_operator','type':'address'}],'name':'isApprovedForAll','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'inputs':[{'name':'_name','type':'string'},{'name':'_symbol','type':'string'}],'payable':false,'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_from','type':'address'},{'indexed':true,'name':'_to','type':'address'},{'indexed':true,'name':'_tokenId','type':'uint256'}],'name':'Transfer','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_owner','type':'address'},{'indexed':true,'name':'_approved','type':'address'},{'indexed':true,'name':'_tokenId','type':'uint256'}],'name':'Approval','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_owner','type':'address'},{'indexed':true,'name':'_operator','type':'address'},{'indexed':false,'name':'_approved','type':'bool'}],'name':'ApprovalForAll','type':'event'},{'constant':true,'inputs':[],'name':'name','outputs':[{'name':'','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'symbol','outputs':[{'name':'','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'tokenStateType','outputs':[{'name':'','type':'bytes32'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_tokenId','type':'uint256'}],'name':'tokenState','outputs':[{'name':'','type':'bytes32'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_owner','type':'address'},{'name':'_index','type':'uint256'}],'name':'tokenOfOwnerByIndex','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'totalSupply','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_index','type':'uint256'}],'name':'tokenByIndex','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'}]";
	public const string ERC721TokenMockABI = "[{'constant':true,'inputs':[{'name':'_interfaceId','type':'bytes4'}],'name':'supportsInterface','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'name','outputs':[{'name':'','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_tokenId','type':'uint256'}],'name':'getApproved','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_tokenId','type':'uint256'}],'name':'approve','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'totalSupply','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'InterfaceId_ERC165','outputs':[{'name':'','type':'bytes4'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_to','type':'address'},{'name':'_tokenId','type':'uint256'}],'name':'transferFrom','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'_owner','type':'address'},{'name':'_index','type':'uint256'}],'name':'tokenOfOwnerByIndex','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_to','type':'address'},{'name':'_tokenId','type':'uint256'}],'name':'safeTransferFrom','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'_tokenId','type':'uint256'}],'name':'exists','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_index','type':'uint256'}],'name':'tokenByIndex','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_tokenId','type':'uint256'}],'name':'ownerOf','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_owner','type':'address'}],'name':'balanceOf','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'symbol','outputs':[{'name':'','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_tokenId','type':'uint256'}],'name':'tokenState','outputs':[{'name':'','type':'bytes32'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_approved','type':'bool'}],'name':'setApprovalForAll','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'tokenStateType','outputs':[{'name':'','type':'bytes32'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_to','type':'address'},{'name':'_tokenId','type':'uint256'},{'name':'_data','type':'bytes'}],'name':'safeTransferFrom','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'gameContract','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_owner','type':'address'},{'name':'_operator','type':'address'}],'name':'isApprovedForAll','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'inputs':[{'name':'_gameContract','type':'address'}],'payable':false,'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_from','type':'address'},{'indexed':true,'name':'_to','type':'address'},{'indexed':true,'name':'_tokenId','type':'uint256'}],'name':'Transfer','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_owner','type':'address'},{'indexed':true,'name':'_approved','type':'address'},{'indexed':true,'name':'_tokenId','type':'uint256'}],'name':'Approval','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_owner','type':'address'},{'indexed':true,'name':'_operator','type':'address'},{'indexed':false,'name':'_approved','type':'bool'}],'name':'ApprovalForAll','type':'event'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_tokenId','type':'uint256'},{'name':'_tokenState','type':'bytes32'}],'name':'mintToken','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_tokenId','type':'uint256'},{'name':'_state','type':'bytes32'}],'name':'setTokenState','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'}]";
	public const string HoardExchangeABI = "[{'constant':true,'inputs':[{'name':'tokenGet','type':'address'},{'name':'amountGet','type':'uint256'},{'name':'tokenGive','type':'address'},{'name':'tokenId','type':'uint256'},{'name':'expires','type':'uint256'},{'name':'nonce','type':'uint256'},{'name':'user','type':'address'}],'name':'amountFilledERC721','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'tokenGet','type':'address'},{'name':'amountGet','type':'uint256'},{'name':'tokenGive','type':'address'},{'name':'amountGive','type':'uint256'},{'name':'expires','type':'uint256'},{'name':'nonce','type':'uint256'}],'name':'order','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_operator','type':'address'},{'name':'_from','type':'address'},{'name':'_tokenId','type':'uint256'},{'name':'','type':'bytes'}],'name':'onERC721Received','outputs':[{'name':'','type':'bytes4'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'','type':'address'},{'name':'','type':'bytes32'}],'name':'orderFills','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'tokenGet','type':'address'},{'name':'amountGet','type':'uint256'},{'name':'tokenGive','type':'address'},{'name':'amountGive','type':'uint256'},{'name':'expires','type':'uint256'},{'name':'nonce','type':'uint256'},{'name':'user','type':'address'},{'name':'amount','type':'uint256'},{'name':'sender','type':'address'}],'name':'testTrade','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'','type':'address'}],'name':'allowedContacts','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'tokenGet','type':'address'},{'name':'amountGet','type':'uint256'},{'name':'tokenGive','type':'address'},{'name':'amountGive','type':'uint256'},{'name':'expires','type':'uint256'},{'name':'nonce','type':'uint256'},{'name':'user','type':'address'}],'name':'amountFilled','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'tokenGet','type':'address'},{'name':'amountGet','type':'uint256'},{'name':'tokenGive','type':'address'},{'name':'_tokenId','type':'uint256'},{'name':'expires','type':'uint256'},{'name':'nonce','type':'uint256'}],'name':'orderERC721','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'','type':'address'},{'name':'','type':'address'}],'name':'tokens','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'tokenGet','type':'address'},{'name':'amountGet','type':'uint256'},{'name':'tokenGive','type':'address'},{'name':'amountGive','type':'uint256'},{'name':'expires','type':'uint256'},{'name':'nonce','type':'uint256'},{'name':'user','type':'address'}],'name':'availableVolume','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'tokenGet','type':'address'},{'name':'amountGet','type':'uint256'},{'name':'tokenGive','type':'address'},{'name':'amountGive','type':'uint256'},{'name':'expires','type':'uint256'},{'name':'nonce','type':'uint256'}],'name':'cancelOrder','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_token','type':'address'},{'name':'_tokenId','type':'uint256'}],'name':'withdrawTokenERC721','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'tokenGet','type':'address'},{'name':'amountGet','type':'uint256'},{'name':'tokenGive','type':'address'},{'name':'tokenId','type':'uint256'},{'name':'expires','type':'uint256'},{'name':'nonce','type':'uint256'},{'name':'user','type':'address'}],'name':'availableVolumeERC721','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'','type':'address'},{'name':'','type':'uint256'}],'name':'erc721Tokens','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'tokenGet','type':'address'},{'name':'amountGet','type':'uint256'},{'name':'tokenGive','type':'address'},{'name':'tokenId','type':'uint256'},{'name':'expires','type':'uint256'},{'name':'nonce','type':'uint256'},{'name':'user','type':'address'},{'name':'amount','type':'uint256'},{'name':'sender','type':'address'}],'name':'testTradeERC721','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'owner','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'tokenGet','type':'address'},{'name':'amountGet','type':'uint256'},{'name':'tokenGive','type':'address'},{'name':'amountGive','type':'uint256'},{'name':'expires','type':'uint256'},{'name':'nonce','type':'uint256'},{'name':'user','type':'address'},{'name':'amount','type':'uint256'}],'name':'trade','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'contractAddress','type':'address'}],'name':'deny','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_token','type':'address'},{'name':'_value','type':'uint256'}],'name':'withdrawToken','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'tokenGet','type':'address'},{'name':'amountGet','type':'uint256'},{'name':'tokenGive','type':'address'},{'name':'tokenId','type':'uint256'},{'name':'expires','type':'uint256'},{'name':'nonce','type':'uint256'}],'name':'cancelOrderERC721','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'tokenGet','type':'address'},{'name':'amountGet','type':'uint256'},{'name':'tokenGive','type':'address'},{'name':'tokenId','type':'uint256'},{'name':'expires','type':'uint256'},{'name':'nonce','type':'uint256'},{'name':'user','type':'address'},{'name':'amount','type':'uint256'}],'name':'tradeERC721','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'','type':'address'},{'name':'','type':'bytes32'}],'name':'orders','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_value','type':'uint256'},{'name':'','type':'bytes'}],'name':'tokenFallback','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'newOwner','type':'address'}],'name':'transferOwnership','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'contractAddress','type':'address'}],'name':'allow','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'payable':false,'stateMutability':'nonpayable','type':'fallback'},{'anonymous':false,'inputs':[{'indexed':true,'name':'from','type':'address'},{'indexed':true,'name':'token','type':'address'},{'indexed':false,'name':'tokenId','type':'uint256'},{'indexed':false,'name':'value','type':'uint256'}],'name':'Deposit','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'to','type':'address'},{'indexed':true,'name':'token','type':'address'},{'indexed':false,'name':'tokenId','type':'uint256'},{'indexed':false,'name':'value','type':'uint256'}],'name':'Withdraw','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'tokenGet','type':'address'},{'indexed':false,'name':'amountGet','type':'uint256'},{'indexed':true,'name':'tokenGive','type':'address'},{'indexed':false,'name':'tokenId','type':'uint256'},{'indexed':false,'name':'amountGive','type':'uint256'},{'indexed':false,'name':'expires','type':'uint256'},{'indexed':false,'name':'nonce','type':'uint256'},{'indexed':true,'name':'user','type':'address'}],'name':'Order','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'tokenGet','type':'address'},{'indexed':false,'name':'amountGet','type':'uint256'},{'indexed':true,'name':'tokenGive','type':'address'},{'indexed':false,'name':'tokenId','type':'uint256'},{'indexed':false,'name':'amountGive','type':'uint256'},{'indexed':false,'name':'expires','type':'uint256'},{'indexed':false,'name':'nonce','type':'uint256'},{'indexed':true,'name':'user','type':'address'},{'indexed':false,'name':'amount','type':'uint256'},{'indexed':false,'name':'give','type':'uint256'},{'indexed':false,'name':'giveAddress','type':'address'}],'name':'Trade','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'tokenGet','type':'address'},{'indexed':false,'name':'amountGet','type':'uint256'},{'indexed':true,'name':'tokenGive','type':'address'},{'indexed':false,'name':'tokenId','type':'uint256'},{'indexed':false,'name':'amountGive','type':'uint256'},{'indexed':false,'name':'expires','type':'uint256'},{'indexed':false,'name':'nonce','type':'uint256'},{'indexed':true,'name':'user','type':'address'}],'name':'Cancel','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'previousOwner','type':'address'},{'indexed':true,'name':'newOwner','type':'address'}],'name':'OwnershipTransferred','type':'event'}]";
	public const string HoardGameABI = "[{'constant':true,'inputs':[],'name':'name','outputs':[{'name':'','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_adminAddr','type':'address'}],'name':'removeAdmin','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'','type':'uint256'}],'name':'itemContractMap','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'','type':'address'}],'name':'admins','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'','type':'uint64'}],'name':'itemIdsMap','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_adminAddr','type':'address'}],'name':'addAdmin','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'nextItemIndex','outputs':[{'name':'','type':'uint64'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'owner','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'devName','outputs':[{'name':'','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'gameSrvURL','outputs':[{'name':'','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'newOwner','type':'address'}],'name':'transferOwnership','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'inputs':[{'name':'_gameOwner','type':'address'}],'payable':false,'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'name':'gameId','type':'uint256'},{'indexed':false,'name':'itemAddress','type':'address'},{'indexed':false,'name':'itemId','type':'uint256'}],'name':'GameItemAdded','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'previousOwner','type':'address'},{'indexed':true,'name':'newOwner','type':'address'}],'name':'OwnershipTransferred','type':'event'},{'constant':false,'inputs':[{'name':'_gameSrvURL','type':'string'}],'name':'setGameSrvURL','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_gameName','type':'string'},{'name':'_devName','type':'string'}],'name':'setGameName','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'itemId','type':'uint256'}],'name':'itemExists','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'itemId','type':'uint256'},{'name':'itemAddress','type':'address'}],'name':'setGameItemContract','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'itemAddress','type':'address'}],'name':'addGameItemContract','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'','type':'address'},{'name':'','type':'uint256'},{'name':'','type':'bytes'}],'name':'tokenFallback','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'}]";
	public const string HoardGameCenterABI = "[ { 'constant': false, 'inputs': [ { 'name': '_adminAddr', 'type': 'address' } ], 'name': 'removeAdmin', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'address' } ], 'name': 'admins', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'uint64' } ], 'name': 'gameIdsMap', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'exchangeSrvURL', 'outputs': [ { 'name': '', 'type': 'string' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'nextGameIndex', 'outputs': [ { 'name': '', 'type': 'uint64' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_adminAddr', 'type': 'address' } ], 'name': 'addAdmin', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'hoardTokenAddress', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'uint256' } ], 'name': 'gameOwnersMap', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'uint256' } ], 'name': 'gameContractMap', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'owner', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'exchangeAddress', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'newOwner', 'type': 'address' } ], 'name': 'transferOwnership', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'inputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'constructor' }, { 'anonymous': false, 'inputs': [ { 'indexed': true, 'name': 'gameOwner', 'type': 'address' }, { 'indexed': false, 'name': 'gameId', 'type': 'uint256' } ], 'name': 'GameAdded', 'type': 'event' }, { 'anonymous': false, 'inputs': [ { 'indexed': true, 'name': 'gameOwner', 'type': 'address' }, { 'indexed': false, 'name': 'gameId', 'type': 'uint256' } ], 'name': 'GameRemoved', 'type': 'event' }, { 'anonymous': false, 'inputs': [ { 'indexed': true, 'name': 'previousOwner', 'type': 'address' }, { 'indexed': true, 'name': 'newOwner', 'type': 'address' } ], 'name': 'OwnershipTransferred', 'type': 'event' }, { 'constant': true, 'inputs': [ { 'name': 'gameIndex', 'type': 'uint64' } ], 'name': 'getGameIdByIndex', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': 'gameId', 'type': 'uint256' } ], 'name': 'getGameContract', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': 'gameId', 'type': 'uint256' } ], 'name': 'gameExists', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'gameId', 'type': 'uint256' }, { 'name': 'gameAddress', 'type': 'address' } ], 'name': 'setGame', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'gameAddress', 'type': 'address' } ], 'name': 'addGame', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'gameId', 'type': 'uint256' } ], 'name': 'removeGame', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_exchangeAddress', 'type': 'address' } ], 'name': 'setExchangeAddress', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_hoardTokenAddress', 'type': 'address' } ], 'name': 'setHoardTokenAddress', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_exchangeSrvURL', 'type': 'string' } ], 'name': 'setExchangeSrvURL', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' } ]";
	public const string HoardGameItemABI = "[{'constant':true,'inputs':[],'name':'name','outputs':[{'name':'_name','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'symbol','outputs':[{'name':'_symbol','type':'string'}],'payable':false,'stateMutability':'view','type':'function'}]";
	public const string HoardGameMockABI = "[{'constant':true,'inputs':[],'name':'name','outputs':[{'name':'','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_adminAddr','type':'address'}],'name':'removeAdmin','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'','type':'uint256'}],'name':'itemContractMap','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'itemId','type':'uint256'},{'name':'itemAddress','type':'address'}],'name':'setGameItemContract','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'Id','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'','type':'address'}],'name':'admins','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_gameName','type':'string'},{'name':'_devName','type':'string'}],'name':'setGameName','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'','type':'uint64'}],'name':'itemIdsMap','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_adminAddr','type':'address'}],'name':'addAdmin','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'itemId','type':'uint256'}],'name':'itemExists','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'nextItemIndex','outputs':[{'name':'','type':'uint64'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_gameSrvURL','type':'string'}],'name':'setGameSrvURL','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'owner','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'devName','outputs':[{'name':'','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'itemAddress','type':'address'}],'name':'addGameItemContract','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'','type':'address'},{'name':'','type':'uint256'},{'name':'','type':'bytes'}],'name':'tokenFallback','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'gameSrvURL','outputs':[{'name':'','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_gameExchange','type':'address'}],'name':'setGameExchange','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'gameExchange','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'newOwner','type':'address'}],'name':'transferOwnership','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'inputs':[{'name':'_gameOwner','type':'address'},{'name':'_devName','type':'string'}],'payable':false,'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'name':'gameId','type':'uint256'},{'indexed':false,'name':'itemAddress','type':'address'},{'indexed':false,'name':'itemId','type':'uint256'}],'name':'GameItemAdded','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'previousOwner','type':'address'},{'indexed':true,'name':'newOwner','type':'address'}],'name':'OwnershipTransferred','type':'event'}]";
	public const string HoardUtilsABI = "[]";
	public const string SupportsInterfaceWithLookupABI = "[{'constant':true,'inputs':[],'name':'InterfaceId_ERC165','outputs':[{'name':'','type':'bytes4'}],'payable':false,'stateMutability':'view','type':'function'},{'inputs':[],'payable':false,'stateMutability':'nonpayable','type':'constructor'},{'constant':true,'inputs':[{'name':'_interfaceId','type':'bytes4'}],'name':'supportsInterface','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'}]";
}
