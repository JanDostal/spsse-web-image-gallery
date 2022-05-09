using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GalleryDatabase.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace GalleryDatabase.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<GalleryOwner> _userManager;
        private readonly SignInManager<GalleryOwner> _signInManager;
        private readonly IWebHostEnvironment _environment;


        public IndexModel(
            UserManager<GalleryOwner> userManager,
            SignInManager<GalleryOwner> signInManager,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public IFormFile Upload { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public GalleryOwner GalleryOwner { get; set; }

        public string DirectoryPath { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }


            [Required]
            [RegularExpression("^[a-zA-Z0-9 @ .]*$", ErrorMessage = "Username must be readable for other users.")]
            [StringLength(60)]
            [Display(Name = "User name")]
            public string UserNameForComments { get; set; }
        }

        public string GetUserId()
        {
            return HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? default;
        }

        private async Task LoadAsync(GalleryOwner user)
        {
            var username = user.UserNameForComments;
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                UserNameForComments = username
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            GalleryOwner = await _userManager.GetUserAsync(User);
            if (GalleryOwner == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(GalleryOwner);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            GalleryOwner = await _userManager.GetUserAsync(User);
            if (GalleryOwner == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(GalleryOwner);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(GalleryOwner);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(GalleryOwner, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Error when trying to set phone number.";
                    return RedirectToPage();
                }

            }

            var username = GalleryOwner.UserNameForComments;
            if (Input.UserNameForComments != username)
            {
                GalleryOwner.UserNameForComments = Input.UserNameForComments;
                await _userManager.UpdateAsync(GalleryOwner);
            }

            await _signInManager.RefreshSignInAsync(GalleryOwner);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostProfileImageUpload()
        {
            GalleryOwner = await _userManager.GetUserAsync(User);
            if (GalleryOwner == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            int successfulProcessing = 0;
            int failedProcessing = 0;

            GalleryOwner.ProfileImageSize = Upload.Length;
            GalleryOwner.ProfileImageContentType = Upload.ContentType;
            GalleryOwner.ProfileImageOriginalName = Upload.FileName;
            GalleryOwner.ProfileImageUploadedAt = DateTime.Now;

            await _userManager.UpdateAsync(GalleryOwner);

            try
            {
                if (GalleryOwner.ProfileImageContentType.Contains("xml") == true || GalleryOwner.ProfileImageContentType.StartsWith("image") == false ||
                    GalleryOwner.ProfileImageSize == null)
                {
                    GalleryOwner.ProfileImageSize = null;
                    GalleryOwner.ProfileImageContentType = null;
                    GalleryOwner.ProfileImageOriginalName = null;
                    GalleryOwner.ProfileImageUploadedAt = null;
                    await _userManager.UpdateAsync(GalleryOwner);
                    throw new Exception();
                }

                FileStream fileStream;
                MemoryStream ims = new MemoryStream();
                MemoryStream preservedAspectRatioForStandardComment = new MemoryStream();
                MemoryStream preservedAspectRatioForCommentedComment = new MemoryStream();

                DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", GalleryOwner.Id);

                Upload.CopyTo(ims);
                using (fileStream = new FileStream(DirectoryPath, FileMode.Create))
                {
                    await Upload.CopyToAsync(fileStream);
                };

                SixLabors.ImageSharp.Image img = SixLabors.ImageSharp.Image.Load(DirectoryPath, out IImageFormat format);
                SixLabors.ImageSharp.Image pic;

                
                    using (pic = SixLabors.ImageSharp.Image.Load(ims.ToArray()))
                    {
                        if (pic.Width > pic.Height)
                        {
                            pic.Mutate(x => x.Resize(0, 320));
                        }
                        else
                        {
                            pic.Mutate(x => x.Resize(320, 0));
                        }

                        pic.Mutate(x => x.Crop(new Rectangle((pic.Width - 320) / 2, (pic.Height - 320) / 2, 320, 320)));
                        System.IO.File.Delete(DirectoryPath);
                        using (fileStream = new FileStream(DirectoryPath, FileMode.Create))
                        {
                            pic.Save(fileStream, format);
                            GalleryOwner.ProfileImageSize = fileStream.Length;
                        }

                        GalleryOwner.ProfileImageHeight = pic.Height;
                        GalleryOwner.ProfileImageWidth = pic.Width;
                    }
                

                using (pic = SixLabors.ImageSharp.Image.Load(ims.ToArray()))
                {
                    if (pic.Width > pic.Height)
                    {
                        pic.Mutate(x => x.Resize(0, 40));
                    }
                    else
                    {
                        pic.Mutate(x => x.Resize(40, 0));
                    }

                    pic.Mutate(x => x.Crop(new Rectangle((pic.Width - 40) / 2, (pic.Height - 40) / 2, 40, 40)));

                    pic.SaveAsJpeg(preservedAspectRatioForStandardComment, new JpegEncoder {Quality = 100});

                    SixLabors.ImageSharp.Image.Load(preservedAspectRatioForStandardComment.ToArray(), out format);

                    GalleryOwner.ThumbnailAspectRatio = (double)pic.Width / pic.Height;
                    GalleryOwner.ThumbnailContentType = format.Name;
                    GalleryOwner.ThumbnailForStandardCommentHeight = pic.Height;
                    GalleryOwner.ThumbnailForStandardCommentWidth = pic.Width;
                    GalleryOwner.ThumbnailForStandardCommentBlob = preservedAspectRatioForStandardComment.ToArray();
                }

                using (pic = SixLabors.ImageSharp.Image.Load(ims.ToArray()))
                {
                    if (pic.Width > pic.Height)
                    {
                        pic.Mutate(x => x.Resize(0, 32));
                    }
                    else
                    {
                        pic.Mutate(x => x.Resize(32, 0));
                    }

                    pic.Mutate(x => x.Crop(new Rectangle((pic.Width - 32) / 2, (pic.Height - 32) / 2, 32, 32)));

                    pic.SaveAsJpeg(preservedAspectRatioForCommentedComment, new JpegEncoder { Quality = 100});

                    SixLabors.ImageSharp.Image.Load(preservedAspectRatioForCommentedComment.ToArray(), out format);

                    GalleryOwner.ThumbnailAspectRatio = (double)pic.Width / pic.Height;
                    GalleryOwner.ThumbnailContentType = format.Name;
                    GalleryOwner.ThumbnailForCommentedCommentHeight = pic.Height;
                    GalleryOwner.ThumbnailForCommentedCommentWidth = pic.Width;
                    GalleryOwner.ThumbnailForCommentedCommentBlob = preservedAspectRatioForCommentedComment.ToArray();
                }

                await _userManager.UpdateAsync(GalleryOwner);
                successfulProcessing++;
            }
            catch
            {
                DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", GalleryOwner.Id);
                System.IO.File.Delete(DirectoryPath);
                failedProcessing++;
            }
            if (failedProcessing == 0)
            {
                SuccessMessage = "Image has been uploaded successfuly.";
            }
            else
            {
                ErrorMessage = "There were " + failedProcessing + " errors during uploading and processing of image.";
            }
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetProfileImage()
        {
            GalleryOwner = await _userManager.GetUserAsync(User);
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", GalleryOwner.Id);

            byte[] result = System.IO.File.ReadAllBytes(DirectoryPath);
            return File(result, GalleryOwner.ProfileImageContentType);
        }

        public IActionResult OnGetDownloadProfileImage ()
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }
            else if (GalleryOwner.ProfileImageSize == null)
            {
                ErrorMessage = "There is nothing to download.";
            }
            else
            {
                DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", GalleryOwner.Id);
                if (System.IO.File.Exists(DirectoryPath))
                {
                    return PhysicalFile(DirectoryPath, GalleryOwner.ProfileImageContentType, GalleryOwner.ProfileImageOriginalName);
                }
                else
                {
                    return RedirectToPage("Error");
                }
            }

            return RedirectToPage("./Index");
        }


        public async Task<IActionResult> OnGetDeleteProfileImage ()
        {
            GalleryOwner = await _userManager.GetUserAsync(User);

            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            if (GalleryOwner.ProfileImageSize == null)
            {
                ErrorMessage = "There is nothing to delete.";
            }
            else
            {
                DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", GalleryOwner.Id);

                GalleryOwner.ThumbnailForStandardCommentHeight = null;
                GalleryOwner.ThumbnailForStandardCommentWidth = null;
                GalleryOwner.ThumbnailForCommentedCommentWidth = null;
                GalleryOwner.ThumbnailForCommentedCommentHeight = null;
                GalleryOwner.ThumbnailContentType = null;
                GalleryOwner.ThumbnailForStandardCommentBlob = null;
                GalleryOwner.ThumbnailForCommentedCommentBlob = null;
                GalleryOwner.ThumbnailAspectRatio = null;
                GalleryOwner.ProfileImageContentType = null;
                GalleryOwner.ProfileImageHeight = null;
                GalleryOwner.ProfileImageOriginalName = null;
                GalleryOwner.ProfileImageSize = null;
                GalleryOwner.ProfileImageUploadedAt = null;
                GalleryOwner.ProfileImageWidth = null;

                System.IO.File.Delete(DirectoryPath);

                await _userManager.UpdateAsync(GalleryOwner);
            }

            return RedirectToPage("./Index");
        }
    }

}
