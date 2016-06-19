/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/5/2 9:35:10
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/

using System;
using System.Transactions;
using Dal;
using QueryContract.cs;
using ResultView.cs;
using Tables;
using ToolHelp;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace Bll
{
    public class UserLogin
    {
        private readonly IUser _user = new User();
        private readonly IUserInfo _userInfo = new UserInfo();
        private readonly IRedisOper _redisOper = new RedisOper(new ReadCfg().GetRedisCfg());
        private readonly IGoodsHandler _goodsHandler = new GoodsHandlerDal();
        private readonly IUserCartgo _userCartgo = new UserCartgo();
        private readonly ICartGoDal _cartGodal = new CartGoDal();

        public ResultLogin Login(QueryUserLogin query)
        {
            var result = new ResultLogin { Exception = new MyException() };
            try
            {
                if (Validate(query.UserName, query.Password) == false)
                {
                    result.Exception.Success = false;
                    result.Exception.Exmsg = "用户名或密码错误";
                    return result;
                }
                var findIt = _user.SearchUser(query.UserName);
                if (findIt == null)
                {
                    result.Exception.Success = false;
                    result.Exception.Exmsg = "用户名或密码错误";
                    return result;
                }
                var pwd = new EncryDecryUser().Md5Encrypt(query.Password);
                result.Exception.Success = findIt.Pwd.Equals(pwd);
                if (result.Exception.Success)
                {
                    var realSsid = new EncryDecry().Md5Decrypt(query.Ssid);
                    var arr = GetSession(realSsid);
                    _redisOper.Set(realSsid, $"1999_{DateTime.Now}_{findIt.Name}");//1999代表session
                    UpdateCartOrderInfo(query.UserName,realSsid);
                }
            }
            catch (Exception ex)
            {
                result.Exception.Success = false;
                result.Exception.Exmsg = ex.Message;
            }
            return result;
        }

        public ResultReg Add(QueryUserReg query)
        {
            var result = new ResultReg { Exception = new MyException() };
            var validateRes = ValidateAddUser(query.UName, query.CheckCode, query.Paw, query.Phone);
            if (validateRes != 0)
            {
                result.Exception.Success = false;
                result.Exception.Exmsg = $"{validateRes}";
                return result;
            }
            //using (var scope = new TransactionScope())
            {
                try
                {
                    var user = new Tb_User { HumanType = 0, IsPermit = 1, Name = query.UName, Pwd = new EncryDecryUser().Md5Encrypt(query.Paw) };
                    query.UName = query.UName.ToString();
                    _user.AddUser(user, query.Phone);
                    var findIt = _user.SearchUser(user.Name);
                    var userInfo = new Tb_UserInfo { UserId = findIt.Id, Address = "", Phone1 = new EncryDecryPhone().Md5Encrypt(query.Phone) };
                    _user.UpdateUserInfo(userInfo);

                    result.Exception.Success = true;
                    //scope.Complete();
                }
                catch (Exception ex)
                {
                    result.Exception.Success = false;
                    result.Exception.Exmsg = ex.Message;
                }
            }
            return result;
        }

        public ResultModifyAddress IsReg(QueryUserExists query)
        {
            var result = new ResultModifyAddress { Exception = new MyException() };
            try
            {
                var realSsid = new EncryDecry().Md5Decrypt(query.Ssid);
                var arr = GetSession(realSsid);
                if (arr.Length < 3)
                {
                    result.Exception.Exmsg = "not found";
                    result.Exception.Success = true;
                    return result;
                }
                var findIt = _user.SearchUser(arr.GetValue(2).ToString());
                if (findIt != null)
                {
                    result.Address = _userInfo.GeTbUserInfos($"{findIt.Id}")?[0].Address;
                }
                result.Exception.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"{ex.Message} {ex.StackTrace}");
                result.Exception.Success = false;
            }
            return result;
        }

        public ResultModifyAddress SaveAddress(QueryAdress query)
        {
            var result = new ResultModifyAddress { Exception = new MyException() };
            try
            {
                var realSsid = new EncryDecry().Md5Decrypt(query.Ssid);
                var arr = GetSession(realSsid);
                if (arr.Length < 3)
                {
                    result.Exception.Exmsg = "not found";
                    result.Exception.Success = true;
                    return result;
                }
                var findIt = _user.SearchUser(arr.GetValue(2).ToString());
                if (findIt == null)
                {
                    throw new NullReferenceException();
                }
                var info = _userInfo.GeTbUserInfos($"{findIt.Name}")?[0];
                if (info != null)
                {
                    info.Address = query.Address;
                    _userInfo.UpdateUserInfo(info);
                }
                result.Exception.Success = true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"{ex.Message} {ex.StackTrace}");
                result.Exception.Success = false;
            }
            return result;
        }

        public ResultLogin ModifyPass(QueryUserModify query)
        {
            var result = new ResultLogin { Exception = new MyException() };
            try
            {
                var realSsid = new EncryDecry().Md5Decrypt(query.Ssid);
                var arr = GetSession(realSsid);
                if (arr.Length < 3)
                {
                    result.Exception.Exmsg = "not found";
                    result.Exception.Success = true;
                    return result;
                }
                var findIt = _user.SearchUser(arr.GetValue(2).ToString());
                if (findIt == null)
                {
                    throw new NullReferenceException();
                }
                if (!findIt.Pwd.Equals(new EncryDecryUser().Md5Encrypt(query.OldPass)))
                {
                    result.Exception.Success = false;
                    result.Exception.Exmsg = "旧密码输入错误";
                    return result;
                }
                findIt.Pwd = new EncryDecryUser().Md5Encrypt(query.Paw);
                _user.UpdateUser(findIt);
                result.Exception.Success = true;
            }
            catch (Exception ex)
            {
                result.Exception.Success = false;
                AppLogger.Error($"{ex.Message} {ex.StackTrace}");
            }

            return result;
        }

        private bool Validate(string userName, string pass)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(pass))
            {
                return false;
            }
            return true;
        }

        private int ValidateAddUser(string userName, string code, string pass, string phone)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(phone))
                return 1;
            if (pass.Length < 6)
                return 2;
            if (phone.Length < 11)
                return 3;
            var val = _redisOper.Get(new EncryDecryPhone().Md5Encrypt(phone) + "_reg");
            var valArr = val.Split('_');
            var createTime = DateTime.Parse(valArr[0]);
            if ((DateTime.Now - createTime).TotalMinutes > 10)
                return 4;
            if (!valArr[1].Equals(code))
                return 5;
            var userExist = _user.SearchUser(userName);
            if (userExist != null)
                return 6;
            return 0;
        }

        private Array GetSession(string realSession)
        {
            var val = _redisOper.Get(realSession);
            return val?.Split('_');
        }

        private void UpdateCartOrderInfo(string uName,string realSessionId)
        {
            using (var scope=new TransactionScope())
            {
                try
                {
                    var cartgo = _cartGodal.GetCartGoByUser(realSessionId);
                    if (cartgo != null)
                    {
                        cartgo.UserId = uName;
                        _cartGodal.UpdateCartGoInfo(cartgo,realSessionId);
                        var order = _cartGodal.GetOrderByUser(realSessionId);
                        if (order != null)
                        {
                            order.UserId = uName;
                            _cartGodal.UpdateOrder(order);
                        }
                     
                    }
                    scope.Complete();
                }
                catch(Exception ex)
                {
                    AppLogger.Error($"{ex.Message} {ex.StackTrace}");
                }
            }
        }
    }

    public class UserRecommend
    {
        private readonly IUserRecommend _userRecommend = new UserRecommendDal();
        private readonly IUser _user = new User();
        private readonly IUserInfo _userInfo = new UserInfo();

        public ResultRecommend AddRecommend(QueryRecommendUser query)
        {
            var result = new ResultRecommend { Exception = new MyException() };
            try
            {
                var realSsid = new EncryDecry().Md5Decrypt(query.Ssid);
                var arr = HelpTool.GetSession(realSsid);
                var errNo = string.Empty;
                if (arr.Length < 3 || !CheckRecommenUser(query.User, new EncryDecryPhone().Md5Decrypt(query.Phone),ref errNo))
                {
                    result.Exception.Exmsg = errNo;
                    result.Exception.Success = false;
                    return result;
                }
                var user = arr.GetValue(2).ToString();
                if (user.Equals(query.User)){
                    result.Exception.Exmsg = "4";
                    result.Exception.Success = false;
                    return result;
                }
                var userRecommend = new Tb_Recommend
                {
                    RecommendUser = user,
                    User = query.User,
                    CDT = DateTime.Now
                };
                _userRecommend.AddRecommend(userRecommend);
                result.Exception.Success = true;
            }
            catch (Exception ex)
            {
                result.Exception.Success = false;
                AppLogger.Error($"{ex.Message} {ex.StackTrace}");
            }
            return result;
        }

        public ResultRecommend GetRecommendInfo(QueryGetRecommend query)
        {
            var result = new ResultRecommend { Exception = new MyException(),RecommendList=new List<RecommendUser>() };
            try
            {
                var realSsid = new EncryDecry().Md5Decrypt(query.Ssid);
                var arr = HelpTool.GetSession(realSsid);
                if (arr.Length < 3 )
                {
                    result.Exception.Exmsg = "not found";
                    result.Exception.Success = true;
                    return result;
                }
                var user = arr.GetValue(2).ToString();
                var recommendList = _userRecommend.GetAllRecommend(user);
                foreach(var rec in recommendList)
                {
                    result.RecommendList.Add(new RecommendUser { User = rec.User });
                }
                result.Exception.Success = true;
            }
            catch(Exception ex)
            {
                result.Exception.Success = false;
                AppLogger.Error($"{ex.Message} {ex.StackTrace}");
            }

            return result;
        }

        private bool CheckRecommenUser (string name,string phone,ref string err)
        {
            var result = true;
            var userInfo=_userInfo.GeTbUserInfos(name);
            if (userInfo == null)
            {
                err = "1";
                result = false;
            }
            if (userInfo.Count == 0)
            {
                err = "1";
                result = false;
            }
            if (!phone.Equals(userInfo[0].Phone1))
            {
                err = "2";
                result = false;
            }
            var findIt = _userRecommend.IsExists(name);
            if (findIt != null)
            {
                err = "3";
                result = false;
            }
            return result;
        }
    }

    public class HelpTool
    {
        private static IRedisOper _redisOper = new RedisOper(new ReadCfg().GetRedisCfg());
        public static Array GetSession(string realSession)
        {
            var val = _redisOper.Get(realSession);
            return val?.Split('_');
        }
    }

    public abstract class BaseEncryDecry
    {
        protected string _sKey;
        ///MD5加密
        public string Md5Encrypt(string pToEncrypt)
        {
            var des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.Default.GetBytes(pToEncrypt);
            des.Key = Encoding.ASCII.GetBytes(_sKey);
            des.IV = Encoding.ASCII.GetBytes(_sKey);
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            var ret = new StringBuilder();
            foreach (var b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();


        }

        ///MD5解密
        public string Md5Decrypt(string pToDecrypt)
        {
            var des = new DESCryptoServiceProvider();

            byte[] inputByteArray = new byte[pToDecrypt.Length / 2];
            for (var x = 0; x < pToDecrypt.Length / 2; x++)
            {
                int i = (Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            des.Key = Encoding.ASCII.GetBytes(_sKey);
            des.IV = Encoding.ASCII.GetBytes(_sKey);
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            var ret = new StringBuilder();
            return System.Text.Encoding.Default.GetString(ms.ToArray());
        }
    }
    public class EncryDecryUser: BaseEncryDecry
    {

        public EncryDecryUser()
        {
            _sKey = GenerateKey();
        }
        // 创建Key
        string GenerateKey()
        {
            return "q&@u#z9^";
        }
    }
    public class EncryDecryPhone : BaseEncryDecry
    {

        public EncryDecryPhone()
        {
            _sKey = GenerateKey();
        }
        // 创建Key
        string GenerateKey()
        {
            return "q&@w?~9+";
        }

    }
}
