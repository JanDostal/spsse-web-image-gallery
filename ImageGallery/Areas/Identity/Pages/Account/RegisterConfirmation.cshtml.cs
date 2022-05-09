using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Threading.Tasks;
using GalleryDatabase.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System;

namespace GalleryDatabase.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<GalleryOwner> _userManager;
        private readonly IEmailSender _sender;
        private GalleryDbContext _context;

        public RegisterConfirmationModel(UserManager<GalleryOwner> userManager, IEmailSender sender, GalleryDbContext context)
        {
            _userManager = userManager;
            _sender = sender;
            _context = context;

        }

        public string Email { get; set; }

        public bool DisplayConfirmAccountLink { get; set; }

        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;

            return Page();
        }
    }
}
