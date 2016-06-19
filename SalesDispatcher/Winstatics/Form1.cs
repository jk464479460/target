using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Winstatics.Bll;

namespace Winstatics
{
    public partial class Form1 : Form
    {
        private readonly BackgroundWorker _bgWorker = new BackgroundWorker();
        private readonly GoodsHandler _goodsHandler=new GoodsHandler();
        private readonly UserStatistics _userStatistics=new UserStatistics();

        public Form1()
        {
            InitializeComponent();
            UpdateUi.Context = SynchronizationContext.Current;
            UpdateUi.CallBackMethod = OnUpdateWcsEvent;
            UpdateUi.UpdateListView = OnUpdateListView;
            UpdateUi.UpdateLoginUser = OnUpdateLoginUser;
            _bgWorker.RunWorkerCompleted += _bgWorker_RunWorkerCompleted;
            _bgWorker.RunWorkerAsync();
        }

        private void _bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Init();
        }

        private void Init()
        {
            Task.Factory.StartNew(() =>
            {
                _goodsHandler.GetCurrentOrderNumber();
            });

            Task.Factory.StartNew(() =>
            {
                _goodsHandler.GetPagedList();
            });

            Task.Factory.StartNew(() =>
            {
                _goodsHandler.At12TimeHanler();
            });
            Task.Factory.StartNew(() =>
            {
                _userStatistics.GetCurrentUserCount();
            });
            Task.Factory.StartNew(() =>
            {
                _userStatistics.DelSession();
            });
        }

        protected virtual void OnUpdateWcsEvent(object param)
        {
            var a = (int) param;
            labelCurrentCnt.Text = $"{a}";

        }
        //更新列表
        protected virtual void OnUpdateListView(object param)
        {
            var res = (IList<ListShowModel>) param;
            listView1.Items.Clear();
            var i = 0;
            foreach (var order in res)
            {
                var view = listView1.Items.Add($"{i++}");
                view.SubItems.Add($"{order.Name}");
                view.SubItems.Add($"{order.Code}");
                view.SubItems.Add($"{order.InNumber}");
                view.SubItems.Add($"{order.OutNumber}");
            }
        }

        protected virtual void OnUpdateLoginUser(object param)
        {
            var a = (int)param;
            this.BeginInvoke(new MethodInvoker(() =>
            {
                labelLoginUser.Text = $"{a}";
            }));

        }

        private void button5_Click(object sender, System.EventArgs e)
        {
          _goodsHandler.GetListView();
        }
    }

    public class ListShowModel
    {
        public string Name { get; set;}
        public string Code { get; set; }
        public int InNumber { get; set; }
        public int OutNumber { get; set; }
    }
    public class UpdateUi
    {
        public static SynchronizationContext Context { get; set; }
        public static SendOrPostCallback CallBackMethod { get; set; }
        public static SendOrPostCallback UpdateListView { get; set; }
        public static SendOrPostCallback UpdateLoginUser { get; set; }

        public static void Post(object param)
        {
            Context.Post(CallBackMethod, param);
        }

        public static void PostUpdateListView(object param)
        {
            Context.Post(UpdateListView, param);
        }

        public static void PostUpdateLoginUser(object param)
        {
            Context.Post(UpdateLoginUser,param);
        }
    }
}
