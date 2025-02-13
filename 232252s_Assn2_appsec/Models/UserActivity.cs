namespace _232252s_Assn2_appsec.Models
{
    public class UserActivity
    {
        public int Id { get; set; }
        public string UserId { get; set; }  // Foreign key to AspNetUsers
        public DateTime ActivityDate { get; set; }
        public string ActivityType { get; set; }  // e.g., "Login", "Password Change"
        public string Description { get; set; }  // Optional additional info

        public virtual ApplicationUser User { get; set; }  // Navigation property
    }

}
