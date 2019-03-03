namespace IDCardApp.Entity
{
    public class CardInfo : BaseEntity
    {
        /// <summary>
        /// 卡号，唯一
        /// </summary>
        public string CardNo { get; set; }
        /// <summary>
        /// 序列号，暂时不用
        /// </summary>
        public string SerialNo { get; set; }
        public string User { get; set; }
        public string UpdateTime { get; set; }

        /// <summary>
        /// 失效日期
        /// </summary>
        public string InvalidDate { get; set; }

        /// <summary>
        /// 卡片类型
        /// </summary>
        public string CardType { get; set; }
    }
}
