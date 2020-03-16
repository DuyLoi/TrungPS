/**
 * Created by HuyNT2.
 * User:
 * Date: 21/04/2015
 * Time: 5:35 PM
 */

/* HEADER*/
var gScript;
var gCFG_OBJ;
var gTrackingID = "";
var gBookID = "";
var gCurCIF = "";
var gIsServing = false;
//bien check giao dich eform
var gIsServingEform = false;
var gEformLinkEdit = "";
var gEformID = "";
var gCustomerTrackingIDEform = "";
var gIsSearchEform = false;
var gListEformIDFilled = [];
//
//bien check ket thuc phuc vu
var gIsEndServing = false;// chua kich hoat

// bien ckeck tam_nghi/ kich_hoat giaodich
var isPause_Play = false;

var gInterlockStatus = false;
//bien check menu them giao dich
var menuSelectDealShowHide = false;
//bien check nhan ngoai le
var menuAcceptExceptionShowHide = false;
var menuSearchEFormShowHide = false;
//bien check doc qrcode
var isReadingQRCode = false;
//
var exceptionTrackingId = "";

//bien check card no info
var isCardATMNoInfo = false;
var bookCardATMNoInfo = "";
var requestQRCodeContent;

//-----------PDL----------
var isBtnStart = false;
// -----------------------

function sendInfoToAddon(inData) {
	Common.logInfo("tpbankSidebarFireEvent: Sidebar send data to Addon: " + Common.stringFromJSONObj(inData));
	addon.port.emit("tpbankSidebarFireEvent", inData);

}

function sendRequestToAddon(inData) {
	Common.logInfo("tpbankSidebarRequestEvent: Sidebar send request to Addon: " + Common.stringFromJSONObj(inData));
	addon.port.emit("tpbankSidebarRequestEvent", inData);

}

function sendFreqGetStatusToAddon(inData) {
	Common.logInfo("tpbankFreqGetStatusEvent: Sidebar send freq get status to Addon: " + Common.stringFromJSONObj(inData));
	addon.port.emit("tpbankFreqGetStatusEvent", inData);

}

addon.port.on("tpbankAddonFireEvent", function (msg) {

	Common.logInfo("tpbankAddonFireEvent: Sidebar script got the reply from Addon: " + Common.stringFromJSONObj(msg));
	if (msg && msg.FCC_Url && msg.FCC_User) {// && document.getElementById('idUsername')) {
		Common.hideLoading();
		gCFG_OBJ = msg;
		var timerCheckUserID = setInterval(function (e) {
			if (document.getElementById('idUsername')) {
				clearInterval(timerCheckUserID);
				timerCheckUserID = null;
				document.getElementById('idUsername').innerHTML = gCFG_OBJ.FCC_User;
				//caont them versionClient

				RequestSv.getWaitingCustomAction();
				setInterval(function (e) {
					if (!gIsSearchEform)
						RequestSv.getWaitingCustomAction();
				}, gCFG_OBJ.EC_Timer_GetComingCustomer);
			}
		}, 200);
		var timerGetTime = setInterval(function () {
			document.getElementById('datesystem').innerHTML = getDateSystem();
		}, 1000)
	}
});
//caont addEventListener

addon.port.on("tpbankAddonGetConfig", function (msg) {

	Common.logInfo("caont  vao day goi khach hang tiep theo");

	// caont request len server lay ip server 
	RequestSv.getIpServerFromServerPublic();

});

addon.port.on("tpbankAddonResponseEvent", function (msg) {
	Common.logInfo("tpbankAddonResponseEvent: Sidebar got the response from Addon: " + Common.stringFromJSONObj(msg));

	if (msg.funcAction == "GetAutoScript") {
		RequestSv.resqAutoScriptAction(msg);
	}
	else if (msg.funcAction == "GetCommingCustomer") {
		RequestSv.respWaitingCustomAction(msg);
	}
	else if (msg.funcAction == "LockCustomerForServing") {
		RequestSv.respStartServingAction(msg);
	}
	else if (msg.funcAction == "MarkCompleteServing") {
		RequestSv.respCompleteServingAction(msg);
	}
	else if (msg.funcAction == "MarkCompleteServingPre") {
		RequestSv.respCompleteServingPreAction(msg);
	}
	else if (msg.funcAction == "GetFCCInfoTemplate") {
		RequestSv.respFCCInfoTemplate(msg);
	}
	else if (msg.funcAction == "GetTransDetailInfo") {
		RequestSv.resqTransDetailInfo(msg);
	}
	else if (msg.funcAction == "SendReviewTransContent") {
		RequestSv.respFCCTransInfo(msg);
	}
	else if (msg.funcAction == "GetUrlCustSignatureImage") {
		RequestSv.respCustSignatureImage(msg);
	}
	else if (msg.funcAction == "MarkCurrentTransComplete") {
		RequestSv.respWaitingScreenAction(msg);
	}
	else if (msg.funcAction == "RequireResetCustSignature") {
		RequestSv.respResetCustSignatureAction(msg);
	}
	else if (msg.funcAction == "PrintPDFFile") {
		RequestSv.respPrintFileAction(msg);
	}
	else if (msg.funcAction == "UpdateCardSupportInfo") {
		RequestSv.respUpdateAtmCardServiceComplete(msg);
	}
	else if (msg.funcAction == "ExportCardHelpReceipt") {
		RequestSv.exportCardHelpReceiptSuccess(msg);
	}
	else if (msg.funcAction == "PushCustomerInstantly") {
		RequestSv.handleResponseExceptionAcceptSuccessClick(msg);
	} else if (msg.funcAction == "PushCustomerInstantly_ChangeCounter") {
		RequestSv.handleExceptionAcceptClickTwoSuccess(msg);

	} else if (msg.funcAction == "AddMoreService") {
		RequestSv.selectDealMenuSuccess(msg);

	} else if (msg.funcAction == "GetHTMLCardHelpTable") {

		RequestSv.repHTMLCardNoInfoSuccess(msg);

	} else if (msg.funcAction == "OpenCloseQRCodeReader") {
		RequestSv.handleReadQRCodeSucess(msg);

	} else if (msg.funcAction == "GetGreeterMessage") {
		RequestSv.handleReadQRCodeContentSuccess(msg);
	}
	else if (msg.funcAction == "SendUrlToGreeter") {
		RequestSv.respUrlToGreeter(msg);
	}
	else if (msg.funcAction == "requestGetInforFromServer") {
		//caont send request to server get ip server
		RequestSv.getTellerNameFromServerLocal();
	}
	else if (msg.funcAction == "requestGetTellerName") {
		//caont send request to server get teller name
		//Common.hideLoading();
	}
	else if (msg.funcAction == "GetlinkEditEForm") {
		RequestSv.resqOpenLinkEditInfoEForm(msg);
	}
	else if (msg.funcAction == "SearchDataFromEForm") {
		RequestSv.respCustomerTrans(msg);
	}
	else if (msg.funcAction == "exRunAutoscript") {
		Common.showAlert("Tích hợp FCC không thành công. Vui lòng thực hiện lại");
	}
	/* KienNT13 - 2018/11/22 - Thêm Thông tin tặng quà */
	else if (msg.funcAction == "GetGiftInfo") {
		RequestSv.parseDataGift(msg);
	}
	else if (msg.funcAction == "AddOrUpdateGiveGift") {
		RequestSv.giftSuccess(msg);
	}
	/* END KienNT13 - 2018/11/22 - Thêm Thông tin tặng quà */
});

/*
	Common functions
*/
var FCCtimeout;
var Common = {
	logInfo: function (inContent) {
		if (!CONST_DEBUG_MODE) {
			return;
		}
		else {
			if (inContent == undefined) return;
			try {

				console.log((inContent));

			}
			catch (err) {
				console.log('error console.log: ' + err);
			}
		}
	},
	getIframe: function (iframeObj) {
		var doc;
		if (iframeObj.contentWindow) {
			return iframeObj.contentWindow;
		}
		if (iframeObj.window) {
			return iframeObj.window;
		}
		if (!doc && iframeObj.contentDocument) {
			doc = iframeObj.contentDocument;
		}
		if (!doc && iframe_object.document) {
			doc = iframeObj.document;
		}
		if (doc && doc.defaultView) {
			return doc.defaultView;
		}
		if (doc && doc.parentWindow) {
			return doc.parentWindow;
		}
		return undefined;
	},
	stringFromJSONObj: function (inObj) {
		return JSON.stringify(inObj);
	},
	stringToJSONObj: function (inStr) {
		try {
			var tmpObj = JSON.parse(inStr);
			return tmpObj;
		}
		catch (ex) {
			Common.logInfo("--- EXCEPTION: Parser string to JSON object ---");
			Common.logInfo(ex);
			return undefined;
		}
	},
	showLoading: function () {
		Common.logInfo("--- SANGNT1 SHOWLOADING ---");

		if (document.getElementById('loadingMask')) {
			document.getElementById('loadingMask').style.display = 'block';
			FCCtimeout = setTimeout(function (e) {
				clearTimeout(FCCtimeout);
				FCCtimeout = null;
				document.getElementById('loadingMask').style.display = 'none';
			}, 30 * 1000);
		}
	},
	hideLoading: function () {
		Common.logInfo("--- SANGNT1 HIDELOADING ---");
		clearTimeout(FCCtimeout);
		FCCtimeout = null;
		if (document.getElementById('loadingMask')) {
			document.getElementById('loadingMask').style.display = 'none';
		}
	},
	getBaseUrl: function (inUrl) {
		var re = new RegExp(/^.*\//);
		return re.exec(inUrl);
	},
	//disable right-click
	clickIE: function (e) {
		if (document.all) {
			return false;
		}
		if (document.layers || (document.getElementById && !document.all)) {
			if (e.which == 2 || e.which == 3) {
				return false;
			}
		}
	},
	clickNS: function (e) {
		if (document.layers || (document.getElementById && !document.all)) {
			if (e.which == 2 || e.which == 3) {
				return false;
			}
		}
	},
	showAlert: function (inStr) {
		window.alert("ECOUNTER TELLER\n" + inStr);
	},
	showAlertCf: function (inStr, okCallback) {
		window.confirm("ECOUNTER TELLER\n" + inStr, function (e) {
			if (successCallback && (typeof (successCallback) == 'function')) {
				successCallback();
			}
		});
	}
}

/*
	Request service
*/
var serviceObj = {
	funcAction: "",
	reqContent: "",
	respResult: ""
}

var RequestSv = {
	getAutoScript: function () {
		Common.showLoading();
		serviceObj.funcAction = "GetAutoScript";
		serviceObj.reqContent = "<custTrackID>20762</custTrackID><bookID>27</bookID>";
		sendRequestToAddon(serviceObj);
	},
	resqAutoScript: function (outData) {
		Common.hideLoading();
		gScript = Common.stringToJSONObj(outData.respResult);
	},
	//caont add open link EForm
	openLinkEditInfoEForm: function (inAction, inBody) {
		document.getElementById('btnResetCustSignature').disabled = true;
		if (!gIsServing) {
			Common.showAlert("Vui lòng click 'Bắt đầu phục vụ' trước khi thực hiện giao dịch.");
			return;
		}
		//Common.showAlert("Thong tin action" + inAction + " : " + inBody);
		if (gInterlockStatus) {
			return;
		}
		else {
			gInterlockStatus = true;
			setTimeout(function (e) {
				gInterlockStatus = false;
			}, 1000);
		}


		//FormID 
		if (inBody.indexOf("<eformID>") > -1 && inBody.indexOf("</eformID>") > -1) {
			var tmpStrArr = inBody.match("<eformID>(.*)</eformID>");
			gEformID = tmpStrArr[1];

		}
		//link edit form 
		if (inBody.indexOf("<linkEditEform>") > -1 && inBody.indexOf("</linkEditEform>") > -1) {
			var tmpStrArr = inBody.match("<linkEditEform>(.*)</linkEditEform>");
			gEformLinkEdit = tmpStrArr[1];

		}

		//Bookid
		if (inBody.indexOf("<bookID>") > -1 && inBody.indexOf("</bookID>") > -1) {
			var tmpStrArr = inBody.match("<bookID>(.*)</bookID>");
			gBookID = tmpStrArr[1];
		}

		//CustomerTrackingID
		if (inBody.indexOf("<custTrackID>") > -1 && inBody.indexOf("</custTrackID>") > -1) {
			var tmpStrArr = inBody.match("<custTrackID>(.*)</custTrackID>");
			gCustomerTrackingIDEform = tmpStrArr[1];
		}
		//customerCIF
		if (inBody.indexOf("<customerCIF>") > -1 && inBody.indexOf("</customerCIF>") > -1) {
			var tmpStrArr = inBody.match("<customerCIF>(.*)</customerCIF>");
			gCurCIF = tmpStrArr[1];
		}

		//doi mau loai giao dich
		for (var i = 0; i < document.getElementsByTagName('tr').length; i++) {
			document.getElementsByTagName('tr')[i].style.backgroundColor = '#471C51';
		}
		var tmpTrID = "tr_cust_track" + gTrackingID + "_book" + gBookID;
		document.getElementById(tmpTrID).style.backgroundColor = '#93b8c9';
		document.getElementById("idTransDetailInfo").style.display = 'none';

		// show button fill fcc
		gIsServingEform = true;

		document.getElementById('btnFillFCC').disabled = false;
		document.getElementById('btnResetCustSignature').disabled = true;
		document.getElementById('btnDetailTran').disabled = false;



		Common.showLoading();
		serviceObj.funcAction = inAction;
		//serviceObj.reqContent = inBody;
		serviceObj.reqContent = gEformLinkEdit;
		sendRequestToAddon(serviceObj);

	}
	,
	resqOpenLinkEditInfoEForm: function (outData) {

		Common.hideLoading();

	},

	openLinkInfoEForm: function (inAction, inBody) {
		//FormID 
		if (inBody.indexOf("<eformID>") > -1 && inBody.indexOf("</eformID>") > -1) {
			var tmpStrArr = inBody.match("<eformID>(.*)</eformID>");
			gEformID = tmpStrArr[1];

		}
		//link edit form 
		if (inBody.indexOf("<linkEditEform>") > -1 && inBody.indexOf("</linkEditEform>") > -1) {
			var tmpStrArr = inBody.match("<linkEditEform>(.*)</linkEditEform>");
			gEformLinkEdit = tmpStrArr[1];

		}

		//doi mau loai giao dich
		var tmpTrID = "tr_cust_track" + gEformID;
		document.getElementById(tmpTrID).style.backgroundColor = '#93b8c9';
		document.getElementById("idTransDetailInfo").style.display = 'none';
		document.getElementById('btnDetailTran').disabled = false;
		document.getElementById('btnFillFCC').disabled = true;
		document.getElementById('btnResetCustSignature').disabled = true;
	},
	handleDetailTranClick: function () {
		if (gEformLinkEdit == "") {
			Common.showAlert("Vui lòng click chọn giao dịch trước khi thực hiện.");
			return;
		}
		Common.showLoading();
		serviceObj.funcAction = 'GetlinkEditEForm';
		//serviceObj.reqContent = inBody;
		serviceObj.reqContent = gEformLinkEdit;
		sendRequestToAddon(serviceObj);
	},
	//caont run auto script eform
	/*resqHandleAutoScriptEForm: function(outData) {
		
		Common.hideLoading();
		gScript = Common.stringToJSONObj(outData.respResult);
		
		var tmpRunAutoScript = new AutoScriptFunc();
		tmpRunAutoScript.idxStep = 0;
		tmpRunAutoScript.runAutoScript(gScript);
		
	},*/

	getAutoScriptAction: function (inAction, inBody) {
		if (!gIsServing) {
			Common.showAlert("Vui lòng click 'Bắt đầu phục vụ' trước khi thực hiện giao dịch.");
			return;
		}
		if (gInterlockStatus) {
			return;
		}
		else {
			gInterlockStatus = true;
			setTimeout(function (e) {
				gInterlockStatus = false;
			}, 1000);
		}

		Common.showLoading();
		serviceObj.funcAction = inAction;
		serviceObj.reqContent = inBody;
		sendRequestToAddon(serviceObj);
		if (inBody.indexOf("<bookID>") > -1 && inBody.indexOf("</bookID>") > -1) {
			var tmpStrArr = inBody.match("<bookID>(.*)</bookID>");
			gBookID = tmpStrArr[1];
		}
		//customerCIF
		if (inBody.indexOf("<customerCIF>") > -1 && inBody.indexOf("</customerCIF>") > -1) {
			var tmpStrArr = inBody.match("<customerCIF>(.*)</customerCIF>");
			gCurCIF = tmpStrArr[1];
		}
	},
	resqAutoScriptAction: function (outData) {
		Common.hideLoading();
		gScript = Common.stringToJSONObj(outData.respResult);
		// Fix
		if (!!gScript && gScript.length > 0 && !!gScript[0].code) {
			Common.showAlert(gScript[0].content);
			return;
		}
		// END
		var tmpRunAutoScript = new AutoScriptFunc();
		tmpRunAutoScript.idxStep = 0;
		tmpRunAutoScript.runAutoScript(gScript);
	},
	getTransDetailInfo: function (inAction, inBody) {
		if (!gIsServing) {
			Common.showAlert("Vui lòng click 'Bắt đầu phục vụ' trước khi thực hiện giao dịch.");
			return;
		}
		if (gInterlockStatus) {
			return;
		}
		else {
			gInterlockStatus = true;
			setTimeout(function (e) {
				gInterlockStatus = false;
			}, 1000);
		}

		Common.showLoading();
		serviceObj.funcAction = inAction;
		serviceObj.reqContent = inBody;
		sendRequestToAddon(serviceObj);
		if (inBody.indexOf("<bookID>") > -1 && inBody.indexOf("</bookID>") > -1) {
			var tmpStrArr = inBody.match("<bookID>(.*)</bookID>");
			gBookID = tmpStrArr[1];
		}
		//customerCIF
		if (inBody.indexOf("<customerCIF>") > -1 && inBody.indexOf("</customerCIF>") > -1) {
			var tmpStrArr = inBody.match("<customerCIF>(.*)</customerCIF>");
			gCurCIF = tmpStrArr[1];
		}
	},
	resqTransDetailInfo: function (outData) {
		Common.hideLoading();
		document.getElementById("idTransDetailInfo").innerHTML = outData.respResult;
		document.getElementById("idTransDetailInfo").style.display = '';
	},
	getWaitingCustomAction: function () {
		if (gIsServing) return;
		serviceObj.funcAction = "GetCommingCustomer";
		serviceObj.reqContent = "<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername>";
		sendRequestToAddon(serviceObj);
	},
	respWaitingCustomAction: function (outData) {
		Common.logInfo("HuyNT2: comming user data: " + Common.stringFromJSONObj(outData));
		var tmpStartServingCmd = "";
		if (outData && outData.respResult && document.getElementById("sidebarContent")) {
			document.getElementById("sidebarContent").innerHTML = outData.respResult;
			if (document.getElementById("txtParamsStartServing")) {
				tmpStartServingCmd = document.getElementById("txtParamsStartServing").value;
			}
		}
		if (tmpStartServingCmd.indexOf("<currentCustTrackingID>") > -1 && tmpStartServingCmd.indexOf("</currentCustTrackingID>") > -1) {
			var tmpStrArr = tmpStartServingCmd.match("<currentCustTrackingID>(.*)</currentCustTrackingID>");
			gTrackingID = tmpStrArr[1];
			Common.logInfo("HuyNT2: TrackingID = " + gTrackingID);
		}
		if (gTrackingID && gTrackingID.length > 1) {
			showButtonStatus();
		}
	},
	setStartServingActionPopup: function () {
		document.getElementById("bgExceptionStart").style.display = 'block';
	},
	setStartServingAction: function () {
		isBtnStart = true;
		document.getElementById("bgExceptionStart").style.display = 'none';
		if (document.getElementById("txtParamsStartServing")) {
			Common.showLoading();
			/* KienNT13 - 2018/11/22 - Thêm Thông tin tặng quà */
			if (!document.getElementById("btnGiveGift")) {
				gGiveGift = true;
			}
			else {
				gGiveGift = false;
				document.getElementById("btnGiveGift").style.display = 'block';
			}
			/* END KienNT13 - 2018/11/22 - Thêm Thông tin tặng quà */
			serviceObj.funcAction = "LockCustomerForServing";
			var tmpCmd = document.getElementById("txtParamsStartServing").value;
			tmpCmd = tmpCmd.replace(gCFG_OBJ.EC_Username_Key, document.getElementById('idUsername').innerHTML);
			serviceObj.reqContent = tmpCmd;
			sendRequestToAddon(serviceObj);
			gIsServing = true;
			showButtonStatus();
		}
	},
	respStartServingAction: function (outData) {
		Common.hideLoading();
		Common.logInfo("HuyNT2: start serving customer: " + Common.stringFromJSONObj(outData));
		document.getElementById("sendUrlToCust").style.display = "";
	},
	// kết thúc phục vụ-----------------------------------
	setCompleteServingAction: function () {
		/* KienNT13 - 2018/11/22 - Thêm Thông tin tặng quà */
		if (!gGiveGift) {
			Common.showAlert("Chưa nhập thông tin quà tặng");
			return;
		}
		//remove data khi ket thuc
		var listTable = document.getElementsByTagName("table");
		listTable[1].style.display = "none";
		var tableCustomerInfo = listTable[0].getElementsByTagName('tr');
		for (var item of tableCustomerInfo) {
			var td = item.getElementsByTagName('td');
			td[0].innerHTML = '';
		}

		/* END KienNT13 - 2018/11/22 - Thêm Thông tin tặng quà */
		//caont add
		document.getElementById('btnFillFCC').disabled = true;
		document.getElementById('btnDetailTran').disabled = true;

		document.getElementById("table_card_atm_noinfo").innerHTML = "";
		document.getElementById('menu_select_deal').style.display = 'none';
		document.getElementById("sendUrlToCust").style.display = "none";
		gListEformIDFilled = [];
		menuSelectDealShowHide = false;
		bookCardATMNoInfo = "";
		isCardATMNoInfo = false;
		if (gIsServingEform) {
			var listRow = document.getElementById('tbBookingList').getElementsByTagName('tr');
			var list_formid = "";
			for (var i = 0; i < listRow.length; i++) {
				if (i == (listRow.length - 1))
					list_formid += listRow[i].getAttribute('eformid');
				else
					list_formid += listRow[i].getAttribute('eformid') + "|";
			}
			serviceObj.funcAction = "UpdateListStatusEForm";
			serviceObj.reqContent = "<listFormID>" + list_formid + "</listFormID><userRequest>" + document.getElementById('idUsername').innerHTML + "</userRequest>";
			sendRequestToAddon(serviceObj);
		}
		// kiem tra ket thuc: gIsEndServing=false=> !gIsEndServing= true=>if (!gIsEndServing) =true
		// kiểm tra với gIsEndServing =false
		// if (!gIsEndServing) {
		// 	document.getElementById('btnShowWaitingScreen').disabled = false;
		// 	serviceObj.funcAction = "MarkCompleteServingPre";
		// 	serviceObj.reqContent = "<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername><currentCustTrackID>" + gTrackingID + "</currentCustTrackID>";
		// 	sendRequestToAddon(serviceObj);

		// } 
		// kiểm tra với gIsEndServing =true
		// else {
		// 	RequestSv.getWaitingCustomAction();

		// 	Common.showLoading();
		// 	serviceObj.funcAction = "MarkCompleteServing";
		// 	serviceObj.reqContent = "<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername><currentCustTrackID>" + gTrackingID + "</currentCustTrackID>";
		// 	sendRequestToAddon(serviceObj);
		// 	gTrackingID = "";
		// 	gBookID = "";
		// 	gCurCIF = "";
		// 	gIsServing = false;
		// 	gIsEndServing = false;
		// 	gIsServingEform = false;
		// 	gIsSearchEform = false;
		// 	document.getElementById('btnAcceptException').disabled = true;
		// 	document.getElementById("btnCompleteServing").innerHTML = 'Kết thúc phục vụ';
		// 	document.getElementById("idTransDetailInfo").style.display = 'none';
		// 	document.getElementById('btnCompleteServing').disabled = true;
		// 	document.getElementById('btnSendCustInfo').disabled = true;
		//  	document.getElementById('btnShowWaitingScreen').disabled = true;

		// 	document.getElementById('btnResetCustSignature').disabled = true;
		// 	document.getElementById('btnPrintFile').disabled = true;
		// 	document.getElementById('btnSelectDeal').disabled = true;
		// 	document.getElementById('btnStartServing').disabled = false;

		// 	setTimeout(function (e) {
		// 		showButtonStatus();
		// 	}, 2500);

		// }

		//  bấm -> gọi khách hàng tiếp theo
		if (gIsEndServing) {
			RequestSv.getWaitingCustomAction();

			Common.showLoading();
			serviceObj.funcAction = "MarkCompleteServing";
			serviceObj.reqContent = "<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername><currentCustTrackID>" + gTrackingID + "</currentCustTrackID>";
			sendRequestToAddon(serviceObj);
			gTrackingID = "";
			gBookID = "";
			gCurCIF = "";
			gIsServing = false;
			gIsEndServing = false;
			gIsServingEform = false;
			gIsSearchEform = false;
			document.getElementById('btnAcceptException').disabled = true;
			document.getElementById("btnCompleteServing").innerHTML = 'Kết thúc phục vụ';
			document.getElementById("idTransDetailInfo").style.display = 'none';
			document.getElementById('btnCompleteServing').disabled = true;
			document.getElementById('btnSendCustInfo').disabled = true;
			document.getElementById('btnShowWaitingScreen').disabled = true;

			document.getElementById('btnResetCustSignature').disabled = true;
			document.getElementById('btnPrintFile').disabled = true;
			document.getElementById('btnSelectDeal').disabled = true;
			document.getElementById('btnStartServing').disabled = false;

			setTimeout(function (e) {
				showButtonStatus();
			}, 2500);
			// pdl-------------
			isBtnStart = false;
			id = setInterval(frame, 10000);
			// ----------------
		} else {
			document.getElementById('btnShowWaitingScreen').disabled = false;
			serviceObj.funcAction = "MarkCompleteServingPre";
			serviceObj.reqContent = "<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername><currentCustTrackID>" + gTrackingID + "</currentCustTrackID>";
			sendRequestToAddon(serviceObj);
		}
	},
	// pdl--------------tạm nghỉ
	setPauseServingAction: function () {
		if (document.getElementById('btnShowWaitingScreen').innerHTML === 'Quầy tạm nghỉ') {
			document.getElementById('btnShowWaitingScreen').innerHTML = 'Kích hoạt quầy';
			isPause_Play = true;

		} else {
			document.getElementById('btnShowWaitingScreen').innerHTML = 'Quầy tạm nghỉ';
			isPause_Play = false;
		}
		showButtonStatusbtn_Pause_play();

	},
	// =========================

	respCompleteServingAction: function (outData) {
		Common.hideLoading();
		Common.logInfo("HuyNT2: start serving customer: " + Common.stringFromJSONObj(outData));
	},
	respCompleteServingPreAction: function (outData) {
		Common.hideLoading();
		document.getElementById("btnCompleteServing").innerHTML = 'Gọi KH tiếp theo';
		gIsEndServing = true;
		showButtonStatus();
		Common.logInfo("HuyNT2: start serving customer: " + Common.stringFromJSONObj(outData));
	},
	getFCCInfoTemplate: function () {
		if (gBookID && gBookID.length > 0) {
			Common.showLoading();
			serviceObj.funcAction = "GetFCCInfoTemplate";
			serviceObj.reqContent = "<custTrackingID>" + gTrackingID + "</custTrackingID><custBookID>" + gBookID + "</custBookID>";
			sendRequestToAddon(serviceObj);
		}
	},
	respFCCInfoTemplate: function (outData) {
		Common.logInfo("HuyNT2 abc");
		Common.hideLoading();
		Common.logInfo("HuyNT2: get template FCC info: " + Common.stringFromJSONObj(outData));
		sendFCCInfoToServer(outData.respResult);
	},
	sendFCCTransInfo: function (inFccInfoTemp) {
		Common.logInfo("HuyNT2 abc");
		if (gBookID && gBookID.length > 0) {
			Common.showLoading();
			serviceObj.funcAction = "SendReviewTransContent";
			serviceObj.reqContent = "<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername><reviewContent>" + inFccInfoTemp + "</reviewContent><custTrackingID>" + gTrackingID + "</custTrackingID><custBookID>" + gBookID + "</custBookID>";
			sendRequestToAddon(serviceObj);
		}
	},
	respFCCTransInfo: function (outData) {
		Common.hideLoading();
		Common.logInfo("HuyNT2: sent FCC info to Server: " + Common.stringFromJSONObj(outData));
		//Hien thi chu ky
		showCustSignatureImg();
	},
	getCustSignatureImage: function () {
		if (gBookID && gBookID.length > 0) {
			Common.showLoading();
			serviceObj.funcAction = "GetUrlCustSignatureImage";
			serviceObj.reqContent = "<custTrackingID>" + gTrackingID + "</custTrackingID><custBookID>" + gBookID + "</custBookID>";
			//serviceObj.reqContent ="<custTrackingID>" + "20789" + "</custTrackingID><custBookID>" + "32" + "</custBookID>";
			sendRequestToAddon(serviceObj);
		}
	},
	respCustSignatureImage: function (outData) {
		Common.hideLoading();
		Common.logInfo("HuyNT2: get signature from IPad info: " + Common.stringFromJSONObj(outData));

	},
	showWaitingScreenAction: function () {
		Common.showLoading();

		//caont add
		document.getElementById('btnFillFCC').disabled = true;

		if (gIsServingEform) {
			gListEformIDFilled.push(gEformID);
			serviceObj.funcAction = "UpdateStatusEForm";
			serviceObj.reqContent = "<formID>" + gEformID + "</formID><formStatus>Y</formStatus><userRequest>" + document.getElementById('idUsername').innerHTML + "</userRequest>";
			sendRequestToAddon(serviceObj);
		}


		if (isCardATMNoInfo && bookCardATMNoInfo == gBookID) {
			document.getElementById("table_card_atm_noinfo").innerHTML = "";
			bookCardATMNoInfo = "";
			isCardATMNoInfo = false;
		}
		serviceObj.funcAction = "MarkCurrentTransComplete";
		serviceObj.reqContent = "<counterUserName>" + document.getElementById('idUsername').innerHTML + "</counterUserName><custTrackingID>" + gTrackingID + "</custTrackingID><bookID>" + gBookID + "</bookID>";

		if (gBookID == null || gBookID.length == 0) {
			Common.showAlert("Vui lòng chọn giao dịch của khách hàng trước.");
			return;
		}
		var tmpTrID = "tr_cust_track" + gTrackingID + "_book" + gBookID;
		document.getElementById(tmpTrID).style.backgroundColor = '#999999';
		document.getElementById("idTransDetailInfo").style.display = 'none';
		sendRequestToAddon(serviceObj);

	},
	respWaitingScreenAction: function (outData) {
		Common.hideLoading();
		Common.logInfo("HuyNT2: start waiting screen: " + Common.stringFromJSONObj(outData));
	},
	setResetCustSignatureAction: function () {
		Common.showLoading();
		serviceObj.funcAction = "RequireResetCustSignature";
		serviceObj.reqContent = "<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername><custTrackingID>" + gTrackingID + "</custTrackingID><bookID>" + gBookID + "</bookID>";
		sendRequestToAddon(serviceObj);
	},
	respResetCustSignatureAction: function (outData) {
		Common.hideLoading();
		Common.logInfo("HuyNT2: reset customer signature: " + Common.stringFromJSONObj(outData));
	},
	getPrintFileAction: function () {
		if (gBookID && gBookID.length > 0) {
			Common.showLoading();
			serviceObj.funcAction = "PrintPDFFile";
			serviceObj.reqContent = "<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername><custTrackingID>" + gTrackingID + "</custTrackingID><bookID>" + gBookID + "</bookID>";
			sendRequestToAddon(serviceObj);
		}
	},
	respPrintFileAction: function (outData) {
		Common.hideLoading();
		Common.logInfo("HuyNT2: print file: " + Common.stringFromJSONObj(outData));
	},
	//Update Card Help.
	respUpdateAtmCardService: function () {
		Common.showLoading();
		serviceObj.funcAction = "UpdateCardSupportInfo";
		serviceObj.reqContent = "<custTrackingID>" + gTrackingID + "</custTrackingID><bookID>" + gBookID + "</bookID>" + getDataOfATMCardService();
		sendRequestToAddon(serviceObj);
	},
	respUpdateAtmCardServiceComplete: function () {

		setTimeout(function (e) {
			Common.hideLoading();
		}, 3000);

	},
	//print Card help.
	exportCardHelpReceipt: function () {

		Common.showLoading();
		serviceObj.funcAction = "ExportCardHelpReceipt";
		serviceObj.reqContent = "<custTrackingID>" + gTrackingID + "</custTrackingID><bookID>" + gBookID + "</bookID>";
		sendRequestToAddon(serviceObj);
	},
	exportCardHelpReceiptSuccess: function (outData) {
		setTimeout(function (e) {
			Common.hideLoading();
		}, 2000);

	},
	selectDealMenu: function (indexMenu) {

		Common.showLoading();
		serviceObj.funcAction = "AddMoreService";
		serviceObj.reqContent = "<customerTrackingID>" + gTrackingID + "</customerTrackingID><serviceCode>" + indexMenu + "</serviceCode>";
		sendRequestToAddon(serviceObj);
		showhideMenuSelectDeal();

	},
	selectDealMenuSuccess: function (outData) {
		Common.logInfo("SANGNT1: selectDealMenuSuccess " + Common.stringFromJSONObj(outData));
		Common.hideLoading();
		serviceObj.funcAction = "GetCommingCustomer";
		serviceObj.reqContent = "<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername>";
		sendRequestToAddon(serviceObj);

	},
	handleExceptionClick: function () {
		document.getElementById("txtInputCMNDException").value = "";
		document.getElementById("txtInputCIFException").value = "";
		document.getElementById("txtInputCMNDException").disabled = false;
		document.getElementById("txtInputCIFException").disabled = false;
		document.getElementById("validatePopupException").innerHTML = "";
		document.getElementById("btnOKDealException").style.display = 'block';
		document.getElementById("btnDestroyException").style.display = 'none';
		document.getElementById("btnOKDealExceptionTwo").style.display = 'none';
		if (!menuAcceptExceptionShowHide) {
			document.getElementById("bgException").style.display = 'block';
			menuAcceptExceptionShowHide = true;
		} else {
			document.getElementById("bgException").style.display = 'none';
			menuAcceptExceptionShowHide = false;
		}
		document.getElementById("btnReadQRCode").innerHTML = "Đọc QRCode";
		isReadingQRCode = false;
		clearInterval(requestQRCodeContent);

	},

	handleExceptionAcceptClick: function () {
		Common.showLoading();
		debugger;
		//gIsServing = true;
		var userControl = document.getElementById('idUsername').innerHTML;
		var CMND = document.getElementById("txtInputCMNDException").value;
		var CIF = document.getElementById("txtInputCIFException").value;
		if ((CMND == undefined || CMND.length == 0) && (CIF == undefined || CIF.length == 0)) {
			document.getElementById("validatePopupException").innerHTML = "Vui lòng nhập đầy đủ thông tin";
			document.getElementById("validatePopupException").style.display = 'block';
			Common.hideLoading();
		} else {
			if ((CMND != undefined && CMND.length != 0)) {
				for (i = 0; i < CMND.length; i++) {
					if (!IsNumeric(CMND[i])) {
						document.getElementById("validatePopupException").innerHTML = "Vui lòng chỉ nhập số";
						document.getElementById("validatePopupException").style.display = 'block';
						Common.hideLoading();
						return;

					}
				}
				document.getElementById("validatePopupException").style.display = 'none';
				serviceObj.funcAction = "PushCustomerInstantly";
				serviceObj.reqContent = "<code>" + CMND + "</code><codeType>" + "CM" + "</codeType><counterUsername>" + userControl + "</counterUsername>";
				sendRequestToAddon(serviceObj);
				Common.logInfo("SANGNT1: cothongtin:CMND " + CMND + "***" + CIF);
			} else if ((CIF != undefined && CIF.length != 0)) {

				for (i = 0; i < CIF.length; i++) {
					if (!IsNumeric(CIF[i])) {
						document.getElementById("validatePopupException").innerHTML = "Vui lòng chỉ nhập số";
						document.getElementById("validatePopupException").style.display = 'block';
						Common.logInfo("SANGNT1: Nhap so " + CMND + "***" + CIF);
						Common.hideLoading();
						return;

					}
				}
				document.getElementById("validatePopupException").style.display = 'none';
				serviceObj.funcAction = "PushCustomerInstantly";
				serviceObj.reqContent = "<code>" + CIF + "</code><codeType>" + "CI" + "</codeType><counterUsername>" + userControl + "</counterUsername>";
				sendRequestToAddon(serviceObj);
				Common.logInfo("SANGNT1: cothongtin:CIF " + CMND + "***" + CIF);
			}
		}

	},
	handleResponseExceptionAcceptSuccessClick: function (outData) {
		Common.hideLoading();
		//gIsServing = true;
		var reqCode = outData.respResult.match("<RESP_CODE>(.*)</RESP_CODE>");
		var reqTContent = outData.respResult.match("<RESP_CONTENT>(.*)</RESP_CONTENT>");
		var trackingID = outData.respResult.match("<CUSTOMER_TRACKING_ID>(.*)</CUSTOMER_TRACKING_ID>");
		Common.logInfo("SANGNT1: handleResponseExceptionAcceptSuccessClick " + reqCode[1] + "*****" + reqTContent[1] + "*******" + trackingID[1]);
		if (reqCode[1] == "2010") {
			document.getElementById("validatePopupException").innerHTML = reqTContent[1];
			document.getElementById("validatePopupException").style.display = 'block';
			document.getElementById("btnOKDealException").style.display = 'none';
			document.getElementById("btnDestroyException").style.display = 'block';
			document.getElementById("btnOKDealExceptionTwo").style.display = 'block';

			exceptionTrackingId = trackingID[1];

		} else {
			if (reqCode[1] == "00") {
				document.getElementById("validatePopupException").innerHTML = reqTContent[1];
				document.getElementById("validatePopupException").style.display = 'block';
				serviceObj.funcAction = "GetCommingCustomer";
				serviceObj.reqContent = "<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername>";
				sendRequestToAddon(serviceObj);
				gIsEndServing = false;
				gIsServing = true;
				document.getElementById("btnCompleteServing").innerHTML = 'Kết thúc phục vụ';
				showButtonStatus();
			} else {
				document.getElementById("validatePopupException").innerHTML = reqTContent[1];
				document.getElementById("validatePopupException").style.display = 'block';
			}
		}

	},

	handleSearchEFormClick: function () {
		document.getElementById("txtInputTransID").value = "";
		document.getElementById("txtInputStatus").value = "P";
		document.getElementById('txtInputStatus').getElementsByTagName('option')[1].selected = 'selected'
		document.getElementById("txtInputCMNDCus").value = "";
		document.getElementById("txtInputFromDate").value = "";
		document.getElementById("txtInputToDate").value = "";
		document.getElementById("txtInputQRCode").value = "";
		document.getElementById("txtInputPhoneNo").value = "";
		if (!menuSearchEFormShowHide) {
			document.getElementById("popupSearchEForm").style.display = 'block';
			menuSearchEFormShowHide = true;
		} else {
			document.getElementById("popupSearchEForm").style.display = 'none';
			menuSearchEFormShowHide = false;
		}
	},
	handleSearchTransEForm: function () {
		debugger;
		console.log("handleSearchTransEForm");
		Common.showLoading();
		//gIsServing = true;
		var qrcode1 = document.getElementById("txtInputTransID").value;
		var status = document.getElementById("txtInputStatus").value;
		var cmndCus = document.getElementById("txtInputCMNDCus").value;
		var fromdate = document.getElementById("txtInputFromDate").value;
		var todate = document.getElementById("txtInputToDate").value;
		var qrcode2 = document.getElementById("txtInputQRCode").value;
		var phoneNo = document.getElementById("txtInputPhoneNo").value;
		var transID = "";
		var formtype = "";
		if (qrcode1 == "" && cmndCus == "" && qrcode2 == "" && phoneNo == "") {
			document.getElementById("validateFormSearch").innerHTML = "Vui lòng nhập Số CMND/CCCD/HC của KH hoặc Mã số đơn hoặc Mã QR code hoặc SĐT để tìm kiếm";
			document.getElementById("validateFormSearch").style.display = 'block';
			Common.hideLoading();
			return;
		}
		if (!validateInPutOnlyNumber(cmndCus) && cmndCus != "") {
			document.getElementById("validateFormSearch").innerHTML = "Số CMT vui lòng chỉ nhập số";
			document.getElementById("validateFormSearch").style.display = 'block';
			Common.hideLoading();
			return;
		}
		if (!validateInPutOnlyNumber(phoneNo) && phoneNo != "") {
			document.getElementById("validateFormSearch").innerHTML = "Số điện thoại vui lòng chỉ nhập số";
			document.getElementById("validateFormSearch").style.display = 'block';
			Common.hideLoading();
			return;
		}
		if (fromdate != "" && !checkFormatDate(fromdate)) {
			document.getElementById("validateFormSearch").innerHTML = "Ngày bắt đầu sai định dạng";
			document.getElementById("validateFormSearch").style.display = 'block';
			Common.hideLoading();
			return;
		}
		if (todate != "" && !checkFormatDate(todate)) {
			document.getElementById("validateFormSearch").innerHTML = "Ngày kết thúc sai định dạng";
			document.getElementById("validateFormSearch").style.display = 'block';
			Common.hideLoading();
			return;
		}
		if (qrcode1 != "") {
			if (qrcode1.length != 14) {
				document.getElementById("validateFormSearch").innerHTML = "Mã số đơn sai định dạng";
				document.getElementById("validateFormSearch").style.display = 'block';
				Common.hideLoading();
				return;
			}
			formtype = qrcode1.substring(0, 5);
			transID = qrcode1.substring(5, qrcode1.length);
		} if (qrcode2 != "") {
			if (qrcode2.length != 14) {
				document.getElementById("validateFormSearch").innerHTML = "Mã QRCode sai định dạng";
				document.getElementById("validateFormSearch").style.display = 'block';
				Common.hideLoading();
				return;
			}
			formtype = qrcode2.substring(0, 5);
			transID = qrcode2.substring(5, qrcode2.length);
		}
		gIsSearchEform = true;
		document.getElementById("validateFormSearch").style.display = 'none';
		serviceObj.funcAction = "SearchDataFromEForm";
		serviceObj.reqContent = "<formID>" + transID + "</formID>" +
			"<formType>" + formtype.toUpperCase() + "</formType>" +
			"<formStatus>" + status + "</formStatus>" +
			"<customerIndentify>" + cmndCus + "</customerIndentify>" +
			"<startdate>" + fromdate + "</startdate>" +
			"<enddate>" + todate + "</enddate>" +
			"<phoneNbr>" + phoneNo + "</phoneNbr>" +
			"<userRequest>ADDON</userRequest>" +
			"<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername>";
		sendRequestToAddon(serviceObj);
	},
	respCustomerTrans: function (outData) {
		debugger;
		Common.hideLoading();
		Common.logInfo("HuyNT2: comming user data: " + Common.stringFromJSONObj(outData));
		var tmpStartServingCmd = "";
		var reqCode = outData.respResult.match("<RESP_CODE>(.*)</RESP_CODE>");
		var reqTContent = outData.respResult.match("<MESSAGE_CONTENT>(.*)</MESSAGE_CONTENT>");
		if (reqCode[1] == "00") {
			document.getElementById("popupSearchEForm").style.display = 'none';
			menuSearchEFormShowHide = false;
			if (outData && outData.respResult && document.getElementById("sidebarContent")) {
				document.getElementById("sidebarContent").innerHTML = reqTContent[1];
				console.log(reqTContent[1]);
				if (document.getElementById("txtParamsStartServing")) {
					tmpStartServingCmd = document.getElementById("txtParamsStartServing").value;
				}
			}
			if (tmpStartServingCmd.indexOf("<currentCustTrackingID>") > -1 && tmpStartServingCmd.indexOf("</currentCustTrackingID>") > -1) {
				var tmpStrArr = tmpStartServingCmd.match("<currentCustTrackingID>(.*)</currentCustTrackingID>");
				gTrackingID = tmpStrArr[1];
				Common.logInfo("HuyNT2: TrackingID = " + gTrackingID);
			}
			if (gTrackingID && gTrackingID.length > 1) {
				showButtonStatus();
			}
			gIsSearchEform = true;
			document.getElementById('btnDetailTran').disabled = true;
			document.getElementById('btnFillFCC').disabled = true;
			document.getElementById('btnResetCustSignature').disabled = true;
			document.getElementById("btnCompleteServing").innerHTML = 'Kết thúc phục vụ';
		} else {
			gIsSearchEform = false;
			document.getElementById("validateFormSearch").innerHTML = reqTContent[1];
			document.getElementById("validateFormSearch").style.display = 'block';
		}
	},

	getReadQRCodeContent: function () {

		serviceObj.funcAction = "GetGreeterMessage";
		serviceObj.reqContent = "<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername>";
		sendRequestToAddon(serviceObj);

	},
	handleReadQRCodeContentSuccess: function (outData) {
		var messageID = outData.respResult.match("<GREETER_MSG>(.*)</GREETER_MSG>");
		var message = messageID[1];
		if (message.length == 8) {
			clearInterval(requestQRCodeContent);

			serviceObj.funcAction = "PushCustomerInstantly";
			serviceObj.reqContent = "<code>" + message + "</code><codeType>" + "CI" + "</codeType><counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername>";
			sendRequestToAddon(serviceObj);
			RequestSv.handleReadQRCodeException();

		} else if (message.length > 8) {

			clearInterval(requestQRCodeContent);
			serviceObj.funcAction = "PushCustomerInstantly";
			serviceObj.reqContent = "<code>" + message + "</code><codeType>" + "RECEIPT" + "</codeType><counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername>";
			sendRequestToAddon(serviceObj);
			RequestSv.handleReadQRCodeException();
		} else {

		}

	},
	handleReadQRCodeException: function () {
		if (!isReadingQRCode) {
			serviceObj.funcAction = "OpenCloseQRCodeReader";
			serviceObj.reqContent = "<isOpen>1</isOpen>" + "<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername>";
			sendRequestToAddon(serviceObj);

		} else {
			serviceObj.funcAction = "OpenCloseQRCodeReader";
			serviceObj.reqContent = "<isOpen>0</isOpen>" + "<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername>";
			sendRequestToAddon(serviceObj);

		}

	},
	handleReadQRCodeSucess: function (outData) {
		var idRequest = outData.respResult.match("<ARGS>(.*)</ARGS>");
		if (idRequest[1] == 1) {
			isReadingQRCode = true;
			document.getElementById("btnReadQRCode").innerHTML = "Đang đọc QRCode";
			if (isReadingQRCode) {
				requestQRCodeContent = setInterval(function (e) {
					RequestSv.getReadQRCodeContent();
				}, gCFG_OBJ.EC_Timer_GetComingCustomer);
			}
		} else if (idRequest[1] == 0) {
			isReadingQRCode = false;
			clearInterval(requestQRCodeContent);
			document.getElementById("btnReadQRCode").innerHTML = "Đọc QRCode";
		}

	},
	handleExceptionAcceptClickTwo: function () {
		var userControl = document.getElementById('idUsername').innerHTML;
		serviceObj.funcAction = "PushCustomerInstantly_ChangeCounter";
		serviceObj.reqContent = "<customerTrackingID>" + exceptionTrackingId + "</customerTrackingID><counterUsername>" + userControl + "</counterUsername>";
		sendRequestToAddon(serviceObj);
		Common.logInfo("SANGNT1: handleExceptionAcceptClickTwo " + exceptionTrackingId);

	},
	handleExceptionAcceptClickTwoSuccess: function (outData) {
		var reqCode = outData.respResult.match("<RESP_CODE>(.*)</RESP_CODE>");
		var reqTContent = outData.respResult.match("<RESP_CONTENT>(.*)</RESP_CONTENT>");
		var trackingID = outData.respResult.match("<CUSTOMER_TRACKING_ID>(.*)</CUSTOMER_TRACKING_ID>");
		document.getElementById("validatePopupException").innerHTML = reqTContent[1];
		document.getElementById("validatePopupException").style.display = 'block';
		serviceObj.funcAction = "GetCommingCustomer";
		serviceObj.reqContent = "<counterUsername>" + document.getElementById('idUsername').innerHTML + "</counterUsername>";
		sendRequestToAddon(serviceObj);
		gIsEndServing = false;
		gIsServing = true;
		document.getElementById("btnCompleteServing").innerHTML = 'Kết thúc phục vụ';
		showButtonStatus();
		exceptionTrackingId = "";
		setTimeout(function (e) {
			RequestSv.handleExceptionClick();
		}, 300);

		Common.logInfo("SANGNT1: handleExceptionAcceptClickTwo " + exceptionTrackingId);
	},
	getHTMLCardNoInfo: function (inBookID) {
		var bookID = "";
		Common.logInfo("SANGNT1: getHTMLCardNoInfo:" + inBookID);
		gBookID = inBookID;
		Common.showLoading();
		serviceObj.funcAction = "GetHTMLCardHelpTable";
		serviceObj.reqContent = "";
		sendRequestToAddon(serviceObj);


	},
	repHTMLCardNoInfoSuccess: function (outData) {

		Common.hideLoading();
		isCardATMNoInfo = true;
		bookCardATMNoInfo = gBookID;
		var html = outData.respResult.match("<HTML>(.*)</HTML>");
		var contentHtml = html[1];
		document.getElementById("table_card_atm_noinfo").innerHTML = contentHtml;
		Common.logInfo("SANGNT1: repHTMLCardNoInfoSuccess: " + Common.stringFromJSONObj(outData));


	}, onChangeContentException: function (type, content) {

		Common.logInfo("SANGNT1: onChangeContentException: " + type + "   " + content);
		if (type == "CIF") {

			if (content.length > 0) {
				Common.logInfo("SANGNT1: " + content.length);
				document.getElementById('txtInputCMNDException').disabled = true;

			} else {
				document.getElementById('txtInputCMNDException').disabled = false;
			}

		} else if (type == "CMT") {
			if (content.length > 0) {
				document.getElementById('txtInputCIFException').disabled = true;

			} else {
				document.getElementById('txtInputCIFException').disabled = false;
			}

		}
	},
	cancelUrlToGreeter: function () {
		Common.logInfo("HuyNT2: SendUrlToGreeter:" + gCFG_OBJ.FCC_User);
		Common.showLoading();
		serviceObj.funcAction = "SendUrlToGreeter";
		serviceObj.reqContent = "<counterUsername>" + gCFG_OBJ.FCC_User + "</counterUsername><url>BLANK_URL</url>";
		sendRequestToAddon(serviceObj);
	},
	sendUrlToGreeter: function () {
		Common.logInfo("HuyNT2: SendUrlToGreeter:" + gCFG_OBJ.FCC_User);
		Common.showLoading();
		serviceObj.funcAction = "SendUrlToGreeter";
		serviceObj.reqContent = "<counterUsername>" + gCFG_OBJ.FCC_User + "</counterUsername><url>" + document.getElementById('txtInputUrl').value + "</url>";
		sendRequestToAddon(serviceObj);
	},
	respUrlToGreeter: function () {
		Common.logInfo("HuyNT2: SendUrlToGreeter:" + gCFG_OBJ.FCC_User);
		Common.hideLoading();
	},
	//caont ham get ip server from server 
	getIpServerFromServerPublic: function () {
		Common.logInfo("caont function get ipserver");

		Common.showLoading();

		serviceObj.funcAction = "requestGetInforFromServer";


		serviceObj.reqContent = '<typeGuest>ADDON</typeGuest>' +
			'<versionClient>1.0.1</versionClient>';
		sendRequestToAddon(serviceObj);
	},
	getTellerNameFromServerLocal: function () {
		Common.logInfo("caont function get TellerName");

		serviceObj.funcAction = "requestGetTellerName";

		sendRequestToAddon(serviceObj);
	},
	handleFillFCCClick: function () {
		if (gListEformIDFilled.indexOf(gEformID) != -1) {
			Common.showAlert("Giao dịch đã được xử lý rồi hoặc bị hết hiệu lực");
			return;
		}
		Common.logInfo("caont function get auto script ");

		serviceObj.funcAction = "GetAutoScript";
		serviceObj.reqContent = "<custTrackID>" + gCustomerTrackingIDEform + "</custTrackID>" + "<bookID>" + gBookID + "</bookID>";

		sendRequestToAddon(serviceObj);
	},
	/* KienNT13 - 2018/11/22 - Thêm Thông tin tặng quà*/
	// Lấy thông tin quà tặng
	handleGiveGiftClick: function () {
		document.getElementById("popupGiveGift").style.display = 'block';
		serviceObj.funcAction = "GetGiftInfo";
		serviceObj.reqContent = "<currentCustTrackingID>" + gTrackingID + "</currentCustTrackingID>";
		sendRequestToAddon(serviceObj);
	},
	// Hiển thị popup tặng quà
	parseDataGift: function (outData) {
		if (outData.respResult.indexOf("<CUSTOMER_NAME>") > -1 && outData.respResult.indexOf("</CUSTOMER_NAME>") > -1) {
			document.getElementById("txtCustomerName").value = outData.respResult.match("<CUSTOMER_NAME>(.*)</CUSTOMER_NAME>")[1];
		}

		if (outData.respResult.indexOf("<TIME_WAIT>") > -1 && outData.respResult.indexOf("</TIME_WAIT>") > -1) {
			document.getElementById("txtTimeWaiting").value = outData.respResult.match("<TIME_WAIT>(.*)</TIME_WAIT>")[1];
		}

		var giftName = '';
		if (outData.respResult.indexOf("<GIFT_NAME>") > -1 && outData.respResult.indexOf("</GIFT_NAME>") > -1) {
			giftName = outData.respResult.match("<GIFT_NAME>(.*)</GIFT_NAME>")[1];
		}
		document.getElementById('rdoGift').checked = !(giftName == null || giftName == undefined || giftName == "");
		document.getElementById("txtGiftName").value = giftName;

		var giftReason = '';
		if (outData.respResult.indexOf("<GIFT_REASON>") > -1 && outData.respResult.indexOf("</GIFT_REASON>") > -1) {
			giftReason = outData.respResult.match("<GIFT_REASON>(.*)</GIFT_REASON>")[1];
		}
		document.getElementById("rdoNoGift").checked = !(giftReason == null || giftReason == undefined || giftReason == "");
		document.getElementById("txtGiftReason").value = giftReason;
	},
	// Đóng popup tặng quà
	handleGiveGiftClose: function () {
		document.getElementById("popupGiveGift").style.display = 'none';
	},
	// Disable text khi chọn Tặng quà / Không tặng
	activeGiveGift: function (id) {
		if (id == 'rdoGift') {
			document.getElementById('txtGiftName').disabled = false;
			document.getElementById('txtGiftName').focus();
			document.getElementById('txtGiftReason').disabled = true;
			document.getElementById('txtGiftReason').value = "";
		} else {
			document.getElementById('txtGiftName').disabled = true;
			document.getElementById('txtGiftName').value = "";
			document.getElementById('txtGiftReason').disabled = false;
			document.getElementById('txtGiftReason').focus();
		}
	},
	// Kiểm tra và lưu thông tin quà tặng
	handleGiveGiftSave: function () {
		if (!document.getElementById('rdoGift').checked && !document.getElementById('rdoNoGift').checked) {
			Common.showAlert("Vui lòng nhập thông tin Quà tặng");
			return;
		}
		if (document.getElementById('rdoGift').checked && !document.getElementById("txtGiftName").value) {
			Common.showAlert("Vui lòng nhập thông tin Loại quà");
			document.getElementById('txtGiftName').focus();
			return;
		}
		if (document.getElementById('rdoNoGift').checked && !document.getElementById("txtGiftReason").value) {
			Common.showAlert("Vui lòng nhập thông tin Lý do");
			document.getElementById('txtGiftReason').focus();
			return;
		}
		serviceObj.funcAction = "AddOrUpdateGiveGift";
		serviceObj.reqContent = "<currentCustTrackingID>" + gTrackingID + "</currentCustTrackingID>" +
			"<giftName>" + document.getElementById('txtGiftName').value + "</giftName>" +
			"<giftReason>" + document.getElementById('txtGiftReason').value + "</giftReason>";
		sendRequestToAddon(serviceObj);
	},
	// Kết quả lưu thông tin quà tặng
	giftSuccess: function (outData) {
		if (outData.respResult == '') {
			gGiveGift = true;
			document.getElementById("popupGiveGift").style.display = 'none';
		}
		else {
			Common.showAlert(outData.respResult);
			return;
		}
	}
	/* END KienNT13 - 2018/11/22 - Thêm Thông tin tặng quà */
}

function showhideMenuSelectDeal() {
	if (!menuSelectDealShowHide) {
		document.getElementById('menu_select_deal').style.display = 'block';
		menuSelectDealShowHide = true;
	} else {
		document.getElementById('menu_select_deal').style.display = 'none';
		menuSelectDealShowHide = false;
	}

}

/*
	Login action
*/
function loginToSystem() {

}



/*
*/
function removeAllChildNodes(inParent) {
	var children = inParent.childNodes.length;
	for (var i = 0; i < children.length; i++) {
		inParent.removeNode(children[i]);
	}
}
function IsNumeric(input) {
	return (input - 0) == input && ('' + input).trim().length > 0;
}
function validateInPutOnlyNumber(data) {
	var reg = /^\d+$/;
	return reg.test(data);
}
function checkFormatDate(date) {
	if (date == "" || date == undefined || date == null) {
		return false;
	}
	var arrdate = date.split('/');
	if (arrdate[0] == undefined) {
		return false;
	} else {
		if (arrdate[0].length != 2)
			return false;
		else if (parseInt(arrdate[0]) < 0 || parseInt(arrdate[0]) > 31)
			return false;
	}
	if (arrdate[1] == undefined) {
		return false;
	} else {
		if (arrdate[1].length != 2)
			return false;
		else if (parseInt(arrdate[1]) < 0 || parseInt(arrdate[1]) > 12)
			return false;
	}
	if (arrdate[2] == undefined) {
		return false;
	} else {
		if (arrdate[2].length != 4)
			return false;
	}
	return true;
}
function sendFCCInfoToServer(inStr) {
	RequestSv.sendFCCTransInfo(inStr);

}
function showCustSignatureImg() {
	RequestSv.getCustSignatureImage();
}
var idxStep = 0;
function runAutoScript(inSteps) {
	if (inSteps[idxStep]) {
		inSteps[idxStep].script = replaceKeyWithValue(inSteps[idxStep].script);
		sendInfoToAddon(inSteps[idxStep]);
		if (inSteps[idxStep].delayEnd) {
			var tmpTimer = setTimeout(function (e) {
				clearTimeout(tmpTimer);
				tmpTimer = null;
				idxStep++;
				runAutoScript(inSteps);
			}, inSteps[idxStep].delayEnd);
		}
	}
	else {
		return;
	}
}
var AutoScriptFunc = function () {
	this.idxStep = 0;
	var that = this;
	this.runAutoScript = function (inSteps) {
		if (inSteps[that.idxStep]) {
			inSteps[that.idxStep].script = replaceKeyWithValue(inSteps[that.idxStep].script);
			sendInfoToAddon(inSteps[that.idxStep]);
			if (inSteps[that.idxStep].delayEnd) {
				var tmpTimer = setTimeout(function (e) {
					clearTimeout(tmpTimer);
					tmpTimer = null;
					that.idxStep++;
					that.runAutoScript(inSteps);
				}, inSteps[that.idxStep].delayEnd);
			}
		}
		else {
			return;
		}
	}
}

function replaceKeyWithValue(inStr) {
	return replaceAll(inStr, gCFG_OBJ.EC_Customer_CIF_Key, gCurCIF);
}

function escapeRegExp(string) {
	return string.replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1");
}

function replaceAll(oldStr, findStr, replaceStr) {
	return oldStr.replace(new RegExp(escapeRegExp(findStr), 'g'), replaceStr);
}

function showButtonStatus() {
	if (gIsServing) {
		document.getElementById('btnStartServing').disabled = true;
		document.getElementById('btnCompleteServing').disabled = false;
		document.getElementById('btnSendCustInfo').disabled = false;
		document.getElementById('btnShowWaitingScreen').disabled = true;
		document.getElementById('btnResetCustSignature').disabled = false;
		document.getElementById('btnPrintFile').disabled = false;
		//document.getElementById('btnSelectDeal').disabled = true;
		if (gIsEndServing) {
			document.getElementById('btnAcceptException').disabled = false;
			document.getElementById('btnShowWaitingScreen').disabled = false;
		} else {
			document.getElementById('btnAcceptException').disabled = true;

		}
	}
	else {
		//caont add
		document.getElementById('btnFillFCC').disabled = true;
		document.getElementById('btnDetailTran').disabled = true;
		document.getElementById('btnStartServing').disabled = false;
		document.getElementById('btnCompleteServing').disabled = true;
		document.getElementById('btnSendCustInfo').disabled = true;
		document.getElementById('btnShowWaitingScreen').disabled = true;
		document.getElementById('btnResetCustSignature').disabled = true;
		document.getElementById('btnPrintFile').disabled = true;
		if (gTrackingID && gTrackingID.length > 0)
			document.getElementById('btnAcceptException').disabled = true;
		else
			document.getElementById('btnAcceptException').disabled = false;
		//document.getElementById('btnSelectDeal').disabled = true;


	}
}


/* READY!!! */

document.addEventListener("DOMContentLoaded", function (event) {
	Common.logInfo("Sidebar load ready!!!");
	showButtonStatus();

	Notification.requestPermission(function (permission) {
		if (permission === "granted") {
			var notif = new Notification("HuyNT2 there!");
			notif.onclose = function (nt) {
				nt.preventDefault();
			}
			notif.onshow = function (nt) {
				setTimeout(function (e) {
					nt.close();
				}, 15000);
			}
			//setTimeout(notif.close.bind(notif), 5000);
		}
	});
});

function getDataOfATMCardService() {



	var numberAccount = document.getElementById('atm-cif-customer').value;
	var numberCIF = "";
	if (numberAccount.length > 8);
	numberCIF = numberAccount.substring(0, 8);
	var name = document.getElementById('atm-name-customer').value;

	var numberCard = document.getElementById('atm-number-card').value;
	var Cmt = document.getElementById('atm-cmt-customer').value;
	var ngayCap = document.getElementById('atm-date-customer').value;
	var noiCap = document.getElementById('atm-address-customer').value;
	var khoaThe = document.getElementById('atm-lock-card').value;
	var khPin = document.getElementById('atm-active-pin-again').value;
	var huyTklk = document.getElementById('atm-destroy-account').value;
	var tkHuy = document.getElementById('atm-account-destroy-name').value;
	var doitkMacdinh = document.getElementById('atm-change-account').value;
	var tkMacdinh = document.getElementById('atm-account-change-default').value;
	var moKhoathe = document.getElementById('atm-unlock-card').value;
	var pinMoi = document.getElementById('atm-active-pin-new').value;
	var themTKLKthe = document.getElementById('atm-add-new-account').value;
	var tklkThe = document.getElementById('atm-name-new-account').value;
	var ngungSDthe = document.getElementById('atm-end-card').value;
	var ycKhac = document.getElementById('atm-request-other').value;
	var ndYeucau = document.getElementById('atm-content-request-other').value;
	var phlThechinh = document.getElementById('atm-card-main').value;
	var phlThephu = document.getElementById('atm-card-extra').value;
	var llThenuot = document.getElementById('atm-card-in-ebank').value;
	var khieunai = document.getElementById('atm-card-complain').value;
	var ndLaylaithenuot = document.getElementById('atm-content-card-in-ebank').value;
	var ndKhieunai = document.getElementById('atm-content-card-complain').value;
	var paramRequest = "<parameters>" + gTrackingID + "#" + gBookID + "#" + numberCIF + "#" + name + "#" + numberAccount + "#" +
		numberCard + "#" + Cmt + "#" + ngayCap + "#" + noiCap + "#" + khoaThe + "#" + khPin + "#" + huyTklk + "#" + tkHuy + "#" + doitkMacdinh + "#"
		+ tkMacdinh + "#" + moKhoathe + "#" + pinMoi + "#" + themTKLKthe + "#" + tklkThe + "#" + ngungSDthe + "#" + ycKhac + "#" + ndYeucau + "#"
		+ phlThechinh + "#" + phlThephu + "#" + llThenuot + "#" + khieunai + "#" + ndLaylaithenuot + "#" + ndKhieunai + "#" + "</parameters>";

	return paramRequest;

}
function getDateSystem() {
	var date = new Date();
	var result = "";
	result = addZeroToDate(date.getDate()) + "/" + addZeroToDate((date.getMonth() + 1)) + "/" + date.getFullYear() + " " + addZeroToDate(date.getHours()) + ":" + addZeroToDate(date.getMinutes()) + ":" + addZeroToDate(date.getSeconds());
	return result;
}
function addZeroToDate(str) {
	if (str.toString().length == 1) {
		return "0" + str;
	}
	return str;
}
function showButtonStatusbtn_Pause_play() {
	if (isPause_Play) {
		document.getElementById('btnStartServing').disabled = true;
		document.getElementById('btnCompleteServing').disabled = true;
		document.getElementById('btnSendCustInfo').disabled = true;
		document.getElementById('btnResetCustSignature').disabled = true;
		document.getElementById('btnPrintFile').disabled = true;

		document.getElementById('btnAcceptException').disabled = true;
		document.getElementById('btnSearchEForm').disabled = true;
		document.getElementById('btnFillFCC').disabled = true;
		document.getElementById('btnDetailTran').disabled = true;
	} else {
		// document.getElementById('btnStartServing').disabled = false;
		document.getElementById('btnCompleteServing').disabled = false;
		document.getElementById('btnSendCustInfo').disabled = false;
		document.getElementById('btnShowWaitingScreen').disabled = false;
		document.getElementById('btnResetCustSignature').disabled = false;
		document.getElementById('btnAcceptException').disabled = false;
		document.getElementById('btnSearchEForm').disabled = false;
	}
}
// window.onload = function(){
// 	i
// 		setTimeout(alertStart10s, 10000);
// };
// function alertStart10s(){
// 	alert("KH đã chuyển trạng thái P - Khách hàng vào quầy, chưa bấm phục vụ sau 10s");
// }
var id = setInterval(frame, 10000);
function frame() {
	if (isBtnStart) {
		clearInterval(id);
	} else {
		alert("KH đã chuyển trạng thái P - Khách hàng vào quầy, chưa bấm phục vụ sau 10s");
	}
}

