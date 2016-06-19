/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/5/19 14:59:20
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/

using System;
using Dal;
using QueryContract.cs;
using ResultView.cs;
using Tables;
using ToolHelp;

namespace Bll
{
    public class UserPostInfo
    {
        private readonly IRedisOper _redisOper = new RedisOper(new ReadCfg().GetRedisCfg());
        private readonly IPostInfoDal _postInfo=new PostInfoDal();

        public ResultAddPostInfo AddPostInfo(QueryAddPostInfo query)
        {
            var result=new ResultAddPostInfo {Exception=new MyException()};
            try
            {
                var realSsid = new EncryDecry().Md5Decrypt(query.Ssid);
                var arr = GetSession(realSsid);
                if (arr.Length < 3 || ValidatePostInfo(query) != 0)
                {
                    result.Exception.Exmsg = "not found 标题或内容超过规定";
                    result.Exception.Success = true;
                    return result;
                }
                var  postInfo=new Tb_PostInfo
                {
                    CreateTime=DateTime.Now,
                    UserId= arr.GetValue(2).ToString(),
                    Title =query.Title,
                    PostText=query.PostText,
                    IsShow=1,
                    ViewCount=1
                };
                _postInfo.AddPostInfo(postInfo);
                result.Exception.Success = true;
            }
            catch (Exception ex)
            {
                result.Exception.Success = false;
                AppLogger.Error($"{ex.Message} {ex.StackTrace}");
            }
            return result;
        }

        public ResultAdverInfo GetPostInfo()
        {
            var result=new ResultAdverInfo {Exception=new MyException()};
            try
            {
                result.InfoList = _postInfo.GetPostInfos();
                result.Exception.Success = true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"{ex.Message} {ex.StackTrace}");
                result.Exception.Success = false;
            }
            return result;
        }

        public ResultAddPostInfo AddClick(QueryAddClick query)
        {
            var result=new ResultAddPostInfo {Exception = new MyException()};
            try
            {
               _postInfo.UpdatePostInfo(query.Id);
                result.Exception.Success = true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"{ex.Message} {ex.StackTrace}");
                result.Exception.Success = false;
            }
            return result;
        }

        private int ValidatePostInfo(QueryAddPostInfo query)
        {
            if (query.Title.Length > 16)
                return 1;
            if (query.PostText.Length > 56)
                return 2;
            return 0;
        }
        private Array GetSession(string realSession)
        {
            var val = _redisOper.Get(realSession);
            return val?.Split('_');
        }
    }
}
