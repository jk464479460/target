$(document).ready(function () {
    function index() {
        this.bindEvent();
        this.initUi();
    }

    index.prototype = {
        bindEvent: function () {
            var self = this;
            //搜素
            $(document).on('click', '#anylink', function() {
                $("#goods_List").children().remove();
                self.search();
            });
            //数量加 减
            $(document).on('click', '.IncrCnt,.DecCnt', function () {
                var cnt;
                if ($(this).attr('class') === 'IncrCnt') {
                    cnt = parseInt($(this).next().val());
                    cnt = cnt + 1;
                    $(this).next().val(cnt);
                } else {
                    cnt = parseInt($(this).prev().val());
                    cnt = cnt - 1;
                    $(this).prev().val(cnt);
                }

            });
            //添加购物车
            $(document).on('click', '.cartBtn', function () {
                var butCnt=$(this).prev().prev().val();
                var code = $(this).parent().parent().attr('data-mark');
                var ssid = getCookie('UID');
                if (ssid === null) {
                    $.ajax({
                        url: '../../../Api/Cookie/ApplyGruid',
                        dataType: 'json',
                        type: "POST",
                        success: function (json) {
                            setCookie("UID", json);
                            ssid = json;
                        },
                        error: function () {

                        }
                    });
                }
                var dataJson = {};
                dataJson.Uid = ssid;
                dataJson.Count = parseInt(butCnt);
                dataJson.Code = code;
                $.ajax({
                    url: '../../../api/GoodsAddCart/AddGoodsToCart',
                    type: 'POST',
                    dataType:'json',
                    data: dataJson,
                    beforeSend: showLoader(),
                    success: function (data) {
                        hideLoader();
                        data=JSON.parse(data);
                        if(data.Exception.Success) {
                            ShowInfo("成功添加");
                        } else { ShowInfo("添加失败"); }
                    },
                    error: function () {
                        ShowInfo("添加失败了亲");
                    }
                });

            });
        },
        initUi: function () {
            var self = this;
            if (getCookie('UID') === null) {
                $.ajax({
                    url: '../../../Api/Cookie/ApplyGruid',
                    dataType: 'json',
                    type: "POST",
                    success: function (json) {
                        setCookie("UID", json);
                    },
                    error: function () {

                    }
                });
            }
            self.search();
        }
        , search: function () {
            var json = {};
            $("#goods_List").html();
            json.GoodsName = "";
            if ($('#sName').val() != "") {
                json.GoodsName = $('#sName').val();
            }
            $.ajax({
                url: '../../../Api/GoodsOper/GoodsSearch',
                type: "POST",
                data: json,
                beforeSend: showLoader(),
                success: function (data, textStatus) {
                    hideLoader();
                    var obj = JSON.parse(data);
                    if (obj.Exception.Success) {
                        for (var i = 0; i < obj.GoodsList.length; i++) {
                            if (i % 2 === 0) {
                                var r = leftGoodsHtml.replace("@@@", obj.GoodsList[i].Code);
                                r = r.replace("###", obj.GoodsList[i].GoodsName);
                                r = r.replace("$$$", obj.GoodsList[i].Price);
                                r = r.replace("!!!", obj.GoodsList[i].Code);
                                $("#goods_List").append(r);
                            } else {
                                var l = rightGoodsHtml.replace("@@@", obj.GoodsList[i].Code);
                                l = l.replace("###", obj.GoodsList[i].GoodsName);
                                l = l.replace("$$$", obj.GoodsList[i].Price);
                                l = l.replace("!!!", obj.GoodsList[i].Code);
                                $("#goods_List").append(l);
                            }
                        }
                    } else {
                       alert("eror")
                    }
                },
                error: function (data, status, e) {
                    alert("eror")
                }
            });
        }
       
        //,addToCart:function() {
        //    //根据code、数量传递进入参数
        //}
    };
    var obj = new index();
    
});

var leftGoodsHtml = '<div data-descr="一个单元" class="goods_left" data-mark="!!!">\
                    <div data-descr="图">\
                        <a href="javascript:void(0)" data-role="none"><img src="/images/goods/@@@.jpg" /></a>\
                    </div>\
                    <div data-descr="操作">\
                        <span>###</span>\
                        <span>$$$</span>\
                        <a class="IncrCnt" href="javascript:void(0)" data-role="none" style="text-decoration: none; font-size: 36px">+</a>\
                        <input type="text" class="goodsCnt" style="width: 20px" data-role="none" value="1"/>\
                        <a class="DecCnt" href="javascript:void(0)" data-role="none" style="text-decoration: none; font-size: 36px">-</a>\
                        <a data-role="none" href="javascript:void(0)" class="cartBtn">购物车</a>\
                    </div>\
                </div>';

var rightGoodsHtml = '<div data-descr="一个单元" class="goods_right" data-mark="!!!">\
                    <div data-descr="图">\
                        <a href="javascript:void(0)" data-role="none"><img src="/images/goods//@@@.jpg" /></a>\
                    </div>\
                    <div data-descr="操作">\
                        <span>###</span>\
                        <span>$$$</span>\
                        <a class="IncrCnt" href="javascript:void(0)" data-role="none" style="text-decoration: none; font-size: 36px">+</a>\
                        <input type="text" class="goodsCnt" style="width: 20px" data-role="none" value="1"/>\
                        <a class="DecCnt" href="javascript:void(0)" data-role="none" style="text-decoration: none; font-size: 36px">-</a>\
                        <a data-role="none" href="javascript:void(0)" class="cartBtn">购物车</a>\
                    </div>\
                </div>';