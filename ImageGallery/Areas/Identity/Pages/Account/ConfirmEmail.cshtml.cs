using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using GalleryDatabase.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace GalleryDatabase.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<GalleryOwner> _userManager;

        private GalleryDbContext _context;

        public ConfirmEmailModel(UserManager<GalleryOwner> userManager, GalleryDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                var defaultAlbum = new Album
                {
                    Name = "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDL" +
                            "WhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko" +
                            "9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa",
                    GalleryOwnerId = user.Id,
                    AlbumAccessibility = Accessibility.Private,
                    DateCreated = DateTime.Now
                };
                _context.Albums.Add(defaultAlbum);
                await _context.SaveChangesAsync();
            }
            StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            return Page();
        }
    }
}
