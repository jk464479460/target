function setCookie(name, value) {
    var minutes = 60*10;
    var exp = new Date();
    exp.setTime(exp.getTime() + minutes * 60 * 1000);
    document.cookie = name + "=" + escape(value) + ";expires=" + exp.toGMTString();
    //location.href = "../index.html";
}

function getCookie(name) {
    var arr, reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");
    if (arr = document.cookie.match(reg)) {
        return unescape(arr[2]);
    }
    else
        return null;
}

function delCookie(name) {
    var exp = new Date();
    exp.setTime(exp.getTime() - 1000);
    var cval = getCookie(name);
    if (cval != null) {

        document.cookie = name + "=" + cval + ";expires=" + exp.toUTCString() + ";path=/;";
    }

}


//
//function setCookie(c_name="UID", value, expiredays=0.5) {
//    var exdate = new Date()
//    exdate.setDate(exdate.getDate() + expiredays)
//    document.cookie = c_name + "=" + escape(value) +
//    ((expiredays == null) ? "" : ";expires=" + exdate.toGMTString())
//}

////取回cookie
//function getCookie(c_name) {
//    if (document.cookie.length > 0) {
//        c_start = document.cookie.indexOf(c_name + "=")
//        if (c_start != -1) {
//            c_start = c_start + c_name.length + 1
//            c_end = document.cookie.indexOf(";", c_start)
//            if (c_end == -1) c_end = document.cookie.length
//            return unescape(document.cookie.substring(c_start, c_end))
//        }
//    }
//    return ""
//}

function showLoader() {
    $.mobile.loading('show', {
        text: '处理中...', //加载器中显示的文字  
        textVisible: true, //是否显示文字  
        theme: 'a',        //加载器主题样式a-e  
        textonly: false,   //是否只显示文字  
        html: ""           //要显示的html内容，如图片等  
    });
}

//隐藏加载器.for jQuery Mobile 1.2.0  
function hideLoader() {
    //隐藏加载器  
    $.mobile.loading('hide');
}

function ShowInfo(showInfo) {
    $.mobile.loading('show', {
        text: showInfo, //加载器中显示的文字  
        textVisible: true, //是否显示文字  
        theme: 'a',        //加载器主题样式a-e  
        textonly: false,   //是否只显示文字  
        html: ""           //要显示的html内容，如图片等  
    });
    setTimeout(3000, $.mobile.loading('hide'));
}
//休眠函数
function sleep(numberMillis) {
    var now = new Date();
    var exitTime = now.getTime() + numberMillis;
    while (true) {
        now = new Date();
        if (now.getTime() > exitTime)
            return;
    }
}

function CloseInfo() {
    $("#popupBasic").popup("close");
}

setInterval('CloseInfo()', 2000);
function ShowInfo(text) {
    $("#popupBasic>p").text(text);
     $("#popupBasic").popup("open");
}


function CloseInfo2(obj) {
    $("+obj+").popup("close");
}

function ShowInfo2(obj,text) {
    $(obj).text(text);
    $(obj).popup("open");
    setInterval(function() {
        $(obj).popup("close");
    }, 2000);
}
//获取url参数
function GetRequest() {

    var url = location.search; //获取url中"?"符后的字串
    var theRequest = new Object();
    if (url.indexOf("?") != -1) {
        var str = url.substr(1);
        strs = str.split("&");
        for (var i = 0; i < strs.length; i++) {
            theRequest[strs[i].split("=")[0]] = (strs[i].split("=")[1]);
        }
    }
    return theRequest;
}