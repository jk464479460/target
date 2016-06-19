$(document).ready(function() {
    function Pay() {
        this.ssid = "";
        this.bindEvent();
        this.initUi();
    }

    Pay.prototype = {
        bindEvent:function() {
            var self = this;
            self.ssid = self.getCookieW();
            $(document).on('click', '#editAddress', function () {
                $('#addressLb').css('display', 'none');
                $('.newAddressTxt').css('display', 'block');
            });

            $(document).on('click', 'a.newAddressTxt', function () {
                $('#addressLb').css('display', 'block');
                var addStr = $('input.newAddressTxt').val();
                $('#addressLb').val(addStr);
                $('.newAddressTxt').css('display', 'none');
            });

            $(document).on('click', '#btn', function () {
                var address = $("#addressLb").val();
                var phone = $("#phone").val();
                var reg = /^\d{11}$/;
                if (address === "" || phone === "" || !reg.test(phone)) {
                    return;
                }
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

            $(document).on('click', "#submitOrder", function () {
                var address = $("#addressLb").val();
                var phone = $("#phone").val();
                var ssid = self.ssid;
                var dataParam = {};
                dataParam.Ssid = ssid;
                dataParam.Phone = phone;
                dataParam.Address = address;
                dataParam.CheckCode = $("#checkCode").val();
                dataParam.AreaId = $("#day").val();
                if (dataParam.CheckCode === "") {
                    return;
                }
                $.ajax({
                    url: "../../../api/SubmitOrder/Submit",
                    dataType: "json",
                    type: "POST", 
                    data: dataParam,
                    success: function (json) {
                        json = JSON.parse(json);
                        if (json.Exception.Success) {
                            location.href = "../orderShow.html?orderId=" + json.OrderId;
                        } else {
                            ShowInfo("下单失败");
                            alert(json.Exception.Exmsg)
                        }
                    },
                    error:function() {
                        
                    }
                });
            });
        }
        , initUi:function() {
            var self = this;
            var dataParam = {};
            dataParam.Ssid = self.ssid;
            $.ajax({
                url: "../../../api/Pay/InitPayInfo",
                type: "POST",
                dataType: "json",
                data: dataParam,
                beforeSend: showLoader(),
                success: function (jsonStr) {
                    hideLoader();
                    var json = JSON.parse(jsonStr);
                    if (json.Exception.Success) {
                        $("#payFee").text(json.PayFee);
                        $("phone").val(json.Phone);
                        $("#addressLb").val(json.Address===null?"":json.Address);
                        for (var i = 0; i < json.GoodsList.length; i++) {
                            var obj = json.GoodsList[i];
                            var tpl = htmlTpl.replace("#code#", obj.GoodsCode == null ? "" : obj.GoodsCode);
                            tpl = tpl.replace("#goodsDescri#", obj.GoodsName == null ? "" : obj.GoodsName);
                            tpl = tpl.replace("#price#", obj.GoodsPrice);
                            $("#goodsList").append(tpl);
                        }
                    } else {
                        
                    }
                },
                error:function() {
                    
                }
            });

            $.ajax({
                url: "../../../Api/AreaV1/GetArea",
                type: "POST",
                dataType: "json",
                success: function(jsonStr) {
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
                ,error:function() {

                }
            });
        }
         , getCookieW: function () {
             var cookie = getCookie("UID");
             if (cookie === null) location.href = "../index.html";
             return cookie;
         }
        , getCheckCode: function () {
            var dataParam = {};
            dataParam.PhoneNumber = $("#phone").val();
            $.ajax({
                url: "../../../api/Phone/SendCheckCode",
                type:"POST",
                dataType: "json",
                beforeSend: showLoader(),
                data:dataParam,
                success: function (json) {
                    hideLoader();
                    var data = JSON.parse(json);
                    if (data === "ok")
                        ShowInfo("已经发送验证码");
                    else ShowInfo("发送失败，请重试");
                },
                error:function() {
                    
                }
            });
        }
    };

    var payObj = new Pay();
});

var htmlTpl='<tr>\
                          <td><img src="/images/goods/#code#.jpg" data-role="none" style="float:left;">\
                          <div style="float: right; ">\
                              <pre>#goodsDescri#</pre>\
                              <pre>#price#</pre>\
                          </div></td>\
                      </tr>';