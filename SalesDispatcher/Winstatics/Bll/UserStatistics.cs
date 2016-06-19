/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/5/16 19:19:37
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Threading;
using Help;
using StackExchange.Redis;

namespace Winstatics.Bll
{
    public class UserStatistics
    {
        private readonly string _redisCfg;
        private ConnectionMultiplexer _connectionMultiplexer;

        public UserStatistics()
        {
            using (var reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"/config/config.ini"))
            {
                var line = reader.ReadLine();
                var arr = line.Split('@');
                _redisCfg = arr[1];
            }
        }
        public ConnectionMultiplexer Connection
        {
            get
            {
                if (_connectionMultiplexer == null)
                {
                    _connectionMultiplexer = ConnectionMultiplexer.Connect(_redisCfg);
                }
                return _connectionMultiplexer;
            }
        }

        public void GetCurrentUserCount()
        {
            while (true)
            {
                Thread.Sleep(10000);
                var total = 0;
                try
                {
                    var server = Connection.GetServer(_redisCfg, 6379);
                    var db = Connection.GetDatabase();
                    var keys = server.Keys();

                    foreach (var key in keys.AsQueryable())
                    {
                        var value = db.StringGet(key).ToString();
                        var arr = value.Split('_');
                        if (!arr[0].Equals("1999"))
                            continue;
                        total++;
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"{ex.Message} {ex.StackTrace}");
                }
                UpdateUi.UpdateLoginUser(total);
            }
        }

        public void DelSession()
        {
            var oldTime = DateTime.Now;
            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    if (DateTime.Now.Hour != 0 || DateTime.Now.Minute != 10)
                        continue;
                    var server = Connection.GetServer(_redisCfg, 6379);
                    var db = Connection.GetDatabase();
                    var keys = server.Keys();

                    foreach (var key in keys.AsQueryable())
                    {
                        var value = db.StringGet(key).ToString();
                        var arr = value.Split('_');
                        if (!arr[0].Equals("1999"))
                            continue;
                        db.KeyDelete(key);
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"{ex.Message} {ex.StackTrace}");
                }
            }
        }

    }
}
