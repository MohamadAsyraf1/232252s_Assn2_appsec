using _232252s_Assn2_appsec.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _232252s_Assn2_appsec.Pages
{


    public class SessionCheckModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public SessionCheckModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public IActionResult OnGet()
        {
            bool isAuthenticated = _signInManager.IsSignedIn(User);

            // If the user is not authenticated, log them out
            if (!isAuthenticated)
            {
                _signInManager.SignOutAsync();
            }

            return new JsonResult(new { isAuthenticated });
        }

    }
}
