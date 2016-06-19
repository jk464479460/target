/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/5/7 10:28:58
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/

using System.Data.Entity;
using Tables;

namespace SalesDispatcher.DataContext
{
    public class MySqlContext : DbContext
    {
        public MySqlContext() : base("MyContext") { }
        public DbSet<Tb_Order> TbOrders { get; set; }
        public DbSet<Tb_CartGo> TbCartGoes { get; set; }
    }
}
