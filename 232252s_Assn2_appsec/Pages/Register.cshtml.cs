using _232252s_Assn2_appsec.Models;
using _232252s_Assn2_appsec.Pages.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing;
using System.Text.RegularExpressions;

namespace _232252s_Assn2_appsec.Pages
{
    public class RegisterModel : PageModel
    {
        private UserManager<ApplicationUser> userManager { get; }
        private SignInManager<ApplicationUser> signInManager { get; }

        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {

                var existingUser = await userManager.FindByEmailAsync(RModel.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already exists. Please use a different email.");
                    return Page();
                }

                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");

                // Hash password before storing


                var user = new ApplicationUser()
                {
                    UserName = RModel.Email,
                    Email = RModel.Email,
                    FirstName = RModel.FirstName,
                    LastName = RModel.LastName,
                    CreditCard = protector.Protect(RModel.CreditCard),
                    Mobile = RModel.Mobile,
                    BillingAddress = RModel.BillingAddress,
                    ShippingAddress = RModel.ShippingAddress,
                    Photo = RModel.Photo,

                    // If tracking two previous passwords
                    PreviousPasswordHash1 = "",  // Default empty string
                    PreviousPasswordHash2 = "", // Default empty string

                    LastPasswordChange = DateTime.UtcNow.AddHours(8)




                };
                var result = await userManager.CreateAsync(user, RModel.Password);
                if (result.Succeeded)
                {
                    user.PreviousPasswordHash1 = user.PasswordHash;
                    user.PreviousPasswordHash2 = user.PasswordHash;

                    //await userManager.SetTwoFactorEnabledAsync(user, true);

                    await userManager.UpdateAsync(user); // Ensure it gets saved
                    await signInManager.SignInAsync(user, false);
                    HttpContext.Session.SetString("Email", user.Email);
                    HttpContext.Session.SetString("FirstName", user.FirstName);
                    HttpContext.Session.SetString("LastName", user.LastName);
                    HttpContext.Session.SetString("Mobile", user.Mobile);
                    HttpContext.Session.SetString("BillingAddress", user.BillingAddress);
                    HttpContext.Session.SetString("ShippingAddress", user.ShippingAddress);
                    return RedirectToPage("Index");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("",
                error.Description);
                }
            }
            return Page();
        }

       
        
    }
}
