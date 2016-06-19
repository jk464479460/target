using System;
using System.Threading;
using System.Threading.Tasks;
using SalesDispatcher.Bll;

namespace SalesDispatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始执行任务... ...");
            var run = new OrderDispatcher();
            Task.Factory.StartNew(() =>
            {
                run.StartDispatcher();
               
            });
            Thread.Sleep(2000);
            Task.Factory.StartNew(() =>
            {
                run.OrderDelivery();

            });
            Task.Factory.StartNew(() =>
            {
                run.HandleMq();
              
            });
            Console.ReadLine();
        }
    }
}
