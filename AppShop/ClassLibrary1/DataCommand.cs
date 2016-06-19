using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Tables;

namespace FrameWrok.Common
{
    public class MySqlContext : DbContext
    {
        public MySqlContext() : base("MyContext") { }
        public DbSet<Tb_Goods> TbGoods { get; set; }
        public DbSet<Tb_GoodsInfo> TbGoodsInfos { get; set; }
        public DbSet<Tb_CartGo> TbCartGoes { get; set; }
        public DbSet<Tb_UserInfo> TbUserInfos { get; set; }
        public DbSet<Tb_Order> TbOrders { get; set; }
        public DbSet<Tb_User> TbUsers { get; set; }
    }
    public class DataCommand: MySqlContext/*<TContext> where TContext : DbContext, new()*/
    {
        private readonly string _cmdKey;
        private readonly string _server;
        private IDictionary<string, SqlCmdModel> _dictCmd = new Dictionary<string, SqlCmdModel>();

        /// <summary>
        /// xml中，sqlcmd 的name属性
        /// </summary>
        public string CmdKey
        {
            get
            {
                return _cmdKey;
            }
        }
        /// <summary>
        /// 数据库类型：支持sql server
        /// </summary>
        public string Server
        {
            get
            {
                return _server;
            }
        }

        /// <summary>
        /// 当前sql语句
        /// </summary>
        public string CurCmdLine => _dictCmd.Keys.Any() == false ? string.Empty : _dictCmd[_cmdKey].CmdLine;

        public DataCommand(string server, string sqlCmdName)
        {
            _server = server;
            _cmdKey = sqlCmdName;
            ReadCmmdConfig();
        }

        public DataCommand()
        {
            //ReadCmmdConfig();
        } 

        private IList<object> _paramsDict=new List<object>();
        /// <summary>
        /// sql语句参数
        /// </summary>
        /// <param name="param"></param>
        /// <param name="val"></param>
        public void SetParameters(string param, string val)
        {
            if(_dictCmd.Keys.Any()==false) throw new SqlNotFilledException();
            var targetCmd = _dictCmd[_cmdKey].CmdLine;
            _dictCmd[_cmdKey].CmdLine = targetCmd.Replace(param, val);
            _paramsDict.Add(val);
        }
       
        public List<T> ExecuteSql<T>() where T:class,new ()
        {
            if (string.IsNullOrEmpty(CurCmdLine)) return null;
            using (var db = new MySqlContext())/* new DataCommandContext<TContext>().GetContext()*/
            {
                var sql = CurCmdLine;
                var res = db.Database.SqlQuery<T>(sql).ToList();
                return res;
            }
        }

        /// <summary>
        /// 添加基本表内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Add<T>(T obj, bool save) where T:class 
        {
            var _db = new MySqlContext();//new DataCommandContext<TContext>().GetContext();
            //using (var )
            {
                _db.Entry<T>(obj).State=EntityState.Added;
                _db.SaveChanges();
               
            }
            return true;
        }

        public int ExeSqlCmd()
        {
            using (var db = new MySqlContext())/*new DataCommandContext<TContext>().GetContext()*/
            {
                var cnt=db.Database.ExecuteSqlCommand(_dictCmd[_cmdKey].CmdLine, _paramsDict);
                _paramsDict.Clear();
                return cnt;
            }
        }
        //查找
        public  T Search<T>(Func<T, bool> where) where T:class 
        {
            using (var db = /*new DataCommandContext<TContext>().GetContext())*/new MySqlContext())
            {
                var firstOrDefault = db.Set<T>().AsQueryable().FirstOrDefault(@where);
                return firstOrDefault;
            }
        }
        //修改
        public static bool Update<T>(T obj) where T : class
        {
            var db = /*new DataCommandContext<TContext>().GetContext()*/new MySqlContext();
            //using ()
            {
                db.Set<T>().Attach(obj);
                db.Entry<T>(obj).State=EntityState.Modified;
                return db.SaveChanges() > 0;
            }
        }

        private void ReadCmmdConfig()
        {
            var dir=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory+"config");
            if(dir==null) throw new InvalidOperationException();
            var fileList=dir.GetFiles("*.xml");
            foreach (var fileInfo in fileList)
            {
                ParseXml(fileInfo.FullName);
                if (_dictCmd.Keys.Count > 0) return;
            }
        }

        private void ParseXml(string path)
        {
            SqlRoot root;
            using (var stream = new StreamReader(path))
            {
                var xmlSeri = new XmlSerializer(typeof(SqlRoot));
                root = (SqlRoot)xmlSeri.Deserialize(stream);
            }
            if (!root.SqlCmdList.Any()) throw new SqlNullValueException();
            var serverFound =root.SqlCmdList.Where(x => x.Name.Equals(_server)).ToList();
            if(!serverFound.Any()) throw new SqlNullValueException();
            var cmdFound = serverFound[0].SqlCmd.Where(x => x.Name.Equals(_cmdKey)).ToList();
            if(!cmdFound.Any()) throw new SqlNotFilledException();
            _dictCmd.Add(_cmdKey,cmdFound[0]);
        }
    }
    
    [XmlRoot("SqlRoot")]
    public class SqlRoot
    {
        [XmlElement("SqlList")]
        public List<SqlList> SqlCmdList;
    }
    [XmlRoot("SqlList")]
    public class SqlList
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("SqlCmd")]
        public List<SqlCmdModel> SqlCmd { get; set; } 
    }
    [XmlRoot("SqlCmd")]
    public class SqlCmdModel
    {
        [XmlElement("param")]
        public List<Params> Param { get; set; }
        [XmlElement("CmdLine")]
        public string CmdLine { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
    [XmlRoot]
    public class Params
    {
        [XmlAttribute("name")]
        public string Name { get; set;}
        [XmlAttribute("type")]
        public string Type { get; set; }
    }
}
