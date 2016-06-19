using System;
using System.ComponentModel;
using System.Drawing;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Model;

namespace WinClient
{
    public partial class Form1 : Form
    {
        private readonly BackgroundWorker _bg=new BackgroundWorker();
        private Client _client = new Client();
        private bool _startNext = true;
        private object _lockNext=new object();

        public Form1()
        {
            InitializeComponent();
            UpdateUi.Context = SynchronizationContext.Current;
            UpdateUi.CallBackMethod = OnUpdateWcsEvent;
            _bg.RunWorkerCompleted += _bg_RunWorkerCompleted;
            _bg.RunWorkerAsync();
        }

        private void _bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        
            Task.Factory.StartNew(() =>
            {
                _client.AwayConnect();
            });
            Task.Factory.StartNew(() =>
            {
                _client.ReceiveMessageWcs();
            });
            var t=Task.Factory.StartNew(() =>
            {
                Init(null);
            });
           
        }

        protected virtual void OnUpdateWcsEvent(object param)
        {
          
            //var order = () param;
            //var listViewItem=new ListViewItem();
            //listViewItem.Text = $"订单号: {order.OrderId}    订单时间：{order.CreateTime}";
            //listView1.Items.Add(listViewItem);

            //listView1.Items.Add($"电话:{order.PhoneNumber} 地址：{order.Address}");

            //var listViewItem3 = listView1.Items.Add("名称");
            //listViewItem3.SubItems.Add("数量");
            //listViewItem3.SubItems.Add("单价");
            //listViewItem3.SubItems.Add("总价");

            //var totalPay = 0m;
            //foreach (var item in order.Content)
            //{
            //    var listViewItem4 = listView1.Items.Add($"{item.StockCode}");
            //    listViewItem4.SubItems.Add($"{item.BuyCnt}");
            //    listViewItem4.SubItems.Add($"{item.CurPrice}");
            //    totalPay += item.BuyCnt * item.CurPrice;
            //    listViewItem4.SubItems.Add($"{ item.BuyCnt * item.CurPrice}");
               
            //}
           
        }
        void Init(object param)
        {
            while (true)
            {
                Thread.Sleep(1000);
                lock (_lockNext)
                {
                    if (!_startNext) continue;
                }
                UpdateOrderCnt();
                var order = new MsMqClient().GetAllMessage<OrderDelivery>();
                if (order == null) continue;
                this.Invoke(new MethodInvoker(() =>
                {
                    listView1.Clear();
                }));
                #region 头部设置
                ColumnHeader columnHeader0 = new ColumnHeader
                {
                    Text = "",
                    Width=200

                };
                ColumnHeader columnHeader1 = new ColumnHeader
                {
                    Text = "",
                    Width = 55
                };
                ColumnHeader columnHeader2 = new ColumnHeader
                {
                    Text = "",
                    Width = 55
                };
                ColumnHeader columnHeader3 = new ColumnHeader
                {
                    Text = "",
                    Width = 55
                };
                ColumnHeader columnHeader4 = new ColumnHeader
                {
                    Text = "",
                    Width = 55
                };
                #endregion
                this.Invoke(new MethodInvoker(() =>
                {
                    //listView1.Columns.AddRange(new ColumnHeader[] { columnHeader0, columnHeader1, columnHeader2, columnHeader3, columnHeader4 });

                    var item3 = new ListViewItem();
                    item3.Text = $"订单号：{order.OrderId}    订单时间:{order.CreateTime}";
                    listView1.Items.Add(item3);
                    var itemAddress = new ListViewItem();
                    itemAddress.Text = $"{order.Address} {order.PhoneNumber}";
                    listView1.Items.Add(itemAddress);
                    var item2=new ListViewItem();
                    item2.Text = "名称         条码        数量    单价    总价";
                    listView1.Items.Add(item2);
                    var total = 0m;
                    foreach (var content in order.Content)
                    {
                        var tmp=content.BuyCnt*content.CurPrice;
                        total += tmp;
                        var item = $"{content.Name}       {content.StockCode}    {content.BuyCnt}    {content.CurPrice}    {tmp}"; 
                        listView1.Items.Add(item);
                        
                    }
                    var item4 = new ListViewItem {Text = $"总金额：{total}"};
                    listView1.Items.Add(item4);
                }));
                UpdateOrderCnt();
                lock (_lockNext)
                {
                    _startNext = false;
                }
            }

        }

        private void UpdateOrderCnt()
        {
            this.Invoke(new MethodInvoker(() =>
            {
                label2.Text = $"{new MsMqClient().GetCntMessage<OrderDelivery>()}";
            }));
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.printDialog1.ShowDialog() == DialogResult.OK)
            {
                this.printDocument1.Print();
            }
            lock (_lockNext)
            {
                _startNext = true;
            }
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Font font = new Font("宋体", 12);
            Brush bru = Brushes.Blue;
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                e.Graphics.DrawString(listView1.Items[i].Text, font, bru, 20, i*20);
            }
            this.Invoke(new MethodInvoker(()=> {
                listView1.Items.Clear();
            }));
        }
    }

    public class UpdateUi
    {
        public static SynchronizationContext Context { get; set; }
        public static SendOrPostCallback CallBackMethod { get; set; }

        public static void Post(object param)
        {
            //通过队列Context.Post(CallBackMethod, param);
            new MsMqClient().SendMessageToMq<OrderDelivery>("CLientOrder",param);
        }
       
    }

    public class MsMqClient
    {
        public void SendMessageToMq<T>(string label, object body)
        {
            var path = ".\\private$\\" + "ClientOrderDelivery";

            if (!MessageQueue.Exists(path))
                MessageQueue.Create(path);

            var mq = new MessageQueue(path);
            mq.Formatter = new XmlMessageFormatter(new Type[] { typeof(T) });
            var msg = new System.Messaging.Message
            {
                Label = label,
                Body = body,
                Recoverable = true
            };
            mq.Send(msg);
            msg = null;
            mq.Close();
            mq = null;
        }
        public T GetAllMessage<T>()
        {
            try
            {
                var myQueue = new MessageQueue(".\\private$\\" + "ClientOrderDelivery");
                myQueue.Formatter = new XmlMessageFormatter(new Type[] {typeof (T)});
                //Message[] message = myQueue.GetAllMessages();
                var message = myQueue.Receive();
                return (T) message?.Body;
            }
            catch (Exception ex)
            {
                return default(T);
            }
         
        }

        public int GetCntMessage<T>()
        {
            try
            {
                var myQueue = new MessageQueue(".\\private$\\" + "ClientOrderDelivery");
                myQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(T) });
                var message = myQueue.GetAllMessages();
                return message.Length;
            }
            catch (Exception ex)
            {
                return -1;
            }

        }
    }
}
