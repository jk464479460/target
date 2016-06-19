/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/4/29 9:43:45
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/

using System;
using Dal;
using SendMessageToCustom;
using ToolHelp;

namespace Bll
{
    public class ValidateClient
    {
        private readonly IRedisOper _redisOper = new RedisOper(new ReadCfg().GetRedisCfg());
        private readonly IAliMessage _aliMessage=new AliMessage();
        private static readonly Random Rd= new Random(DateTime.Now.Second);

        //发送验证码到手机
        public string SendCheckCode(string phoneNumber)
        {
            try
            {
                var checkCode = $"{Rd.Next(1, 10)}{Rd.Next(1, 10)}{Rd.Next(1, 10)}{Rd.Next(1, 10)}";
                var req=_aliMessage.SendMessage(checkCode, phoneNumber);
                if (req.Contains("error"))
                {
                    return IndexShow.交易码发送系统故障了.ToString();
                }
                _redisOper.Set(new EncryDecryPhone().Md5Encrypt(phoneNumber), $"{DateTime.Now}_{checkCode}");
                return "ok";
            }
            catch (Exception ex)
            {
                AppLogger.Error($"{ex.Message} {ex.StackTrace}");
                return IndexShow.交易码发送系统故障了.ToString();
            }
        }
        //发送注册码到手机
        public string RegMessage(string phoneNumber)
        {
            try
            {
                var checkCode = $"{Rd.Next(1, 10)}{Rd.Next(1, 10)}{Rd.Next(1, 10)}{Rd.Next(1, 10)}";
                var req = _aliMessage.RegMessage(checkCode, phoneNumber);
                if (req.Contains("error"))
                {
                    return "error";
                }
                _redisOper.Set(new EncryDecryPhone().Md5Encrypt(phoneNumber) +"_reg", $"{DateTime.Now}_{checkCode}");
                return "ok";
            }
            catch (Exception ex)
            {
                AppLogger.Error($"{phoneNumber} {ex.Message} {ex.StackTrace}");
                return "error";
            }
        }


    }
}
