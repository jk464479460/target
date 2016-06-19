$(document).ready(function () {
    function Feng() {
        this.ssid = "";
        this.address = "";
        this.bindEvent();
        this.initUi();
    }
    Feng.prototype = {
        bindEvent: function () {
            var self = this;
            self.ssid = self.getCookieW();
            $(document).on('click', '#editAddress', function () {
                var oldAddress = $("#addressLb").text();
                $("#addressLb").css("display", "none");
                $("#editAddress").css("display", "none");
                $(".newAddressTxt").css("display", "block");
                $(".newAddressOver").css("display", "block");
                $(".newAddressTxt").val(oldAddress);
            });
            $(document).on("click", '.newAddressOver', function () {
                $("#addressLb").css("display", "block");
                $("#editAddress").css("display", "block");
                $(".newAddressOver").css("display", "none");
                $(".newAddressTxt").css("display", "none");
                $("#addressLb").text($(".newAddressTxt").val());
            });
            $(document).on("pagecreate","#ownerAddress",function(){
                $("#addressLb").text(self.address);
                self.initArea();
            });
            $(document).on("pagecreate", "#recommendUser", function () {
                self.initRecommend();
            });

            $(document).on('click','#btnReg',function() {
                if (!self.checkInput()) {
                    return;
                }
                var json = self.getParam();
                $.ajax({
                    url: "../../../api/UserModifyPass/UserModify",
                    type: "POST",
                    dataType: "json",
                    data: json,
                    beforeSend: showLoader(),
                    error: function(err) {
                        alert(err);
                    },
                    success: function(data) {
                        hideLoader();
                        data = JSON.parse(data);
                        if (data.Exception.Success) {
                            ShowInfo2("#ownerInfoPage .popupBasic", "保存成功");
                        } else {
                            ShowInfo2("#ownerInfoPage .popupBasic", "出错啦亲");
                        }
                    }
                });
            });
            $(document).on("click", "#saveAddress",function() {
                var checkYes = true;
                if ($("#addressLb").text() === "")
                    checkYes = false;
                if (checkYes === false) return;
                var queryParam = {};
                queryParam.Ssid = self.ssid;
                queryParam.Address = $("#addressLb").text();
                $.ajax({
                    url: "../../../api/UserAddress",
                    type: "POST",
                    dataType: "json",
                    beforeSend: showLoader(),
                    data: queryParam,
                    success: function (json) {
                        hideLoader();
                        var data = JSON.parse(json);
                        if (data.Exception.Success) {
                            ShowInfo2("#ownerAddress .popupBasic", "保存成功");
                        } else {
                            ShowInfo2("#ownerAddress .popupBasic", "出错啦亲");
                        }
                    },
                    error: function () {
                        ShowInfo2("#ownerAddress .popupBasic", "error");
                    }
                });
            });
            $(document).on("click", "#savePostText",function() {
                var title = $("#textTitle").val();
                var text = $("#PostText").val();
                var checkYes = true;
                if (title === "" || text === "")
                    checkYes = false;
                if (checkYes === false) return;
                var json = {};
                json.Title = title;
                json.PostText = text;
                json.Ssid = self.ssid;
                $.ajax({
                    url: "../../../api/UserPostInfo/AddPostInfo",
                    type: "POST",
                    dataType: "json",
                    beforeSend: showLoader(),
                    data: json,
                    success: function (data) {
                        hideLoader();
                        data = JSON.parse(data);
                        if (data.Exception.Success) {
                            ShowInfo2("#postPapaer .popupBasic", "保存成功");
                        } else {
                            ShowInfo2("#postPapaer .popupBasic", "出错啦亲");
                        }
                    },
                    error: function () {
                        ShowInfo2("#postPapaer .popupBasic", "error");
                    }
                });
            });
            $(document).on("click", "#submitRecUser", function () {
                self.ssid = self.getCookieW();
                var json = {};
                var user = $("#recUser").val();
                var phone = $("#recUserPhone").val();
                if (user === "" || phone === "")
                {
                    ShowInfo2("#recommendUser .popupBasic", "必须填写推荐的用户名和手机号");
                    return;
                }
                json.User = user;
                json.Phone = phone;
                json.Ssid = self.ssid;
                $.ajax({
                    url: "../../../api/RecommendUser/RecommendUser",
                    type: "POST",
                    dataType: "json",
                    beforeSend: showLoader(),
                    data: json,
                    success: function (data) {
                        hideLoader();
                        data = JSON.parse(data);
                        if (data.Exception.Success) {
                            ShowInfo2("#recommendUser .popupBasic", "保存成功");
                            self.initRecommend();
                        } else {
                            //ShowInfo2("#recommendUser .popupBasic", "出错啦亲");
                            switch (data.Exception.Exmsg) {
                                case "1":
                                    ShowInfo2("#recommendUser .popupBasic", "推荐用户未注册");
                                    break;
                                case "2":
                                    ShowInfo2("#recommendUser .popupBasic", "推荐人电话填写错误");
                                    break;
                                case "3":
                                    ShowInfo2("#recommendUser .popupBasic", "某人抢先推荐了");
                                    break;
                                case "4":
                                    ShowInfo2("#recommendUser .popupBasic", "自己无法推荐自己哦");
                                    break;
                            }
                        }
                    },
                    error: function () {
                        ShowInfo2("#postPapaer .popupBasic", "error");
                    }
                });
            });
        },
        initRecommend: function () {
            var self = this;
            self.ssid = self.getCookieW();
            
            var queryParam = {};
            queryParam.Ssid = self.ssid;
            $.ajax({
                url: "../../../api/GetRecommend/GetRecommendUser",
                type: "POST",
                dataType: "json",
                beforeSend: showLoader(),
                data: queryParam,
                success: function (json) {
                    hideLoader();
                    var data = JSON.parse(json);
                    if (data.Exception.Success) {
                        $("#recUserList").empty();
                        for(var i=0;i<data.RecommendList.length;i++){
                            var li = '<li>' + data.RecommendList[i].User + '</li>';
                            $("#recUserList").append(li).trigger("create");;
                        }
                        $("#recUser").val('');
                        $("#recUserPhone").val('');
                        if (data.RecommendList.length == 0)
                            $("#recUserList").append("<li>您还未推荐人哦</li>").trigger("create");
                    } else {
                        ShowInfo2("出错啦亲");
                    }
                },
                error: function () {
                    ShowInfo2("出错啦亲");
                }
            });
        }
        , initUi: function () {
            var self = this;
            var queryParam = {};
            queryParam.Ssid = self.ssid;
            $.ajax({
                url: "../../../api/UserSid/IsReg",
                type: "POST",
                dataType: "json",
                beforeSend: showLoader(),
                data: queryParam,
                success: function (json) {
                    hideLoader();
                    var data = JSON.parse(json);
                    if (data.Exception.Success) {
                        if (data.Exception.Exmsg === "not found") {
                            ShowInfo2("请先登陆");
                            location.href = "../login.html";
                        } else {
                            self.address = data.Address;
                        }
                    } else {
                        //ShowInfo2(data.Exception.Exmsg);
                    }
                },
                error: function () {

                }
            });
        }
        ,initArea:function() {
            $.ajax({
                url: "../../../Api/AreaV1/GetArea",
                type: "POST",
                dataType: "json",
                success: function (jsonStr) {
                    var jsonObj = JSON.parse(jsonStr);
                    if (jsonObj.Exception.Success) {
                        for (var i = 0; i < jsonObj.Areas.length; i++) {
                            var html = '<option value="' + jsonObj.Areas[i].Id + '">' + jsonObj.Areas[i].Name + '</option>';
                            $("#day").append(html);
                        }
                        $("#day").selectmenu('refresh', true);
                    } else {

                    }
                }
               , error: function () {

               }
            });
        }
        , getCheckCode: function () {
             var dataParam = {};
             dataParam.PhoneNumber = $("#phone").val();
             $.ajax({
                 url: "../../../api/reg/SendRegCode",
                 type: "POST",
                 dataType: "json",
                 beforeSend: showLoader(),
                 data: dataParam,
                 success: function (json) {
                     hideLoader();
                     var data = JSON.parse(json);
                 },
                 error: function () {

                 }
             });
         }
        , getCookieW: function () {
              var cookie = getCookie("UID");
              if (cookie === null) location.href = "../index.html";
              return cookie;
        }
        ,checkInput:function() {
            var oldPass = $("#oldPass").val();
            var pass1 = $("#password1").val();
            var pass2 = $("#password2").val();
            var phone = $("#phone").val();
            var validateNumber = $("#validateNumber").val();
            var reg = /^\d{11}$/;
            if (pass1 != pass2) {
                $("#errMsgPwd2").text("错误:二次密码不一致");
                return false;
            }
           /* if (reg.test(phone)) {
                $("#phone .ui-input-text").css(" background-color", 'red');
                $("#phone").attr("placeholder","请填写短信中的注册码");
                return false;
            }
            if (validateNumber === "") {
                $("#validateNumber .ui-input-text").css(" background-color", 'red');
                $("#validateNumber").attr("placeholder","请填写短信中的注册码");
                return false;
            }*/
            if (oldPass === "") {
                $("#username .ui-input-text").css(" background-color", 'red');
                $("#errMsgUName").text("不能为空");
                return false;
            }
            return true;
        }
        , getParam: function () {
            var self = this;
            var json = {};
            json.Ssid = self.ssid;
            json.OldPass = $("#oldPass").val();
            json.Paw = $("#password1").val();
            json.Phone = $("#phone").val();
            return json;
        }
    }
    var fengObj = new Feng();
});