namespace IDCardApp.Entity
{
    public class CardAuth : BaseEntity
    {
        public string Card { get; set; }
        public string Device { get; set; }
        public string UpdateTime { get; set; }
    }
}
