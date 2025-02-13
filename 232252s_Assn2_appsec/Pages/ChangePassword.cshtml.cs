using _232252s_Assn2_appsec.Models;
using _232252s_Assn2_appsec.Pages.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;

namespace _232252s_Assn2_appsec.Pages
{
    public class ChangePasswordModel : PageModel
    {
        [BindProperty]
        public ChangePassword changePassword { get; set; }

        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AuthDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string ReCaptchaSecretKey = "6LcSB9UqAAAAAHmLJyd4Of85mzv3r2G_7cbBK-yj"; // Replace with your reCAPTCHA Secret Key

        public ChangePasswordModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, AuthDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!await ValidateCaptcha(Request.Form["g-recaptcha-response"]))
            {
                ModelState.AddModelError("", "reCAPTCHA verification failed. Please try again.");
                return Page();
            }
            if (ModelState.IsValid)
            {
                var email = HttpContext.Session.GetString("Email");
                if (string.IsNullOrEmpty(email))
                {
                    ModelState.AddModelError("", "User not logged in.");
                    return Page();
                }

                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return Page();
                }

                // Prevent changing password too soon (e.g., within 10 minutes)
                var minPasswordAge = TimeSpan.FromMinutes(1);
                if (DateTime.UtcNow.AddHours(8) - user.LastPasswordChange < minPasswordAge)
                {
                    ModelState.AddModelError("", "You cannot change your password yet. Please wait a while.");
                    return Page();
                }

                var passwordCheck = await userManager.CheckPasswordAsync(user, changePassword.Originalpassword);
                if (!passwordCheck)
                {
                    ModelState.AddModelError("", "Incorrect original password.");
                    return Page();
                }

                var passwordHasher = new PasswordHasher<ApplicationUser>();

                // ? Check if the new password matches any of the previous hashes
                if (passwordHasher.VerifyHashedPassword(user, user.PreviousPasswordHash1, changePassword.NewPassword) == PasswordVerificationResult.Success ||
                    passwordHasher.VerifyHashedPassword(user, user.PreviousPasswordHash2, changePassword.NewPassword) == PasswordVerificationResult.Success)
                {
                    ModelState.AddModelError("", "You cannot reuse your previous passwords.");
                    return Page();
                }
                // ? Change Password
                var result = await userManager.ChangePasswordAsync(user, changePassword.Originalpassword, changePassword.NewPassword);
                if (result.Succeeded)
                {
                    // ? Shift previous passwords
                    user.PreviousPasswordHash2 = user.PreviousPasswordHash1; // Move older password
                    user.PreviousPasswordHash1 = user.PasswordHash; // Store the current password before changing

                    await userManager.UpdateAsync(user); // Save previous password hashes
                    await signInManager.RefreshSignInAsync(user); // Ensure user stays logged in

                    // Update last password change timestamp
                    user.LastPasswordChange = DateTime.UtcNow.AddHours(8);

                    
                    return RedirectToPage("/Index", new { Message = "Password changed successfully." });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                
            }

            return Page();
        }



        public void OnGet()
        {
        }

        private async Task<bool> ValidateCaptcha(string captchaResponse)
        {
            using (HttpClient client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "secret", ReCaptchaSecretKey },
                    { "response", captchaResponse }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                var json = JsonSerializer.Deserialize<ReCaptchaResponse>(jsonResponse);
                return json.success && json.score >= 0.5; // Acceptable threshold score
            }
        }
        public class ReCaptchaResponse
        {
            public bool success { get; set; }
            public double score { get; set; }
            public string action { get; set; }
            public string challenge_ts { get; set; }
            public string hostname { get; set; }
        }
    }

    
}

