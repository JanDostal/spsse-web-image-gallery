using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GalleryDatabase.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GalleryDatabase.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private UserManager<GalleryOwner> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly SignInManager<GalleryOwner> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private GalleryDbContext _context;

        public DeletePersonalDataModel(
            UserManager<GalleryOwner> userManager,
            SignInManager<GalleryOwner> signInManager,
            GalleryDbContext context,
            ILogger<DeletePersonalDataModel> logger,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public string GetUserId()
        {
            return HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? default;
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.Users.
                Select(x => new GalleryOwner
                {
                    Albums = x.Albums,
                    Id = x.Id,
                    Email = x.Email,
                    AccessFailedCount = x.AccessFailedCount,
                    GalleryAccessibility = x.GalleryAccessibility,
                    ConcurrencyStamp = x.ConcurrencyStamp,
                    UserName = x.UserName,
                    TwoFactorEnabled = x.TwoFactorEnabled,
                    LockoutEnabled = x.LockoutEnabled,
                    UserNameForComments = x.UserNameForComments,
                    ThumbnailForStandardCommentWidth = x.ThumbnailForStandardCommentWidth,
                    ProfileImageUploadedAt = x.ProfileImageUploadedAt,
                    ThumbnailAspectRatio = x.ThumbnailAspectRatio,
                    DateCreated = x.DateCreated,
                    EmailConfirmed = x.EmailConfirmed,
                    LockoutEnd = x.LockoutEnd,
                    NormalizedEmail = x.NormalizedEmail,
                    NormalizedUserName = x.NormalizedUserName,
                    PasswordHash = x.PasswordHash,
                    PhoneNumber = x.PhoneNumber,
                    PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                    SecurityStamp = x.SecurityStamp,
                    ThumbnailContentType = x.ThumbnailContentType,
                    ThumbnailForCommentedCommentBlob = x.ThumbnailForCommentedCommentBlob,
                    ThumbnailForCommentedCommentHeight = x.ThumbnailForCommentedCommentHeight,
                    ThumbnailForCommentedCommentWidth = x.ThumbnailForCommentedCommentWidth,
                    ThumbnailForStandardCommentBlob = x.ThumbnailForStandardCommentBlob,
                    ThumbnailForStandardCommentHeight = x.ThumbnailForStandardCommentHeight,
                    ProfileImageWidth = x.ProfileImageWidth,
                    ProfileImageContentType = x.ProfileImageContentType,
                    ProfileImageHeight = x.ProfileImageHeight,
                    ProfileImageOriginalName = x.ProfileImageOriginalName,
                    ProfileImageSize = x.ProfileImageSize
                }).
                Where(x => x.Id == GetUserId()).SingleOrDefaultAsync();


            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            var images = _context.Images.
               Select(x => new Image
               {
                   AlbumId = x.AlbumId,
                   Album = x.Album,
                   Comments = x.Comments,
                   ImageAccessibility = x.ImageAccessibility,
                   UploadedAt = x.UploadedAt,
                   DateTaken = x.DateTaken,
                   ImageContentType = x.ImageContentType,
                   ImageHeight = x.ImageHeight,
                   ImageWidth = x.ImageWidth,
                   OriginalName = x.OriginalName,
                   Size = x.Size,
                   ImageId = x.ImageId,
               })
                .Where(p => p.Album.GalleryOwnerId == user.Id);

            string file;

            foreach (var item in images.AsEnumerable())
            {
                var imagecommentUsers = _context.CommentUsers.
                    Select(x => new CommentUser
                    {
                        Comment = x.Comment,
                        CommentId = x.CommentId,
                        ReactionType = x.ReactionType,
                        UserId = x.UserId,
                    })
                    .Where(x => x.Comment.ImageId == item.ImageId);

                _context.CommentUsers.RemoveRange(imagecommentUsers);

                var thumbnails = _context.Thumbnails.Where(x => x.ImageId == item.ImageId);

                _context.Thumbnails.RemoveRange(thumbnails);

                _context.Comments.RemoveRange(item.Comments.AsQueryable());
               
               file = Path.Combine(_environment.ContentRootPath, "Uploads", item.ImageId.ToString());
                _context.Images.Remove(item);
                System.IO.File.Delete(file);
            }

            await _context.SaveChangesAsync();

            _context.Albums.RemoveRange(user.Albums.AsQueryable());

            var userCommentUsers = _context.CommentUsers.Where(p => p.UserId == user.Id);

            foreach (var item in userCommentUsers.AsEnumerable())
            {
                var commentWithYourReaction = _context.Comments.Where(x => x.CommentId == item.CommentId).SingleOrDefault();


                _context.CommentUsers.Remove(item);

                if (item.ReactionType == CommentUser.Reaction.Like)
                {
                    commentWithYourReaction.Likes--;
                }
                else
                {
                    commentWithYourReaction.Dislikes--;
                }

                _context.Comments.Update(commentWithYourReaction);
            }

            var userComments = _context.Comments.
                Select(x => new Comment
                {
                    CommentedComment = x.CommentedComment,
                    CommentedCommentId = x.CommentedCommentId,
                    CommentId = x.CommentId,
                    Content = x.Content,
                    WasEdited = x.WasEdited,
                    ControversyScore = x.ControversyScore,
                    DatePosted = x.DatePosted,
                    Dislikes = x.Dislikes,
                    DateUserWasRemoved = x.DateUserWasRemoved,
                    Image = x.Image,
                    Likes = x.Likes,
                    ReactionScore = x.ReactionScore,
                    UserId = x.UserId,
                    ImageId = x.ImageId,
                    TotalWeightFromReactions = x.TotalWeightFromReactions,
                    CommentUsers = x.CommentUsers
                })
                .Where(x => x.UserId == user.Id);

            foreach (var item in userComments.AsEnumerable())
            {
                _context.CommentUsers.RemoveRange(item.CommentUsers);

                int numberOfReplies = _context.Comments.AsNoTracking().Where(x => x.CommentedCommentId == item.CommentId).ToList().Count;

                if (numberOfReplies > 0)
                {
                    item.Content = null;
                    item.ControversyScore = 0;
                    item.DateUserWasRemoved = DateTime.Now;
                    item.Dislikes = 0;
                    item.Likes = 0;
                    item.ReactionScore = 0;
                    item.TotalWeightFromReactions = 0;
                    item.UserId = null;
  
                    _context.Comments.Update(item);
                }
                else
                {
                    _context.Comments.Remove(item);
                }
            }

            file = Path.Combine(_environment.ContentRootPath, "Uploads", user.Id);
            System.IO.File.Delete(file);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{user.Id}'.");
            }

            await _context.SaveChangesAsync();
            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", user.Id);

            return Redirect("~/");
        }
    }
}
