using System.Data.Entity;
using Tables;

namespace Dal
{
    public class MySqlContext: DbContext
    {
        public MySqlContext() : base("MyContext") { }
        public DbSet<Tb_Goods> TbGoods { get; set; }
        public DbSet<Tb_GoodsInfo> TbGoodsInfos { get; set; } 
        public DbSet<Tb_CartGo> TbCartGoes { get; set; } 
        public DbSet<Tb_UserInfo> TbUserInfos { get; set; } 
        public DbSet<Tb_Order> TbOrders { get; set; }
        public DbSet<Tb_Order_Old> TbOrderOlds { get; set; }
        public DbSet<Tb_User> TbUsers { get; set; } 
        public DbSet<Tb_PostInfo> TbPostInfos { get; set; }
        public DbSet<Tb_Recommend> TbRecommends { get; set; }

    }

}