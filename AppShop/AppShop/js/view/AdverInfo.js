$(document).ready(function() {
    function AdverInfo() {
        this.ssid = "";
        this.bindEvent();
        this.initUi();
    }

    AdverInfo.prototype = {
        bindEvent:function() {
            var self = this;
            self.ssid = self.getCookieW();
            //展开时，指定的回调函数 
           

        }
        , initUi:function() {
            var self = this;
            var queryParam = {};
            queryParam.Ssid = self.ssid;
            $.ajax({
                url: "../../../api/UserSid",
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
            self.getPostInfo();
        }
        ,getPostInfo:function() {
            var self = this;
            $.ajax({
                url: "../../../api/GetPostInfoU/GetPostInfo",
                type: "POST",
                dataType: "json",
                beforeSend: showLoader(),
                data:null,
                success: function (json) {
                    hideLoader();
                    var data = JSON.parse(json);
                    if (data.Exception.Success) {
                       for (var i = 0; i < data.InfoList.length; i++) {
                           var html = htmlTpl.replace("#title#", data.InfoList[i].Title);
                           html = html.replace("#user#", data.InfoList[i].User);
                           html = html.replace("#time#", data.InfoList[i].Time);
                           html = html.replace("#count#", data.InfoList[i].Count);
                           html = html.replace("#cotent#", data.InfoList[i].Content);
                           html = html.replace("#Id#", data.InfoList[i].Id);
                           $("#infoList").append(html).trigger("create");
                           $($(".viewCount").find("a")[i]).on('click', function () {
                               var obj = $(this).parent().parent();
                               self.AddClick($(obj).attr("data-id"));
                           });
                       }
                      
                    } else {
                        ShowInfo2(data.Exception.Exmsg);
                    }
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
        , getCheckCode: function () {
            var dataParam = {};
        }
        ,AddClick:function(id) {
            if (id == null)
                return;
            var json = {};
            json.ID = id;
            $.ajax({
                url: "../../../api/AddtPostView/AddClick",
                type: "POST",
                dataType: "json",
                //beforeSend: showLoader(),
                data: json,
                success: function (json) {
                    //hideLoader();
                    var data = JSON.parse(json);
                    if (data.Exception.Success) {
                      

                    } else {
                        ShowInfo2(data.Exception.Exmsg);
                    }
                },
                error: function () {
                    ShowInfo2("出错啦亲");
                }
            });
        }
    };

    var payObj = new AdverInfo();
});

var htmlTpl = ' <div data-role="collapsible" class="viewCount" data-id="#Id#">\
                    <h6>#title#\
                        <p> #user# 发表于 #time# 浏览#count#</p></h6>\
                    <p>#cotent#</p>\
                </div>';