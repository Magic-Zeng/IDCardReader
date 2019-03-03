using IDCardApp.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Utils;

namespace IDCardApp.Repository
{
    public class BaseRepo<T> where T: BaseEntity
    {
        protected DbContext ctx = null;
        public BaseRepo(DbContext context)
        {
            ctx = context;
        }
        
        public IList<T> Fetch()
        {
            return ctx.Set<T>().ToList();
        }

        public T Find(string key)
        {
            return ctx.Set<T>().Find(key);
        }

        public IList<T> Find(Func<T, bool> filter)
        {
            if(filter == null)
            {
                return Fetch();
            }
            return ctx.Set<T>().Where(filter).ToList();
        }

        public void Add(T entity)
        {
            ctx.Set<T>().Add(entity);
        }

        public void Delete(string key)
        {
            var entity = Find(key);
            if (entity != null)
            {
                Delete(entity);
            }
        }

        public void Delete(T entity)
        {
            ctx.Set<T>().Remove(entity);
        }

        public void Update(T entity)
        {
            ctx.Entry(entity).State = EntityState.Modified;
        }
        
        public string ParseCmd(string cmd, string arg)
        {
            if(cmd == "fetch")
            {
                return JsonUtil.Stringify(Fetch());
            }
            if(cmd == "create")
            {
                var entity = JsonUtil.Parse<T>(arg);
                entity.NewId();
                Add(entity);
                return JsonUtil.Stringify(entity.PkId);
            }
            if(cmd == "destory")
            {
                foreach(var id in arg.Split(','))
                {
                    Delete(id);
                }
            }
            if(cmd == "update")
            {
                var entity = JsonUtil.Parse<T>(arg);
                Update(entity);
            }
            return null;
           
        }
    }
}
