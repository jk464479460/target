$(document).ready(function() {
    function LoginIt() {
        this.ssid = "";
      
        this.bindEvent();
    }
    LoginIt.prototype = {
        bindEvent:function() {
            var self = this;
            self.ssid = self.getCookieW();
            alert(self.ssid)
          
             $(document).on('click', '#btnLogin', function () {
                 var dataParam = self.getParam();
                 if (dataParam === {}) {
                     return;
                 }
                 $.ajax({
                     url: "../../../api/user/Login",
                     type: "POST",
                     dataType: "json",
                     data:dataParam,
                     beforeSend: showLoader(),
                     success: function (json) {
                         hideLoader();
                         json = JSON.parse(json);
                         if (json.Exception.Success) {
                             location.href = '../index.html';
                         } else {
                            
                         }
                     },
                     error:function() {
                         
                     }
                 });
             });
        }
        ,initUi:function() {
            
        }
        , getParam() {
            var self = this;
            var userName = $("#username").val();
            var pass = $("#password").val();
            if (userName === "" || pass === "") {
                return {};
            }
            var json = {};
            json.UserName = userName;
            json.Password = pass;
            json.Ssid = self.ssid;
            return json;
        }
        , getCookieW: function () {
              var cookie = getCookie("UID");
              if (cookie === null) location.href = "../index.html";
              return cookie;
          }
    }
    var loginObj = new LoginIt();
   
});

