$(document).ready(function () {
    function index() {
        this.vid = 0;
        this.page = 1;
        this.pageNum = 4;
        this.finished = 0;
        this.sover = 0;
        this.pageTotal = 20;

        this.bindEvent();
        this.initUi();
    }

    index.prototype = {
        bindEvent: function () {
            var self = this;
            //搜素
            $(document).on('click', '#anylink', function () {
                $("#goods_List").children().remove();
                self.search();
            });
            //数量加 减
            $(document).on('click', '.IncrCnt,.DecCnt', function () {
                var cnt;
                if ($(this).attr('class') === 'IncrCnt') {
                    var txt=$(this).parent().next();
                    cnt = parseInt($($(txt).children("input")[0]).val());
                    cnt = cnt + 1;
                    $($(txt).children("input")[0]).val(cnt);
                } else {
                    var txt2 = $(this).parent().prev();
                    cnt = parseInt($($(txt2).children("input")[0]).val());
                    cnt = cnt - 1;
                    cnt = cnt <= 0 ? 1 : cnt;
                    $($(txt2).children("input")[0]).val(cnt);
                }

            });
            //添加购物车
            $(document).on('click', '.cartBtn', function () {
                var butCnt = $($(this).parent().prev().prev()).children("input").val();
                var code = $(this).parent().parent().parent().attr('data-mark');
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
                            ShowInfo("ssid 分配错误");
                        }
                    });
                }
                var dataJson = {};
                dataJson.Uid = ssid;
                try {
                    dataJson.Count = parseInt(butCnt);
                } catch (e) {
                    dataJson.Count = 1;
                }

                dataJson.Code = code;
                $.ajax({
                    url: '../../../api/GoodsAddCart/AddGoodsToCart',
                    type: 'POST',
                    dataType: 'json',
                    data: dataJson,
                    beforeSend: showLoader(),
                    success: function (data) {
                        hideLoader();
                        data = JSON.parse(data);
                        if (data.Exception.Success) {
                            ShowInfo("成功添加");
                        } else {
                            ShowInfo("添加失败");
                            alert(data.Exception.Exmsg);
                            if (data.Exception.Exmsg === "02") {
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
                        }
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
            //页面滚动执行事件
            $(window).scroll(function () {
                self.loadmore($(this));
            });
            //如果屏幕未到整屏自动加载下一页补满
            var setdefult = setInterval(function () {
                if (self.sover === 1)
                    clearInterval(setdefult);
                else if ($("#goods_List").height() < $(window).height())
                    self.loadmore($(window));
                else
                    clearInterval(setdefult);
            }, 500);
        }
        , getQueryParam: function () {
            var self = this;
            var json = {};
            $("#goods_List").html();
            json.GoodsName = "";
            if ($('#sName').val() != "") {
                json.GoodsName = $('#sName').val();
            }
            json.PageIndex = self.page;
            json.PageNum = self.pageNum;
            return json;
        }
        , search: function () {
            var self = this;
            var json = self.getQueryParam();
            $.ajax({
                url: '../../../Api/GoodsOper/GoodsSearch',
                type: "POST",
                data: json,
                success: function (data, textStatus) {
                    var obj = JSON.parse(data);
                    if (obj.Exception.Success) {
                        self.pageTotal = obj.PageTotal;
                        self.page += 1;
                        for (var i = 0; i < obj.GoodsList.length; i++) {
                            if (i % 2 === 0) {
                                var r = leftGoodsHtml.replace("@@@", obj.GoodsList[i].Code);
                                r = r.replace("###", obj.GoodsList[i].GoodsName);
                                r = r.replace("$$$", obj.GoodsList[i].Price);
                                r = r.replace("!!!", obj.GoodsList[i].Code);
                                r = r.replace("!@", obj.GoodsList[i].SaleCount);
                                $("#goods_List").append(r);
                            } else {
                                var l = rightGoodsHtml.replace("@@@", obj.GoodsList[i].Code);
                                l = l.replace("###", obj.GoodsList[i].GoodsName);
                                l = l.replace("$$$", obj.GoodsList[i].Price);
                                l = l.replace("!@", 12);
                                $("#goods_List").append(l);
                            }
                        }
                    } else {
                        ShowInfo("eror");
                    }
                },
                error: function (data, status, e) {
                    ShowInfo("eror");
                }
            });
        }
       , loadmore: function (obj) {
           var self = this;
           if (self.finished === 0 && self.sover === 0) {
               var scrollTop = $(obj).scrollTop();
               var scrollHeight = $(document).height();
               var windowHeight = $(obj).height();

               if ($(".loadmore").length === 0) {
                   var txt = '<div class="loadmore"><span class="loading"></span>加载中..</div>';
                   $("#goods_List").append(txt);
               }
               if (scrollTop + windowHeight - scrollHeight <= 50) {
                   //此处是滚动条到底部时候触发的事件，在这里写要加载的数据，或者是拉动滚动条的操作
                   //防止未加载完再次执行
                   self.finished = 1;
                   var json = self.getQueryParam();
                   $.ajax({
                       url: '../../../Api/GoodsOper/GoodsSearch',
                       type: "POST",
                       data: json,
                       success: function (data, textStatus) {
                           var obj = JSON.parse(data);
                           if (obj.Exception.Success) {
                               self.pageTotal = obj.PageTotal;
                               for (var i = 0; i < obj.GoodsList.length; i++) {
                                   if (i % 2 === 0) {
                                       var r = leftGoodsHtml.replace("@@@", obj.GoodsList[i].Code);
                                       r = r.replace("###", obj.GoodsList[i].GoodsName);
                                       r = r.replace("$$$", obj.GoodsList[i].Price);
                                       r = r.replace("!!!", obj.GoodsList[i].Code);
                                       r = r.replace("!@", obj.GoodsList[i].SaleCount);
                                       $("#goods_List").append(r);
                                   } else {
                                       var l = rightGoodsHtml.replace("@@@", obj.GoodsList[i].Code);
                                       l = l.replace("###", obj.GoodsList[i].GoodsName);
                                       l = l.replace("$$$", obj.GoodsList[i].Price);
                                       l = l.replace("!!!", obj.GoodsList[i].Code);
                                       l = l.replace("!@", obj.GoodsList[i].SaleCount);
                                       $("#goods_List").append(l);
                                   }
                               }
                           } else {
                               alert("eror")
                           }
                       },
                       error: function (data, status, e) {
                           ShowInfo("eror");
                       }
                   });
                   setTimeout(function () {
                       $(".loadmore").remove();
                       //$('#goods_List').append(result);
                       self.page += 1;
                       self.finished = 0;
                       //最后一页
                       if (self.page === self.pageTotal) {
                           self.sover = 1;
                           self.loadover();
                       }
                   }, 1000);
               }
           }
       }
        , loadover: function () {
            if (self.sover === 1) {
                var overtext = "没有更多了... ...";
                $(".loadmore").remove();
                if ($(".loadover").length > 0) {
                    $(".loadover span").eq(0).html(overtext);
                }
                else {
                    var txt = '<div class="loadover"><span>' + overtext + '</span></div>';
                    $("goods_List").append(txt);
                }
            }
        }
    };
    var obj = new index();

});

var leftGoodsHtml = '<div data-descr="一个单元" class="goods_left" data-mark="!!!">\
                    <div data-descr="图" style="min-height:150px">\
                        <a href="javascript:void(0)" data-role="none"><img src="/images/goods/@@@.jpg" /></a>\
                    </div>\
                    <div> \
                         <span>###</span>\
                         <span>$$$</span>\
                    </div>\
                    <div data-descr="操作">\
                         <div style="background-color: lightblue;width:40px;height:40px; font-size: 40px;border:2px solid;float:left;text-align:center;" data-role="none"><a class="IncrCnt" href="javascript:void(0)" data-role="none" style="text-decoration: none;">+</a></div>\
                        <div style="width:40px;height:40px; font-size: 40px;border:2px solid;float:left;font-size: 30px;border-left: none;border-right: none" data-role="none"><input type="text" class="goodsCnt" data-role="none" value="1" style="border:0px;"/></div>\
                        <div style="background-color: lightblue;width:40px; height:40px;font-size: 40px;border:2px solid;float:left;text-align:center;" data-role="none"><a class="DecCnt" href="javascript:void(0)" data-role="none" style="text-decoration: none;">-</a></div>\
                        <div style="float:left;" data-role="none"><a data-role="none" href="javascript:void(0)" class="cartBtn">购物车</a><label>!@份</label></div>\
                    </div>\
                </div>';

var rightGoodsHtml = '<div data-descr="一个单元" class="goods_right" data-mark="!!!">\
                    <div data-descr="图" style="min-height:150px">\
                        <a href="javascript:void(0)" data-role="none"><img src="/images/goods//@@@.jpg" /></a>\
                    </div>\
                    <div><span>###</span>\
                        <span>$$$</span>\</div>\
                    <div data-descr="操作">\
                        <div style="background-color: lightblue;width:40px;height:40px; font-size: 40px;border:2px solid;float:left;" data-role="none"><a class="IncrCnt" href="javascript:void(0)" data-role="none" style="text-decoration: none;">+</a></div>\
                        <div style="width:40px;height:40px; font-size: 40px;border:2px solid;float:left;font-size: 30px;border-left: none;border-right: none" data-role="none"><input type="text" class="goodsCnt" data-role="none" value="1" style="border:0px;"/></div>\
                       <div style="background-color: lightblue;width:40px;height:40px; font-size: 40px;border:2px solid;float:left;text-align:center;" data-role="none"><a class="DecCnt" href="javascript:void(0)" data-role="none" style="text-decoration: none;">-</a></div>\
                        <div style="float:left;" data-role="none"><a data-role="none" href="javascript:void(0)" class="cartBtn">购物车</a><label>!@份</label></div>\
                    </div>\
                </div>';