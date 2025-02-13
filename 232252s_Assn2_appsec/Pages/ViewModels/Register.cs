using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace _232252s_Assn2_appsec.Pages.ViewModels
{
    public class Register
    {
        
   
        [Required]
        [DataType(DataType.Text)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Text)]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.CreditCard)]
        public string CreditCard { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string Mobile { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string BillingAddress { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Text)]
        public string ShippingAddress { get; set; }

        [Required]
        [DataType(DataType.ImageUrl)]
        public string Photo { get; set; } = string.Empty;



        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(12, ErrorMessage = "Enter at least a 12 characters password")]
      
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{8,}$",ErrorMessage = "Passwords must be at least 8 characters long and contain at least an uppercase letter, lower case letter, digit and a symbol")]

        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password does not match")]
        public string ConfirmPassword { get; set; }
    }

}
