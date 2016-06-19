$(document).ready(function() {
    function orderShow() {
        this.initUi();
    }

    orderShow.prototype ={
        bindEvent:function() {

        }
        ,initUi:function() {
            var request = new Object();
            request = GetRequest();
            var orderId = request["orderId"];
            $("#orderCode").text(orderId);
        }
    };
    var objOrderShow = new orderShow();
});