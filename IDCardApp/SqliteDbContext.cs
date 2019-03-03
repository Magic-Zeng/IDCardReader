using IDCardApp.Entity;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace IDCardApp
{
    public class SqliteDbContext : DbContext
    {
        public SqliteDbContext() : base("IDCardConn")
        { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //处理表名变复数的问题
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Building>();
            modelBuilder.Entity<Device>();
            modelBuilder.Entity<UserInfo>();
            modelBuilder.Entity<CardInfo>();
            modelBuilder.Entity<CardAuth>();
        }
    }
}
