using System;
using System.Collections.Generic;
using System.Transactions;
using Dal;
using QueryContract.cs;
using ResultView.cs;
using Tables;
using ToolHelp;

/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/4/23 21:25:07
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/
namespace Bll
{
    public class CartgoBll
    {
        private readonly IRedisOper _redisOper = new RedisOper(new ReadCfg().GetRedisCfg());
        private readonly IUserCartgo _userCartgo = new UserCartgo();
        private readonly IUserInfo _userInfo = new UserInfo();

        public ResultUserCartgo ShowUserCartgo(QueryUserCartGo query)
        {
            var result = new ResultUserCartgo { Exception = new MyException(), CartGoAll = new UserCartGoAll() };
            try
            {
                if (!ValidateUserSsid(query.Ssid))
                {
                    result.Exception.Success = false;
                    return result;
                }
                //根据uid查找数据库表 连接货物信息表
                var realSsid = new EncryDecry().Md5Decrypt(query.Ssid);
                var sessionArr = GetSession(realSsid);
                if (sessionArr.Length == 3)
                {
                    realSsid = sessionArr.GetValue(2).ToString();
                }
                var cartgoInfo = _userCartgo.GetUserCartgoInfo(realSsid);
                result.CartGoAll.AllGoodsInCartgo = cartgoInfo;
                foreach (var info in cartgoInfo)
                {
                    result.CartGoAll.TotalPayment += info.TotalPrice;
                }
                result.Exception.Success = true;
            }
            catch (Exception ex)
            {
                result.Exception.Success = false;
                result.Exception.Exmsg = ex.Message;
            }
            return result;
        }

        public ResultRmUserCartgo RmUserCartgo(QueryRmCartgo query)
        {
            var result = new ResultRmUserCartgo { Exception = new MyException() };
            try
            {
                if (!ValidateUserSsid(query.Ssid))
                {
                    result.Exception.Success = false;
                    return result;
                }
                var realSsid = GetRealSsid(query.Ssid);
                _userCartgo.RmUserCartgo(realSsid, query.Code);
                result.Exception.Success = true;
            }
            catch (Exception ex)
            {
                result.Exception.Success = false;
                result.Exception.Exmsg = ex.Message;
            }
            return result;
        }

        public ResultPayGoods GetPayGoodsList(QueryPayGood query)
        {
            var result = new ResultPayGoods { Exception = new MyException(), GoodsList = new List<PayGoodsList>() };
            try
            {
                var realSsid = GetRealSsid(query.Ssid);
                if (string.IsNullOrEmpty(realSsid)) throw new Exception(IndexShow.客户端被篡改.ToString());
                var sessionArr = GetSession(realSsid);
                if (sessionArr == null) throw new Exception(IndexShow.redis失效了.ToString());
                if (sessionArr.Length == 3)
                {
                    realSsid = sessionArr.GetValue(2).ToString();
                }
                var cartGo = _userCartgo.GetUserCartgoInfo(realSsid);
                if (sessionArr.Length == 3)
                {
                    var userInfo = _userInfo.GeTbUserInfos(sessionArr.GetValue(2).ToString());
                    result.Phone = new EncryDecryPhone().Md5Decrypt(userInfo?[0].Phone1);
                    result.Address = userInfo?[0].Address;
                }
                var payFee = 0m;
                foreach (var item in cartGo)
                {
                    result.GoodsList.Add(new PayGoodsList
                    {
                        GoodsName = item.GoodsName,
                        GoodsPrice = $"{item.CurPrice}×{item.BuyCnt}={item.TotalPrice}",
                        GoodsCode = item.StockCode
                    });
                    payFee += item.TotalPrice;
                }
                result.PayFee = $"{payFee}";
                result.Exception.Success = true;
            }
            catch (Exception ex)
            {
                result.Exception.Success = false;
                result.Exception.Exmsg = ex.Message;
            }
            return result;
        }

        public ResultRmUserCartgo AddBuyCnt(QueryAddBuyCnt query)
        {
            var result = new ResultRmUserCartgo { Exception=new MyException()};
            try
            {
                if (!ValidateUserSsid(query.Ssid))
                {
                    result.Exception.Success = false;
                    return result;
                }
                var realSsid = new EncryDecry().Md5Decrypt(query.Ssid);
                var sessionArr = GetSession(realSsid);
                if (sessionArr.Length == 3)
                {
                    realSsid = sessionArr.GetValue(2).ToString();
                }
                _userCartgo.AddBuyCnt(realSsid, query.Code, query.Cnt);
                result.Exception.Success = true;
            }catch(Exception ex)
            {
                result.Exception.Success = false;
                AppLogger.Error($"{ex.Message} {ex.StackTrace}");
            }
            return result;
        }

        private bool ValidateUserSsid(string ssid)
        {
            if (string.IsNullOrEmpty(ssid))
                return false;
            var res = new EncryDecry().Md5Decrypt(ssid);
            if (_redisOper.Get(res) == null)
                return false;
            return true;
        }

        private string GetRealSsid(string querySsid)
        {
            try
            {
                return new EncryDecry().Md5Decrypt(querySsid);
            }
            catch (Exception ex)
            {
                AppLogger.Error($"{querySsid} {ex.Message} {ex.StackTrace}");
                return string.Empty;
            }
        }

        private Array GetSession(string realSession)
        {
            var val = _redisOper.Get(realSession);
            return val?.Split('_');
        }
    }

    public class OrderBll
    {
        private readonly IUserCartgo _userCartgo = new UserCartgo();
        private readonly IRedisOper _redisOper = new RedisOper(new ReadCfg().GetRedisCfg());

        public ResultSubmitOrder SubimtOrder(QueryCreateOrder query)
        {
            var result = new ResultSubmitOrder { Exception = new MyException() };
            try
            {
                result.Exception.Success = false;
                if (!ValidateUserSsid(query.Ssid) || !ValidateCheckCode(new EncryDecryPhone().Md5Encrypt(query.Phone), query.CheckCode))
                {
                    return result;
                }

                var userSsid = GetRealSsid(query.Ssid);
                var orderId = GetOrderId(query.Phone);
                var session = GetSession(userSsid);
                var userRegName= session.Length == 3 ? $"{session.GetValue(2)}" : userSsid; 
                using (var ts = new TransactionScope())
                {
                    try
                    {
                        //更新cartgo
                        _userCartgo.SubmitOrder(userRegName, orderId);
                        //插入订单表
                        var order = new Tb_Order
                        {
                            Address = query.Address,
                            CreateTime = DateTime.Now,
                            PhoneNumber = query.Phone,
                            OrderId = orderId,
                            AreaId = int.Parse(query.AreaId)
                        };

                        order.UserId = userRegName;
                        _userCartgo.AddOrder(order);
                        result.OrderId = orderId;
                        result.Exception.Success = true;
                        ts.Complete();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Exception.Success = false;
                result.Exception.Exmsg = ex.Message;
            }
            return result;
        }

        private string GetOrderId(string phoneNumber)
        {
            var dateStr = DateTime.Now.ToString("yyyyMMddHHmmss");
            var phoneId = phoneNumber.Substring(phoneNumber.Length - 4, 4);
            return $"{dateStr}{phoneId}";
        }
        private bool ValidateUserSsid(string ssid)
        {
            if (string.IsNullOrEmpty(ssid))
                return false;
            var res = new EncryDecry().Md5Decrypt(ssid);
            if (_redisOper.Get(res) == null)
                return false;
            return true;
        }
        private string GetRealSsid(string querySsid)
        {
            return new EncryDecry().Md5Decrypt(querySsid);
        }

        private Array GetSession(string realSession)
        {
            var val = _redisOper.Get(realSession);
            return val?.Split('_');
        }

        //后台验证验证码
        public bool ValidateCheckCode(string phoneNumber, string checkCode)
        {
            var val = _redisOper.Get(phoneNumber);
            var arr = val.Split('_');
            var createTime = DateTime.Parse(arr[0]);
            return (DateTime.Now - createTime).TotalMinutes <= 100 && checkCode.Equals(arr[1]);
        }
    }

    public class AreaBll
    {
        private readonly ISaleArea _saleArea = new SaleArea();
        public ResultArea GetAllArea()
        {
            var result = new ResultArea { Exception = new MyException(), Areas = new List<AreaCode>() };
            try
            {
                var areas = _saleArea.GeTbSaleAreas();
                foreach (var area in areas)
                {
                    result.Areas.Add(new AreaCode { Id = area.Id, Name = area.Area });
                }
                result.Exception.Success = true;
            }
            catch (Exception ex)
            {
                result.Exception.Success = false;
                AppLogger.Error($"{ex.Message} {ex.StackTrace}");
            }
            return result;
        }
    }
}
