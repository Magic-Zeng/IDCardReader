using IDCardApp.Entity;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace IDCardApp.Repository
{
    public class CardRepo : BaseRepo<CardInfo>
    {
        public CardRepo(DbContext context) : base(context)
        { }

        public CardInfo FindByCardNo(string cardNo)
        {
            return Find(t => t.CardNo == cardNo).FirstOrDefault();
        }

        public IList<UserCardDto> QueryUserCard(string key)
        {
            var sql = @"SELECT
	                c.PkId CardId,
	                c.CardNo,
	                c.CardType,
	                InvalidDate,
	                u.PkId UserId,
	                u.Name UserName,
	                u.Phone UserPhone,
	                u.IdentityNumber,
	                u.Room UserRoom
                FROM
	                CardInfo c
                LEFT JOIN UserInfo u ON c.User = u.PkId";
            var args = new List<object>();
            if(!string.IsNullOrEmpty(key))
            {
                sql += " WHERE c.CardNo = {0} or u.Name = {0} or u.IdentityNumber = {0} or u.Phone= {0}";
                args.Add(key);
            }
            return ctx.Database.SqlQuery<UserCardDto>(sql, args.ToArray()).ToList();
        }
    }
}
