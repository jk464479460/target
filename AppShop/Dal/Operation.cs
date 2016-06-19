using System;
using System.Collections.Generic;
using FrameWrok.Common;
using Tables;
using System.Linq;

namespace Dal
{
    //http://www.tuicool.com/articles/yiUry2
    public interface IGoodsHandler
    {
        //入库
        void StockIn(Tb_Goods goodsIn);
        //查询商品
        IList<GoodsDisplayInfo> GetGoodsInfoByName(string whereStr);
        void AddStockToCart(Tb_CartGo cartGo);
        List<string> GetDiscountInfo();
        Tb_Goods GetGoodsInfoByCode(string code);
        void InsertGoodsInfo(Tb_GoodsInfo goodsInfo);
    }

    public interface IUserCartgo
    {
        IList<UserCartGoInfo> GetUserCartgoInfo(string userSsid);
        void RmUserCartgo(string userSsid, string code);
        void SubmitOrder(string userSsid, string orderId);
        void AddOrder(Tb_Order order);
        void AddBuyCnt(string userSsid, string code, string cnt);
    }

    public interface IUserInfo
    {
        IList<Tb_UserInfo> GeTbUserInfos(string userSsid);
        void UpdateUserInfo(Tb_UserInfo info);
    }

    public interface IUser
    {
        Tb_User SearchUser(string userName);
        void AddUser(Tb_User user, string phone);
        void UpdateUserInfo(Tb_UserInfo userInfo);
        void UpdateUser(Tb_User user);
    }
    public class GoodsHandlerDal : IGoodsHandler
    {
        public IList<GoodsDisplayInfo> GetGoodsInfoByName(string whereStr)
        {
            var cmd=new DataCommand("mySql", "SearchGoodsByName");
            cmd.SetParameters("@whereStr", whereStr);
            return cmd.ExecuteSql<GoodsDisplayInfo>();
        }

        public void StockIn(Tb_Goods goodsIn)
        {
            var cmd = new DataCommand();
            var findIt= cmd.Search<Tb_Goods>(x=> x.Code.Equals(goodsIn.Code));
            if (findIt != null)
            {
                findIt.LastUpDateTime = DateTime.Now;
                findIt.Numbers += goodsIn.Numbers;
                DataCommand.Update(findIt);
            }
            cmd.Add<Tb_Goods>(goodsIn,false);
            //findIt = cmd.Search<Tb_Goods>(x => x.Code.Equals(goodsIn.Code));
            //var cmd2=new DataCommand("mySql", "InsertGoodsInfo2");
            //cmd2.SetParameters("@GoodsId",$"{ findIt.Id}");
            //cmd2.SetParameters("@SalePrice", $"{goodsIn.InnerPrice}");
            //cmd2.ExeSqlCmd();
        }

        public void AddStockToCart(Tb_CartGo cartGo)
        {
            var cmd = new DataCommand();
            var findIt = cmd.Search<Tb_CartGo>(x => x.StockCode.Equals(cartGo.StockCode) && x.UserId.Equals(cartGo.UserId) && x.IsPay!=1);
            if (findIt != null)
            {
                findIt.BuyCnt += cartGo.BuyCnt;
                findIt.CurPrice = cartGo.CurPrice;
                DataCommand.Update(findIt);
                return;
            }
            cmd=new DataCommand("mySql", "InsertGoodsCartGo");
            cmd.SetParameters("@UserId",cartGo.UserId);
            cmd.SetParameters("@StockCode", cartGo.StockCode);
            cmd.SetParameters("@BuyCnt", $"{cartGo.BuyCnt}");
            cmd.SetParameters("@CurPrice", $"{cartGo.CurPrice}");
            cmd.SetParameters("@TempUser",$"{cartGo.TempUser}");
            cmd.SetParameters("@CreateTime", cartGo.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.ExeSqlCmd();
        }

        public List<string> GetDiscountInfo()
        {
            var db = new MySqlContext();
            var idList=db.Set<Tb_GoodsInfo>().Where(x=>x.Discount!=1).Select(x=>x.GoodsId).ToList();
            var codeList = db.TbGoods.Where(x=>idList.Contains(x.Id)).Select(x=>x.Code).ToList();
            return codeList;
        }

        public Tb_Goods GetGoodsInfoByCode(string code)
        {
            var cmd = new DataCommand();
            var result=cmd.Search<Tb_Goods>(x => x.Code.Equals(code));
            return result;
        }

        public void InsertGoodsInfo(Tb_GoodsInfo goodsInfo)
        {
            var cmd = new DataCommand("mySql", "InsertGoodsInfo");
            cmd.SetParameters("@GoodsId",$"{goodsInfo.GoodsId}");
            cmd.SetParameters("@SalePrice",goodsInfo.SalePrice);
            cmd.SetParameters("@Discount",$"{goodsInfo.Discount}");
            cmd.ExeSqlCmd();
        }
      
        public void UpdateOrder(Tb_Order order)
        {
            DataCommand.Update<Tb_Order>(order);
        }
    }

    public class UserCartgo: IUserCartgo
    {
        public IList<UserCartGoInfo> GetUserCartgoInfo(string userSsid)
        {
            var cmd = new DataCommand("mySql", "GetUserCartgoInfo");
            cmd.SetParameters("@ssid", userSsid);
            return cmd.ExecuteSql<UserCartGoInfo>();
        }

        public void RmUserCartgo(string userSsid,string code)
        {
            var cmd = new DataCommand("mySql", "RmUserCartgo");
            cmd.SetParameters("@ssid", userSsid);
            cmd.SetParameters("@code",code);
            cmd.ExeSqlCmd();
        }
        public void SubmitOrder(string userSsid,string orderId)
        {
            var cmd = new DataCommand("mySql", "SubmitOrder");
            cmd.SetParameters("@ssid", userSsid);
            cmd.SetParameters("@orderId", orderId);
            cmd.ExeSqlCmd();
        }

        public void AddOrder(Tb_Order order)
        {
            var cmd = new DataCommand();
            cmd.Add(order,true);
        }

        public void AddBuyCnt(string userSsid,string code,string cnt)
        {
            var cmd = new DataCommand("mySql", "AddBuyCnt");
            cmd.SetParameters("@ssid",userSsid);
            cmd.SetParameters("@code",code);
            cmd.SetParameters("@cnt",$"{cnt}");
            cmd.ExeSqlCmd();
        }
    }
    public interface ICartGoDal
    {
        Tb_CartGo GetCartGoByUser(string Uid);
        void UpdateCartGoInfo(Tb_CartGo cartGo,string UpdateCartGoInfo);
        void UpdateOrder(Tb_Order order);
        Tb_Order GetOrderByUser(string Uid);
    }
    public class CartGoDal: ICartGoDal
    {
        public Tb_CartGo GetCartGoByUser(string Uid)
        {
            var cmd = new DataCommand();
            var cartGo=cmd.Search<Tb_CartGo>(x => x.UserId.Equals(Uid));
            return cartGo;
        }
        public Tb_Order GetOrderByUser(string Uid)
        {
            var cmd = new DataCommand();
            var cartGo = cmd.Search<Tb_Order>(x => x.UserId.Equals(Uid));
            return cartGo;
        }
        public void UpdateCartGoInfo(Tb_CartGo cartGo,string oldId)
        {
            var cmd = new DataCommand("mySql", "UpdateCartGoInfo");
            cmd.SetParameters("@userId",cartGo.UserId);
            cmd.SetParameters("@Oldid",oldId);
            cmd.ExeSqlCmd();
        }

        public void UpdateOrder(Tb_Order order)
        {
            DataCommand.Update<Tb_Order>(order);
        }
    }
    public class UserInfo: IUserInfo
    {
        public IList<Tb_UserInfo> GeTbUserInfos(string userSsid)
        {
            var cmd = new DataCommand("mySql", "GeTbUserInfos");
            cmd.SetParameters("@ssid", userSsid);
            return cmd.ExecuteSql<Tb_UserInfo>();
        }
       
        public void UpdateUserInfo(Tb_UserInfo info)
        {
            DataCommand.Update(info);
        }
    }

    public class User: IUser
    {
        public Tb_User SearchUser(string userName)
        {
            var cmd = new DataCommand("mySql", "GeTbUserByName");
            cmd.SetParameters("@name",userName);
            var findIt = cmd.ExecuteSql<Tb_User>().FirstOrDefault();
            return findIt;
        }

      /*  public Tb_UserInfo GetUserInfo(string userName)
        {
            var cmd = new DataCommand();
            var findIt = cmd.Search<Tb_User>(x => x.Name.Equals(userName));
            var info=cmd.Search<Tb_UserInfo>(x => x.UserId==findIt.Id);
            return info;
        }*/

        public void AddUser(Tb_User user,string phone)
        {
            var db = new MySqlContext();
            db.TbUsers.Add(user);
            db.SaveChanges();
        }

        public void UpdateUserInfo(Tb_UserInfo userInfo)
        {
            var cmd=new DataCommand("mySql", "InsertUserInfo");
            cmd.SetParameters("@UserId",$"{userInfo.UserId}");
            cmd.SetParameters("@Address",userInfo.Address);
            cmd.SetParameters("@Phone", userInfo.Phone1);
            cmd.ExeSqlCmd();
        }

        public void UpdateUser(Tb_User user)
        {
            DataCommand.Update(user);
        }
    }

    public interface ISaleArea
    {
        IList<Tb_SaleArea> GeTbSaleAreas();
    }
    public class SaleArea: ISaleArea
    {
        public IList<Tb_SaleArea> GeTbSaleAreas()
        {
            var cmd = new DataCommand("mySql", "GeTbSaleAreas");
            return cmd.ExecuteSql<Tb_SaleArea>();
        } 
    }

    public interface IPostInfoDal
    {
        void AddPostInfo(Tb_PostInfo info);
        IList<AdeverModel> GetPostInfos();
        void UpdatePostInfo(string info);
    }
    public class PostInfoDal: IPostInfoDal
    {
        public void AddPostInfo(Tb_PostInfo info)
        {
             var db = new MySqlContext();
            db.TbPostInfos.Add(info);
            db.SaveChanges();
        }

        public IList<AdeverModel> GetPostInfos()
        {
            var cmd=new DataCommand("mySql", "GetPostInfos");
            return cmd.ExecuteSql<AdeverModel>();
        }

        public void UpdatePostInfo(string info)
        {
           var cmd=new DataCommand("mySql", "UpdatePostInfos");
            cmd.SetParameters("@Id",$"{info}");
            cmd.ExeSqlCmd();
        }
    }

    public interface IUserRecommend
    {
        void AddRecommend(Tb_Recommend recommend);
        IList<Tb_Recommend> GetAllRecommend(string name);
        Tb_Recommend IsExists(string name);
    }
    public class UserRecommendDal: IUserRecommend
    {
        public void AddRecommend(Tb_Recommend recommend)
        {
            var db = new MySqlContext();
            db.TbRecommends.Add(recommend);
            db.SaveChanges();
        }

        public IList<Tb_Recommend> GetAllRecommend(string name)
        {
            var db = new MySqlContext();
            var result = db.TbRecommends.Where(x=>x.RecommendUser.Equals(name));
            return result.ToList();
        }
        public Tb_Recommend IsExists(string name)
        {
            var db = new MySqlContext();
            var result = db.TbRecommends.Where(x => x.User.Equals(name)).FirstOrDefault();
            return result;
        }
    }
}
