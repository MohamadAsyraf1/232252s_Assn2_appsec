using Microsoft.AspNetCore.Identity;

namespace _232252s_Assn2_appsec.Models
{
    public class ApplicationUser : IdentityUser

    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string CreditCard { get; set; }
        public string Mobile { get; set; }
        public string BillingAddress { get; set; }

        public string ShippingAddress { get; set; }
        public string Photo { get; set; }
       
        public string PreviousPasswordHash1 { get; set; }
        public string PreviousPasswordHash2 { get; set; }
        public DateTime LastPasswordChange { get; set; }
        




    }
}
