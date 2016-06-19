/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/4/22 16:38:02
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/

using System;
using System.IO;
using ToolHelp;

namespace Bll
{
    public class ReadCfg
    {
        private static string _cfgPath;

        public ReadCfg()
        {
            _cfgPath =AppDomain.CurrentDomain.BaseDirectory;
        }
        public string GetRedisCfg()
        {
            var result = string.Empty;
            var fullPath = _cfgPath + @"config/config.ini";
            AppLogger.Info(fullPath);
            using (var reader = new StreamReader(fullPath))
            {
                var line=reader.ReadLine();
                if(string.IsNullOrEmpty(line))
                    throw new Exception("配置文件错误");
                var arr = line.Split('@');
                result=arr[1];
            }
            return result;
        }
    }
}
