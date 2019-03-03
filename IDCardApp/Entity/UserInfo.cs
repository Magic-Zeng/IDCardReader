namespace IDCardApp.Entity
{
    public class UserInfo : BaseEntity
    {
        public string IdentityNumber { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Room { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }
        public string Birth { get; set; }
        public string Address { get; set; }
        public string IssueOffice { get; set; }
        public string ValidBegin { get; set; }
        public string ValidEnd { get; set; }
        public string IdentityPhoto { get; set; }
        public string GatherPhoto { get; set; }
        public string CreateTime { get; set; }

        public void SetIdentityPhotoWebPath()
        {
            if(this.IdentityNumber != null)
                this.IdentityPhoto = "../Data/photos/" + this.IdentityNumber + ".bmp";
        }
    }
}
