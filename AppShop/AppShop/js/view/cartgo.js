$(document).ready(function() {
    function cartGo() {
        this.ssid = "";
        this.bindEvent();
        this.initUi();
    }

    cartGo.prototype = {
        bindEvent: function () {
            var self = this;
            self.ssid = self.getCookieW();
            //移除商品
            $(document).on('click', '.rmGoods', function () {
                var dataParam = {};
                dataParam.Ssid = self.ssid;
                dataParam.Code = $(this).attr("data-code");
                $.ajax({
                    url: "../../../api/RmCartgo/RmCartgo",
                    type: "POST",
                    dataType: "json",
                    beforeSend: showLoader(),
                    data: dataParam,
                    success: function (json) {
                        hideLoader();
                        var obj = JSON.parse(json);
                        if (obj.Exception.Success) {
                            ShowInfo("删除成功");
                            self.initUi();
                        } else {
                            ShowInfo("出错");
                        }
                    },
                    error:function() {
                        ShowInfo("出错啦");
                    }
                });
            });
            $(document).on('click', '.IncrCnt,.DecCnt', function () {
                var cnt = 0;
                var oldFee = 0;
                if ($(this).attr('class') === 'IncrCnt') {
                    cnt = parseInt($($(this).next()).val());
                    oldFee = cnt;
                    cnt = cnt + 1;
                    $($(this).next()).val(cnt);
                } else {
                    cnt = parseInt($($(this).prev()).val());
                    oldFee = cnt;
                    cnt = cnt - 1;
                    if (cnt <= 0) return;
                    $($(this).prev()).val(cnt);
                }
                var priceOne = $($(this).parent()).attr('data-price');
                $(this).parent().parent().parent().next().find('.totalPrice').text = "";
                var curTotalPrice = (cnt * parseFloat(priceOne)).toFixed(2);

                $(this).parent().parent().parent().next().find('.totalPrice').text(curTotalPrice);
                var curFee = parseFloat($("#payFee").text());
                curFee = curFee - parseFloat(priceOne) * oldFee;
                $("#payFee").text(curFee + parseFloat(curTotalPrice));
                var dataJson = {};
                dataJson.Cnt = cnt;
                dataJson.Ssid = self.ssid;
                dataJson.Code = $(this).attr("data-code");
                $.ajax({
                    url: "../../../api/CartgoAddBuyCnt/AddBuyCnt",
                    type: "POST",
                    dataType: "json",
                    data:dataJson,
                    beforeSend: showLoader(),
                    success: function (json) {
                        hideLoader();
                        var rtJson = JSON.parse(json);
                        if (rtJson.Exception.Success) {
                        } else {
                            ShowInfo("出现错误");
                        }
                    },
                    error: function (error) {
                        ShowInfo("出现错误");
                    }
                });
            });
        }
        , initUi:function() {
            var self = this;
            var dataJson = {};
            dataJson.Ssid = self.ssid;
            $("#listCartgo").children().remove();
            $.ajax({
                url: "../../../api/CartGoShow/InitShowCart",
                type: "POST",
                dataType: "json",
                data: dataJson,
                beforeSend: showLoader(),
                success: function (json) {
                    hideLoader();
                    var rtJson = JSON.parse(json);
                    if (rtJson.Exception.Success) {
                        for (var i = 0; i < rtJson.CartGoAll.AllGoodsInCartgo.length; i++) {
                            var cartgo = rtJson.CartGoAll.AllGoodsInCartgo[i];
                            var templ = htmlTemplate.replace("!!!", cartgo.StockCode);
                            templ = templ.replace("&&&", cartgo.StockCode);
                            templ = templ.replace("@@@", cartgo.GoodsName);
                            templ = templ.replace("###", cartgo.CurPrice);
                            templ = templ.replace("###", cartgo.CurPrice);
                            templ = templ.replace("%%%", cartgo.BuyCnt);
                            templ = templ.replace("!!!", cartgo.StockCode);
                            templ = templ.replace("!!!", cartgo.StockCode);
                            templ = templ.replace('$$$', cartgo.TotalPrice);
                            $("#listCartgo").append(templ);
                        }
                        $('#listCartgo').append(operHtml);
                        $("#payFee").text(rtJson.CartGoAll.TotalPayment);
                    } else {
                        ShowInfo("出现错误");
                    }
                },
                error:function() {
                    ShowInfo("出现错误啦");
                }
            });
        }
        ,getCookieW:function() {
            var cookie = getCookie("UID");
            if (cookie === null) location.href = "../index.html";
            return cookie;
        }
    };
    var obj = new cartGo();
});

var htmlTemplate = '<tr data-role="none" style="margin-left:-36px;">' +
            '<td style="" width="5%"><img src="/images/goods/!!!.jpg" data-role="none" class="picGoods" width="55px"></td>' +
            '<td  style="width:25%;"><pre>@@@ &nbsp;&nbsp;单价:###</pre></td>' +
            '<td style="width:10%;"><div data-descr="操作" data-price="###">\
                        <a style="text-decoration: none;float:left" data-role="none" href="javascript:void(0)" class="IncrCnt" data-code="!!!">+</a>\
                        <input type="text" value="%%%" data-role="none" class="goodsCnt" style="width: 20px;float:left">\
                        <a style="text-decoration: none;folat:left" data-role="none" href="javascript:void(0)" class="DecCnt" data-code="!!!">-</a>\
                    </div></td></tr>'+
                    '<tr><td data-role="none" style="margin-left:-36px;"><div>\
                        <a data-code="&&&" class="rmGoods" href="javascript:void(0)" data-role="none" style="text-decoration: none;">移除</a>\
                    </div></td><td colspan="2"><div>\
                        <label class="totalPrice">总价：$$$</label>\
                    </div></td></tr><tr><td style="border-bottom: 1px solid green;" colspan="3"></td></tr>';
var operHtml = '<tr><td><span><lable>总价:</label></td><td><pre id="payFee"></pre></span></td></tr>\
                <tr><td><a href="pay.html" data-role="none" data-ajax="false">去结算</a></td></tr>';