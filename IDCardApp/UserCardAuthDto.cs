using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDCardApp
{
    public class UserCardAuthDto
    {
        public Entity.UserInfo User { get; set; }

        public Entity.CardInfo Card { get; set; }

        public IList<Entity.Device> Devices { get; set; }

        public string GetAuthStr()
        {
            var cardType = Card.CardType == "A" ? "01" : "02";
            DateTime invalidDate = DateTime.Today.AddYears(1);
            if(!string.IsNullOrEmpty(Card.InvalidDate))
            {
                invalidDate = DateTime.Parse(Card.InvalidDate);
            }
            
            return string.Format("{0}{1:yyyyMMddHHmmss}{2}",
                cardType, invalidDate, string.Join("", Devices.Select(d => d.Code)));
        }
    }
}
