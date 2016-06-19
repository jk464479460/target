/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/5/7 10:51:50
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/

using System.Collections;
using System.Collections.Generic;
using FrameWrok.Common;
using SalesDispatcher.DataContext;
using Tables;

namespace SalesDispatcher.Dal
{
    public interface IOperDal
    {
        IList<Tb_SaleArea> GetDeliveryAreas();
        IList<OrderDeliveryModel> GeTbOrdersNotSend();
        void UpdateTbOrdersSend(string orderId);
    }
    public class OperDal: IOperDal
    {
        public IList<Tb_SaleArea> GetDeliveryAreas()
        {
            var cmd=new DataCommand<MySqlContext>("mySql", "GetDeliveryArea");
            return cmd.ExecuteSql<Tb_SaleArea>();
        }

        public IList<OrderDeliveryModel> GeTbOrdersNotSend()
        {
            var cmd = new DataCommand<MySqlContext>("mySql", "GeTbOrdersNotSend");
            return cmd.ExecuteSql<OrderDeliveryModel>();
        }

        public void UpdateTbOrdersSend(string orderId)
        {
            var cmd=new DataCommand<MySqlContext>("mySql", "UpdateTbOrdersSend");
            cmd.SetParameters("@orderId",orderId);
            cmd.ExeSqlCmd();
        }
    }

    public class OrderDeliveryModel: Tb_Order
    {
        public string StockCode { get; set; }
        public int BuyCnt { get; set; }
        public decimal CurPrice { set; get; }
        public string Name { get; set; }
    }
}
