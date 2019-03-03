using IDCardApp.Entity;
using IDCardApp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace IDCardApp
{
    class CmdRouter
    {
        public static string RepoRoute(string key, string arg = null)
        {
            string str = null;

            var arr = key.Split('.');
            if (arr[0] != "repo") return str;

            using (var ctx = new SqliteDbContext())
            {
                if (arr[1] == "building")
                {
                    str = new BaseRepo<Building>(ctx).ParseCmd(arr[2], arg);
                }
                else if(arr[1] == "device")
                {
                    str = new BaseRepo<Device>(ctx).ParseCmd(arr[2], arg);
                }
                else if(arr[1] == "userinfo")
                {
                    str = new BaseRepo<UserInfo>(ctx).ParseCmd(arr[2], arg);
                }
                else if(arr[1] == "cardinfo")
                {
                    var repo = new CardRepo(ctx);
                    if(arr[2] == "fetchByCardNo")
                    {
                        str = JsonUtil.Stringify(repo.FindByCardNo(arg));
                    }
                    else if(arr[2] == "queryUserCard")
                    {
                        str = JsonUtil.Stringify(repo.QueryUserCard(arg));
                    }
                }
                if(arr[2].StartsWith("fetch") == false)
                    ctx.SaveChanges();
            }

            return str;
        }
    }
}
