using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Dal;
using QueryContract.cs;
using ResultView.cs;
using Tables;
using ToolHelp;
using System.Transactions;

namespace Bll
{
    public class GoodsHanlder
    {
        private readonly IGoodsHandler _goodsHandler=new GoodsHandlerDal();
        private readonly IRedisOper _redisOper = new RedisOper(new ReadCfg().GetRedisCfg());

        //入库
        public ResultStockIn GoodsStockIn(QueryGoodsStockIn query)
        {
            var result=new ResultStockIn {Exception=new MyException()};
            using (var scop=new TransactionScope())
            {
                try
                {
                    var table = new Tb_Goods
                    {
                        Code = query.Code,
                        LastUpDateTime = DateTime.Now,
                        Name = query.GoodsName,
                        InnerPrice = decimal.Parse(query.Price),
                        Numbers = query.StockInCnt
                    };
                    //_goodsHandler.StockIn(table);
                    var db = new MySqlContext();
                    db.TbGoods.Add(table);
                    db.SaveChanges();
                    var findIt = db.TbGoods.Where(x => x.Code.Equals(query.Code)).FirstOrDefault();
                    var tableInfo = new Tb_GoodsInfo()
                    {
                        GoodsId=findIt.Id,
                        SalePrice = query.SalePrice,
                        Discount = decimal.Parse(query.Discount)
                    };
                    _goodsHandler.InsertGoodsInfo(tableInfo);
                    //db.TbGoodsInfos.Add(tableInfo);
                    
                    scop.Complete();
                    result.Exception.Success = true;
                }
                catch (Exception ex)
                {
                    result.Exception.Success = false;
                    result.Exception.Exmsg = $"{ex.Message} {ex.StackTrace}";
                }
            }
               
            return result;
        }

        //搜索
        public ResultGoodsSearch SearchGoods(QuerySearchGoods query)
        {
            var result=new ResultGoodsSearch { Exception=new MyException()};
            if (!ValidateInput(query.GoodsName) && !string.IsNullOrEmpty(query.GoodsName))
            {
                result.Exception.Success = false;
                result.Exception.Exmsg = $"{IndexShow.输入不合法仅允许输入数字字母汉字下划线}";
                return result;
            }
            try
            {
                var whereStr=string.Empty;
                if (query.StockType != 0)
                {
                    whereStr = $" and StockType={query.StockType}";
                }
                var cacheGoodsResult = _redisOper.GetHash<IList<GoodsDisplayInfo>>("AllGoods");
                if (cacheGoodsResult==null)
                {
                    var allGoodsList=_goodsHandler.GetGoodsInfoByName(whereStr);
                    _redisOper.HSet("AllGoods",allGoodsList);
                }
                var cacheList = _redisOper.GetHash<IList<GoodsDisplayInfo>>("AllGoods");
                var search1 = !string.IsNullOrEmpty(query.GoodsName)?(from p in cacheList where p.GoodsName.Contains(query.GoodsName) select p).ToList():cacheList;
                if (query.StockType == 6) {
                    var discountList = _goodsHandler.GetDiscountInfo();
                    search1 = (from p in search1 where discountList.Contains(p.Code) select p).ToList();
                    cacheList = search1;
                }
                var goodsDisplayInfos = search1.ToList();
                result.GoodsList = !string.IsNullOrEmpty(query.GoodsName)
                    ? goodsDisplayInfos.Skip(query.PageNum*(query.PageIndex-1)).Take(query.PageNum).ToList()
                    : cacheList.Skip(query.PageNum * (query.PageIndex - 1)).Take(query.PageNum).ToList();
                result.PageIndex = query.PageIndex;
                result.PageTotal = goodsDisplayInfos.Count/query.PageNum + (goodsDisplayInfos.Count%query.PageNum > 0 ? 1: 0);
                result.Exception.Success = true;
            }
            catch (Exception ex)
            {
                result.Exception.Success = false;
                result.Exception.Exmsg = $"{ex.Message}";
            }
            return result;
        }

        public ResultAddToCart AddToCart(QueryAddCart query)
        {
            var result=new ResultAddToCart{Exception=new MyException()};
          
            try
            {
                var valiResult = ValidateAddCart(query.Code, query.Count);
                if (!valiResult.Equals("1"))
                {
                    result.Exception.Success = false;
                    result.Exception.Exmsg = valiResult;
                    return result;
                }
                query.Uid = query.Uid.Substring(1, query.Uid.Length - 2);
                var realSession = string.Empty;
                if (!ValidateClient(query.Uid, ref realSession))
                {
                    result.Exception.Success = false;
                    result.Exception.Exmsg = "02";
                    return result;
                }
                var whereStr = $" and good.Code={query.Code}";
                var res = _goodsHandler.GetGoodsInfoByName(whereStr);
                
                {
                    var cartGo = new Tb_CartGo {UserId= realSession, StockCode = query.Code, BuyCnt = query.Count, CurPrice = decimal.Parse(res[0].Price),CreateTime=DateTime.Now };
                    var sessionArr=GetSession(realSession);
                    cartGo.TempUser = 1;
                    cartGo.UserId = realSession;//realSession.Split('_')[0];
                    if (sessionArr.Length == 3)
                    {
                        cartGo.TempUser = 0;
                        cartGo.UserId = sessionArr.GetValue(2).ToString();
                    }
                    _goodsHandler.AddStockToCart(cartGo);
                    result.Exception.Success = true;
                }
               
            }
            catch (Exception ex)
            {
                result.Exception.Success = false;
                result.Exception.Exmsg = $"{ex.Message}";
                AppLogger.Error($"{ex.StackTrace} {query.Uid} {query.Code} {query.Count}");
            }
            return result;
        }

        //验证搜素输入非法
        private bool ValidateInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return true;
            const string pattern = "[\u4e00-\u9fa5_a-zA-Z0-9_]{1,10}";
            return Regex.IsMatch(input, pattern);
        }

        private string ValidateAddCart(string code, int buyCount)
        {
            if (buyCount <= 0)
                return $"{IndexShow.购买数量不允许小于0}";
            var whereStr = $" and good.Code={code}";
            var res=_goodsHandler.GetGoodsInfoByName(whereStr);
            if (res == null)
                return $"{IndexShow.商品部存在了}";
            if(res.Count==0)
                return $"{IndexShow.商品部存在了}";
            if (string.IsNullOrEmpty(res[0].Price))
                return $"{IndexShow.商品未上架}";
            return "1";
        }

        private bool ValidateClient(string key, ref string realKey)
        {
            realKey = new EncryDecry().Md5Decrypt(key);
            var vale=_redisOper.Get(realKey);
            return vale != null;
        }

        private Array GetSession(string realSession)
        {
            var val = _redisOper.Get(realSession);
            return val?.Split('_');
        }

       
    }
    public class EncryDecry
    {
        private string _sKey;

        public EncryDecry()
        {
            _sKey = GenerateKey();
        }
        // 创建Key
        string GenerateKey()
        {
            return "_z^7@z8!";
        }
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

}
