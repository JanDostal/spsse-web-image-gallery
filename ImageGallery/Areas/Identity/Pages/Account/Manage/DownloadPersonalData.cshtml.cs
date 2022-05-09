using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GalleryDatabase.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace GalleryDatabase.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<GalleryOwner> _userManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;

        public DownloadPersonalDataModel(
            UserManager<GalleryOwner> userManager,
            ILogger<DownloadPersonalDataModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

            // Only include personal data for download
            Dictionary<string, string> personalData = new Dictionary<string, string>();

            var Id = typeof(GalleryOwner).GetProperty("Id");
            var userNameForComments = typeof(GalleryOwner).GetProperty("UserNameForComments");
            var dateCreated = typeof(GalleryOwner).GetProperty("DateCreated");
            var email = typeof(GalleryOwner).GetProperty("Email");
            var normalizedEmail = typeof(GalleryOwner).GetProperty("NormalizedEmail");
            var passwordHash = typeof(GalleryOwner).GetProperty("PasswordHash");
            var securityStamp = typeof(GalleryOwner).GetProperty("SecurityStamp");
            var concurrencyStamp = typeof(GalleryOwner).GetProperty("ConcurrencyStamp");
            var twoFactorEnabled = typeof(GalleryOwner).GetProperty("TwoFactorEnabled");
            var lockoutEnabled = typeof(GalleryOwner).GetProperty("LockoutEnabled");
            var accessFailedCount = typeof(GalleryOwner).GetProperty("AccessFailedCount");
            var emailConfirmed = typeof(GalleryOwner).GetProperty("EmailConfirmed");
            var lockoutEnd = typeof(GalleryOwner).GetProperty("LockoutEnd");


            personalData.Add(Id.Name, Id.GetValue(user)?.ToString() ?? "null");
            personalData.Add(userNameForComments.Name, userNameForComments.GetValue(user)?.ToString() ?? "null");
            personalData.Add(email.Name, email.GetValue(user)?.ToString() ?? "null");
            personalData.Add(normalizedEmail.Name, normalizedEmail.GetValue(user)?.ToString() ?? "null");
            personalData.Add(emailConfirmed.Name, emailConfirmed.GetValue(user)?.ToString() ?? "null");
            personalData.Add(dateCreated.Name, dateCreated.GetValue(user)?.ToString() ?? "null");
            personalData.Add(passwordHash.Name, passwordHash.GetValue(user)?.ToString() ?? "null");
            personalData.Add(securityStamp.Name, securityStamp.GetValue(user)?.ToString() ?? "null");
            personalData.Add(concurrencyStamp.Name, concurrencyStamp.GetValue(user)?.ToString() ?? "null");
            personalData.Add(twoFactorEnabled.Name, twoFactorEnabled.GetValue(user)?.ToString() ?? "null");
            personalData.Add(lockoutEnabled.Name, lockoutEnabled.GetValue(user)?.ToString() ?? "null");
            personalData.Add(lockoutEnd.Name, lockoutEnd.GetValue(user)?.ToString() ?? "null");
            personalData.Add(accessFailedCount.Name, accessFailedCount.GetValue(user)?.ToString() ?? "null");


            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var l in logins)
            {
                personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
        }
    }
}
