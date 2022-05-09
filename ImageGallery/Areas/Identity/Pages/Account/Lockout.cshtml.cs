using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalleryDatabase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GalleryDatabase.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LockoutModel : PageModel
    {
        private readonly UserManager<GalleryOwner> _userManager; 

        public LockoutModel (UserManager<GalleryOwner> usermanager)
        {
            _userManager = usermanager;
        }


        public int RemainingTime { get; set; }

        public async Task<IActionResult> OnGet(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var result = await _userManager.IsLockedOutAsync(user);

            if (user == null)
            {
                return NotFound("User was not found.");
            }
            else if (result == false)
            {
                return NotFound("User is not locked out.");
            }

            var s = user.LockoutEnd;
            var d =DateTime.Parse(s.ToString());

            RemainingTime = (int) (d.ToLocalTime().Subtract(DateTime.Now)).TotalSeconds;
        

            return Page();

        }
    }
}
