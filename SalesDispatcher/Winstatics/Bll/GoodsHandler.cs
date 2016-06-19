/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/5/16 9:43:59
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Transactions;
using Dal;
using Help;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using Tables;

namespace Winstatics.Bll
{
    public class GoodsHandler
    {
        private const int OrderStaticsTimeDelay = 10000;
        private const int ListTimeDelay = 5000;
        private int _currentPageIndex = 1;
        private const int PageShowNumber = 10;
        //private bool _todayHasHand = false;

        public void Test()
        {
            try
            {
                ISaleArea _area=new SaleArea();
                var res=_area.GeTbSaleAreas();
            }
            catch (Exception ex)
            {
                
            }
        }

        public void GetCurrentOrderNumber()
        {
            while (true)
            {
                Thread.Sleep(OrderStaticsTimeDelay);
                try
                {
                    var orders = new MySqlContext().TbOrders;
                    var startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                    var endTime = new DateTime(startTime.Year, startTime.Month, startTime.AddDays(1).Day, 0, 0, 0);
                    var cnt = orders.AsQueryable().Count(x => x.CreateTime >= startTime && x.CreateTime < endTime);
                    UpdateUi.Post(cnt);
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"{ex.Message} {ex.StackTrace}");   
                }
            }
        }

        public void GetPagedList()
        {
            while (true)
            {
                Thread.Sleep(ListTimeDelay);
                try
                {
                    GetPagedListView();
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"{ex.Message} {ex.StackTrace}");
                }
               
            }
        }

        public void GetListView()
        {
            var context = new MySqlContext();
            var orders = context.TbOrders;
            var cartGo = context.TbCartGoes;
            var goods = context.TbGoods;
            var result = from p in (from c in (from order in orders
                                               join go in cartGo on order.OrderId equals go.OrderId
                                               select new { Code = go.StockCode, Count = go.BuyCnt })
                                    group c by c.Code into g
                                    select new { Code = g.Key, Cnt = g.Sum(p => p.Count) })
                         join good in goods on p.Code equals good.Code
                         orderby p.Code
                         select new { Name = good.Name, Code = good.Code, InNumber = good.Numbers, OutNumber = p.Cnt };
            var listRes = new List<ListShowModel>();
            listRes.AddRange(result.Select(x => new ListShowModel
            {
                Name = x.Name,
                Code = x.Code,
                InNumber = x.InNumber,
                OutNumber = x.OutNumber
            }));
            var dt = new DataTable("");
            dt.Columns.Add("名称");
            dt.Columns.Add("编码");
            dt.Columns.Add("库存");
            dt.Columns.Add("出库");
            foreach (var x in result)
            {
                var row = dt.NewRow();
                row[0] = x.Name;
                row[1] = x.Code;
                row[2] = x.InNumber;
                row[3] = x.OutNumber;
                dt.Rows.Add(row);
            }
            Excel.SaveToExcel(dt, "随便点", AppDomain.CurrentDomain.BaseDirectory + "data.xlsx");

        }
        
        public void At12TimeHanler()
        {
            //var oldTime = DateTime.Now;
            //启动
            Setup();
           /* _todayHasHand = true;*/
            while (true)
            {
                /*  if (DateTime.Now.Day > oldTime.Day)
                  {
                      _todayHasHand = false;
                  }*/
                Thread.Sleep(10000);
                if (DateTime.Now.Hour!=0 || DateTime.Now.Minute!=10 /*|| _todayHasHand == true*/)
                {
                    continue;
                }
                Setup();
              /*  _todayHasHand = true;*/
                //oldTime = DateTime.Now;
            }
        }

        private void Setup()
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    GetGoodsStockSalesCount();
                    CleanOrderTable();
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"{ex.Message} {ex.StackTrace}");
                }
            }
           
        }

        private void GetGoodsStockSalesCount()
        {
            var context = new MySqlContext();
            var result = GetOrderInfo();

            foreach (var item in result.AsQueryable())
            {
                var good = context.TbGoods.FirstOrDefault(x => x.Code.Equals(item.Code));
                good.Numbers = good.Numbers - item.Cnt;
                good.SaleCount = good.SaleCount + item.Cnt;
            }
            context.SaveChanges();
        }

        private void CleanOrderTable()
        {
            var context = new MySqlContext();
            var orders = context.TbOrders.Where(x=>x.IsSend==1);
            foreach (var order in orders)
            {
                var newOrder=new Tb_Order_Old
                {
                  OrderId = order.OrderId,
                  CreateTime=order.CreateTime,
                  Address=order.Address,
                  AreaId=order.AreaId,
                  PhoneNumber=order.PhoneNumber,
                  Id=order.Id,
                  IsSend=order.IsSend,
                  OperTime=order.OperTime,
                  OperUser=order.OperUser,
                  UserId=order.UserId
                };
                context.TbOrderOlds.Add(newOrder);
                context.TbOrders.Remove(order);
            }
            context.SaveChanges();
        }

        private IList<GoodsStatics> GetOrderInfo()
        {
            var context = new MySqlContext();
            var orders = context.TbOrders;
            var cartGo = context.TbCartGoes;
            var goods = context.TbGoods;

            var result = from c in (from order in orders
                                    join go in cartGo on order.OrderId equals go.OrderId
                                    select new { Code = go.StockCode, Count = go.BuyCnt })
                         group c by c.Code
                into g
                         select new GoodsStatics {  Code= g.Key,  Cnt= g.Sum(p => p.Count) };
            return result.ToList();
        }
        //获取分页列表
        private void GetPagedListView()
        {
            var context= new MySqlContext();
            var orders = context.TbOrders;
            var cartGo = context.TbCartGoes;
            var goods = context.TbGoods;
            var orderInfo = from c in(from order in orders join go in cartGo on order.OrderId equals go.OrderId
                select new {Code = go.StockCode, Count = go.BuyCnt}) group c by c.Code into g select new {Code=g.Key,Cnt=g.Sum(p=>p.Count)};
            var total = (from p in orderInfo
                join good in goods on p.Code equals good.Code
                select new {Name = good.Name, Code = good.Code, InNumber = good.Numbers, OutNumber = p.Cnt}).Count();
            var totalNumber = total / PageShowNumber + total % PageShowNumber == 0 ? 0 : 1;
            _currentPageIndex = _currentPageIndex > totalNumber ? 1 : _currentPageIndex;
           
            var result = (from p in (from c in (from order in orders
                                                join go in cartGo on order.OrderId equals go.OrderId
                                                select new { Code = go.StockCode, Count = go.BuyCnt })
                                     group c by c.Code into g
                                     select new { Code = g.Key, Cnt = g.Sum(p => p.Count) })
                join good in goods on p.Code equals good.Code orderby p.Code
                select new {Name = good.Name, Code = good.Code, InNumber = good.Numbers, OutNumber = p.Cnt}).Skip((_currentPageIndex-1)*PageShowNumber).Take(PageShowNumber);
            var listRes=new List<ListShowModel>();
            listRes.AddRange(result.Select(x=> new ListShowModel
            {
                Name=x.Name,
                Code=x.Code,
                InNumber=x.InNumber,
                OutNumber=x.OutNumber
            }));
            UpdateUi.PostUpdateListView(listRes);
            _currentPageIndex++;
        }
      
    }

    public class Excel
    {
        public static int SaveToExcel(DataTable dgv, string title, string filePath)
        {
            if (dgv == null || (dgv.Rows.Count == 0 && dgv.Columns.Count == 0))
                return 1;
            HSSFWorkbook hssfworkbook = new HSSFWorkbook();
            ISheet sheet = hssfworkbook.CreateSheet("Sheet1");
            hssfworkbook.CreateSheet("Sheet2");
            hssfworkbook.CreateSheet("Sheet3");

            //Title
            IRow row = sheet.CreateRow(0);
            ICell cell = row.CreateCell(0);
            cell.SetCellValue(title);
            ICellStyle style = hssfworkbook.CreateCellStyle();
            style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            IFont font = hssfworkbook.CreateFont();
            font.FontHeight = 20 * 20;
            style.SetFont(font);
            cell.CellStyle = style;
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 0, dgv.Columns.Count - 1));

            //Header
            int r, c;
            row = sheet.CreateRow(1);
            for (c = 0; c < dgv.Columns.Count; c++)
            {
                row.CreateCell(c).SetCellValue("");
            }

            // content
            for (r = 0; r < dgv.Rows.Count; r++)
            {
                row = sheet.CreateRow(r + 1);
                for (c = 0; c < dgv.Columns.Count; c++)
                {
                    cell = row.CreateCell(c);
                   
                    if (dgv.Columns[c] != null)
                        cell.SetCellValue(dgv.Columns[c].ToString());
                }
            }
            r = 0;
            FileStream file = null;
            try
            {
                file = new FileStream(filePath, FileMode.Create);
                hssfworkbook.Write(file);
            }
            catch (Exception ex)
            {
                r = 1;
            }
            finally
            {
                if (file != null)
                    file.Close();
            }
            return r;
        }
    }

    public class GoodsStatics
    {
     public string Code { get; set; }
        public int Cnt { get; set; }
    }
}
