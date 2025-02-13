using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.IO;
using _232252s_Assn2_appsec.Models;
using _232252s_Assn2_appsec.Pages.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;

namespace _232252s_Assn2_appsec.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AuthDbContext _context;

        private const string ReCaptchaSecretKey = "6LcSB9UqAAAAAHmLJyd4Of85mzv3r2G_7cbBK-yj"; // Replace with your reCAPTCHA Secret Key

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, AuthDbContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _context = context;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate reCAPTCHA
            if (!await ValidateCaptcha(Request.Form["g-recaptcha-response"]))
            {
                ModelState.AddModelError("", "reCAPTCHA verification failed. Please try again.");
                return Page();
            }

            if (ModelState.IsValid)
            {
                var user = await signInManager.UserManager.FindByEmailAsync(LModel.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Username or Password is incorrect.");
                    return Page();
                }

                var identityResult = await signInManager.PasswordSignInAsync(
                    LModel.Email,
                    LModel.Password,
                    LModel.RememberMe,
                    lockoutOnFailure: true
                );

                if (identityResult.Succeeded)
                {
                    var userActivity = new UserActivity
                    {
                        UserId = user.Id,
                        ActivityDate = DateTime.UtcNow.AddHours(8),
                        ActivityType = "Login",
                        Description = "User logged in successfully"
                    };
                    _context.UserActivities.Add(userActivity);
                    await _context.SaveChangesAsync();

                    // Store user details in session
                    HttpContext.Session.SetString("Email", user.Email);
                    HttpContext.Session.SetString("FirstName", user.FirstName);
                    HttpContext.Session.SetString("LastName", user.LastName);
                    HttpContext.Session.SetString("Mobile", user.Mobile);
                    HttpContext.Session.SetString("BillingAddress", user.BillingAddress);
                    HttpContext.Session.SetString("ShippingAddress", user.ShippingAddress);
                    HttpContext.Session.SetString("Photo", user.Photo ?? ""); // Handle null photos

                    // Encrypt and store credit card info
                    var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                    var protector = dataProtectionProvider.CreateProtector("MySecretKey");
                    HttpContext.Session.SetString("CreditCard", protector.Protect(user.CreditCard));

                    // Create authentication claims
                    var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim("Mobile", user.Mobile),
                new Claim("BillingAddress", user.BillingAddress),
                new Claim("ShippingAddress", user.ShippingAddress)
            };
                    var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

                    return RedirectToPage("Index");
                }

                if (identityResult.IsLockedOut)
                {
                    ModelState.AddModelError("", "Your account is locked due to multiple failed login attempts. Please try again later.");
                    return Page();
                }

                ModelState.AddModelError("", "Username or Password is incorrect.");
            }

            return Page();
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
