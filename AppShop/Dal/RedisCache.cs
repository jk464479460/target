using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using StackExchange.Redis;

/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/4/22 16:26:45
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/
namespace Dal
{
    public interface IRedisOper
    {
        void Set(string key, string val);
        string Get(string key);
        void HSet(string key, object value);
        T GetHash<T>(string key);
    }
    public class RedisOper: IRedisOper
    {
        private static string _redisCfg;
        private static ConnectionMultiplexer _connectionMultiplexer;

        public static ConnectionMultiplexer Connection
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

        public RedisOper(string cfg)
        {
            _redisCfg = cfg;
        }

        public void Set(string key,string val)
        {
            //using (var muxer = ConnectionMultiplexer.Connect(_redisCfg))
            {
                var db = Connection.GetDatabase();
                db.StringSet(key, val);
               // var val = db.StringGet("mykey");
            }
        }

        public string Get(string key)
        {
            //using (var muxer = ConnectionMultiplexer.Connect(_redisCfg))
            {
                var db = Connection.GetDatabase();
              
                return db.StringGet(key);
            }
        }

        public void HSet(string key, object value)
        {
            //using (var muxer = ConnectionMultiplexer.Connect(_redisCfg))
            {
                var db = Connection.GetDatabase();
                StackExchangeRedisExtensions.Set(db, key, value);
            }

        }
        public T GetHash<T>(string key)
        {
            //using (var muxer = ConnectionMultiplexer.Connect(_redisCfg))
            {
                var db = Connection.GetDatabase();
                var data=StackExchangeRedisExtensions.Get<T>(db,key);
               
                return data;
            }
        }



    }
    public static class RedisUtils
    {
        //Serialize in Redis format:
        public static HashEntry[] ToHashEntries(object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            return properties.Select(property => new HashEntry(property.Name, property.GetValue(obj).ToString())).ToArray();
        }
        //Deserialize from Redis format
        public static T ConvertFromRedis<T>(HashEntry[] hashEntries)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            var obj = Activator.CreateInstance(typeof(T));
            foreach (var property in properties)
            {
                HashEntry entry = hashEntries.FirstOrDefault(g => g.Name.ToString().Equals(property.Name));
                if (entry.Equals(new HashEntry())) continue;
                property.SetValue(obj, Convert.ChangeType(entry.Value.ToString(), property.PropertyType));
            }
            return (T)obj;
        }
    }

    class StackExchangeRedisExtensions
    {
        public static T Get<T>( IDatabase cache, string key)
        {
            return Deserialize<T>(cache.StringGet(key));
        }

        public static object Get(IDatabase cache, string key)
        {
            return Deserialize<object>(cache.StringGet(key));
        }

        public static void Set(IDatabase cache, string key, object value)
        {
            cache.StringSet(key, Serialize(value));
        }

        static byte[] Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, o);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        static T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream(stream))
            {
                T result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }
    }
}
