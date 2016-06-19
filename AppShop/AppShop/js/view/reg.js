$(document).ready(function () {
    function Reg() {
        this.ssid = "";
        this.bindEvent();
    }
    Reg.prototype = {
        bindEvent: function () {
            var self = this;
            self.ssid = self.getCookieW();
            $(document).on('click', '#btn', function () {
                var countdown = 60;
                function settime() {
                    var val = $("#btn");
                    if (countdown === 0) {
                        $(val).removeAttr("disabled");
                        $(val).val("获取验证码");
                        countdown = 60;
                        clearInterval(sh);
                    } else {
                        $(val).attr("disabled", true);
                        $(val).val("重新发送(" + countdown + ")");
                        countdown--;
                    }

                }
                var sh;
                sh = setInterval(settime, 1000);
                self.getCheckCode();
            });

            $(document).on('click','#btnReg',function() {
                if (!self.checkInput()) {
                    return;
                }
                var json = self.getParam();
                $.ajax({
                    url: "../../../api/RegAdd/Add",
                    type: "POST",
                    dataType: "json",
                    data: json,
                    beforeSend: showLoader(),
                    error: function() {

                    },
                    success: function(data) {
                        hideLoader();
                        data = JSON.parse(data);
                        if (data.Exception.Success) {
                            ShowInfo2("提交成功");
                            location.href = "../login.html";
                        } else {
                            var err = data.Exception.Exmsg;
                            var obj = "#home .popupBasic"
                            switch (err) {
                                case "1":
                                    ShowInfo2(obj,"用户名或验证码密码电话不能空");
                                    break;
                                case "2":
                                    ShowInfo2(obj,"密码长度不小于6位");
                                    break;
                                case "3":
                                    ShowInfo2(obj,"手机号长度11位");
                                    break;
                                case "4":
                                    ShowInfo2(obj,"验证码过期");
                                    break;
                                case "5":
                                    ShowInfo2(obj,"验证码错误");
                                    break;
                                case "6":
                                    ShowInfo2(obj,"用户已经存在");
                                    break;
                            }
                        }
                    }
                });
            });
        }
        , initUi: function () {

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
                     console.log(data)
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
            var uName = $("#username").val();
            var pass1 = $("#password1").val();
            var pass2 = $("#password2").val();
            var phone = $("#phone").val();
            var validateNumber = $("#validateNumber").val();
            var reg = /^\d{11}$/;
            if (pass1 != pass2) {
                $("#errMsgPwd2").text("错误:二次密码不一致");
                return false;
            }
            //if (reg.test(phone)) {
            //    $("#phone .ui-input-text").css(" background-color", 'red');
            //    $("#phone").attr("placeholder","请填写短信中的注册码");
            //    return false;
            //}
            if (validateNumber === "") {
                $("#validateNumber .ui-input-text").css(" background-color", 'red');
                $("#validateNumber").attr("placeholder","请填写短信中的注册码");
                return false;
            }
            if (uName === "") {
                $("#username .ui-input-text").css(" background-color", 'red');
                $("#errMsgUName").text("用户名不能为空");
                return false;
            }
            return true;
        }
        , getParam: function () {
            var self = this;
            var json = {};
            json.Ssid = self.ssid;
            json.UName = $("#username").val();
            json.Paw = $("#password1").val();
            json.Phone = $("#phone").val();
            json.CheckCode = $("#validateNumber").val();
            return json;
        }
    }
    var regObj = new Reg();
});