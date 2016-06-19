/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/5/7 16:37:40
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;

namespace MsCore
{
    public class MsMq
    {
        public void SendMessageToMq<T>(string label, object body)
        {
            var path = ".\\private$\\" + "OrderDelivery";

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
            var myQueue = new MessageQueue(".\\private$\\" + "OrderDelivery");
            myQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(T) });
            //Message[] message = myQueue.GetAllMessages();
            var message = myQueue.Receive();
            return (T)message?.Body;
        }
    }

}
