/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/4/28 17:13:17
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/

using Top.Api;
using Top.Api.Request;

namespace SendMessageToCustom
{
    public interface IAliMessage
    {
        string SendMessage(string checkCode, string phoneNum = "15295739525");
        string RegMessage(string checkCode, string phoneNum = "15295739525");
    }
    public class AliMessage: IAliMessage
    {
        public string SendMessage(string checkCode,string phoneNum= "15295739525")
        {
            var url = "http://gw.api.taobao.com/router/rest";
            //成为开发者，创建应用后系统自动生成
            var appkey = "23347464";
            var secret = "735a20d75db9ec92a18952613a27da42";
            var client = new DefaultTopClient(url, appkey, secret);
            var req = new AlibabaAliqinFcSmsNumSendRequest
            {
                Extend = "丰利家",
                SmsType = "normal",
                SmsFreeSignName = "身份验证",
                SmsParam = "{\"code\":\"####\",\"product\":\"丰利家\"}",
                RecNum = phoneNum,
                SmsTemplateCode = "SMS_7755876"
            };
          
            req.SmsParam = req.SmsParam.Replace("####", checkCode);
            var rsp = client.Execute(req);
            return rsp.Body;
        }

        public string RegMessage(string checkCode, string phoneNum = "15295739525")
        {
            var url = "http://gw.api.taobao.com/router/rest";
            //成为开发者，创建应用后系统自动生成
            var appkey = "23347464";
            var secret = "735a20d75db9ec92a18952613a27da42";
            var client = new DefaultTopClient(url, appkey, secret);
            var req = new AlibabaAliqinFcSmsNumSendRequest
            {
                Extend = "丰利家",
                SmsType = "normal",
                SmsFreeSignName = "注册验证",
                SmsParam = "{\"code\":\"####\",\"product\":\"丰利家\"}",
                RecNum = phoneNum,
                SmsTemplateCode = "SMS_7755872"
            };

            req.SmsParam = req.SmsParam.Replace("####", checkCode);
            var rsp = client.Execute(req);
            return rsp.Body;
        }
    }
}
