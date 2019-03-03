using IDCardApp.Entity;
using IDCardApp.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace IDCardApp
{
    public class UserCardAuth
    {
        DbContext dbCtx = null;
        public UserCardAuth()
        {
            dbCtx = new SqliteDbContext();
        }

        public void Auth(UserCardAuthDto dto)
        {
            if (dto.User == null || dto.Card == null) return;

            AddOrUpdateUser(dto.User);
            dto.Card.User = dto.User.PkId;
            AddOrUpdateCard(dto.Card);
            UpdateAuth(dto.Card.PkId, dto.Devices);

            dbCtx.SaveChanges();
        }

        private void AddOrUpdateUser(UserInfo user)
        {
            var repo = new BaseRepo<UserInfo>(dbCtx);
            UserInfo existsUser = null;
            if (!string.IsNullOrEmpty(user.PkId))
            {
                existsUser = repo.Find(user.PkId);
            }
            else if (!string.IsNullOrEmpty(user.IdentityNumber))
            {
                existsUser = repo.Find(t => t.IdentityNumber == user.IdentityNumber).FirstOrDefault();
            }

            if(existsUser == null)
            {
                user.NewId();
                repo.Add(user);
            }
            else
            {
                existsUser.Name = user.Name;
                existsUser.Phone = user.Phone;
                existsUser.IdentityNumber = user.IdentityNumber;
                //todo: other fields
                user.PkId = existsUser.PkId;
                repo.Update(existsUser);
            }
        }

        private void AddOrUpdateCard(CardInfo card)
        {
            var repo = new BaseRepo<CardInfo>(dbCtx);
            CardInfo existsCard = null;
            if(!string.IsNullOrEmpty(card.PkId))
            {
                existsCard = repo.Find(card.PkId);
            }
            else if(!string.IsNullOrEmpty(card.CardNo))
            {
                existsCard = repo.Find(t => t.CardNo == card.CardNo).FirstOrDefault();
            }

            card.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (existsCard == null)
            {
                card.NewId();
                repo.Add(card);
            }
            else
            {
                existsCard.UpdateTime = card.UpdateTime;
                existsCard.User = card.User;
                existsCard.SerialNo = card.SerialNo;
                existsCard.CardType = card.CardType;
                existsCard.InvalidDate = card.InvalidDate;
                repo.Update(existsCard);
                card.PkId = existsCard.PkId;
            }
        }

        private void UpdateAuth(string cardPkId, IList<Device> devices)
        {
            var repo = new BaseRepo<CardAuth>(dbCtx);
            var existsList = repo.Find(t => t.Card == cardPkId);
            foreach(var e in existsList)
            {
                repo.Delete(e);
            }

            if(devices != null && devices.Count > 0)
            {
                var updateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                foreach(var dev in devices)
                {
                    var entity = new CardAuth()
                    {
                        Card = cardPkId,
                        Device = dev.PkId,
                        UpdateTime = updateTime
                    };
                    entity.NewId();
                    repo.Add(entity);
                }
            }
        }

        public UserCardAuthDto GetByCardNo(string cardNo)
        {
            var dto = new UserCardAuthDto();
            dto.Card = new BaseRepo<CardInfo>(dbCtx)
                .Find(t => t.CardNo == cardNo).FirstOrDefault();
            if(dto.Card == null)
            {
                dto.Card = new CardInfo() { CardNo = cardNo };
            }
            else
            {
                dto.User = new BaseRepo<UserInfo>(dbCtx).Find(dto.Card.User);
                var deviceIds = new BaseRepo<CardAuth>(dbCtx)
                    .Find(t => t.Card == dto.Card.PkId)
                    .Select(t => t.Device)
                    .ToArray();
                dto.Devices = new BaseRepo<Device>(dbCtx)
                    .Find(t => deviceIds.Contains(t.PkId))
                    .ToList();
            }
            return dto;
        }

        public UserCardAuthDto GetByCardIdOrUserId(string cardId, string userId)
        {
            var dto = new UserCardAuthDto();
            if(!string.IsNullOrEmpty(cardId))
            {
                dto.Card = new BaseRepo<CardInfo>(dbCtx).Find(cardId);
                if(dto.Card != null)
                {
                    dto.User = new BaseRepo<UserInfo>(dbCtx).Find(dto.Card.User);
                    var deviceIds = new BaseRepo<CardAuth>(dbCtx)
                        .Find(t => t.Card == dto.Card.PkId)
                        .Select(t => t.Device)
                        .ToArray();
                    dto.Devices = new BaseRepo<Device>(dbCtx)
                        .Find(t => deviceIds.Contains(t.PkId))
                        .ToList();
                }
            }
            else if(!string.IsNullOrEmpty(userId))
            {
                dto.User = new BaseRepo<UserInfo>(dbCtx).Find(userId);                
            }
            return dto;
        }
    }
}
