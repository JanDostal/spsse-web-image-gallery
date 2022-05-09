using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using GalleryDatabase.Models;
using GalleryDatabase.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace GalleryDatabase.Pages
{
    public class ReadOnlyGalleryModel : PageModel
    {
        private readonly UserManager<GalleryOwner> _userManager;
        private readonly DropdownList _dropdownList;
        private readonly GalleryDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly SortingHelper _helper;

        public List<SelectListItem> GalleryMethodOfSortingList { get; private set; }

        public List<SelectListItem> GalleryOrderByList { get; private set; }

        public List<SelectListItem> ImageList { get; private set; }

        public ReadOnlyGalleryModel(UserManager<GalleryOwner> userManager, GalleryDbContext context, IWebHostEnvironment environment, DropdownList dropdownList, SortingHelper helper)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
            _dropdownList = dropdownList;
            _helper = helper;
        }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public GalleryOwner GalleryOwner { get; set; }

        [BindProperty]
        public GalleryOwnerReadModel GalleryOwnerReadOnly { get; set; }

        public Album Album { get; set; }


        public Image Image { get; set; }

        public Thumbnail Thumbnail { get; set; }

        [BindProperty]
        public string GalleryMethod { get; set; }
        [BindProperty]
        public OrderBy GalleryOrderBy { get; set; }

        public int NumberOfAlbums { get; set; }

        public int NumberOfImages { get; set; }

        public int DefaultAlbumNumberOfImages { get; set; }

        public long DefaultAlbumTotalSize { get; set; }

        public string DirectoryPath { get; set; }

        public IEnumerable<Image> Images { get; set; }

        public IEnumerable<ImageReadModel> ImagesFromService { get; set; }

        public int Counter { get; set; }

        public IActionResult OnGetMoveToChosenImage (Guid imageId, string galleryOwnerEmail, string galleryMethod, OrderBy galleryOrderBy)
        {
            GalleryOwnerReadOnly = _userManager.Users.AsNoTracking().
                Select(a => new GalleryOwnerReadModel()
                {
                    UserNameForComments = a.UserNameForComments,
                    AccessFailedCount = a.AccessFailedCount,
                    Albums = a.Albums,
                    GalleryAccessibility = a.GalleryAccessibility,
                    ProfileImageUploadedAt = a.ProfileImageUploadedAt,
                    ThumbnailAspectRatio = a.ThumbnailAspectRatio,
                    ConcurrencyStamp = a.ConcurrencyStamp,
                    DateCreated = a.DateCreated,
                    Email = a.Email,
                    EmailConfirmed = a.EmailConfirmed,
                    Id = a.Id,
                    LockoutEnabled = a.LockoutEnabled,
                    LockoutEnd = a.LockoutEnd,
                    NormalizedEmail = a.NormalizedEmail,
                    NormalizedUserName = a.NormalizedUserName,
                    PasswordHash = a.PasswordHash,
                    PhoneNumber = a.PhoneNumber,
                    PhoneNumberConfirmed = a.PhoneNumberConfirmed,
                    ProfileImageContentType = a.ProfileImageContentType,
                    ProfileImageHeight = a.ProfileImageHeight,
                    ProfileImageOriginalName = a.ProfileImageOriginalName,
                    ProfileImageSize = a.ProfileImageSize,
                    ProfileImageWidth = a.ProfileImageWidth,
                    SecurityStamp = a.SecurityStamp,
                    ThumbnailContentType = a.ThumbnailContentType,
                    ThumbnailForCommentedCommentBlob = a.ThumbnailForCommentedCommentBlob,
                    ThumbnailForCommentedCommentHeight = a.ThumbnailForCommentedCommentHeight,
                    ThumbnailForCommentedCommentWidth = a.ThumbnailForCommentedCommentWidth,
                    ThumbnailForStandardCommentBlob = a.ThumbnailForStandardCommentBlob,
                    ThumbnailForStandardCommentHeight = a.ThumbnailForStandardCommentHeight,
                    ThumbnailForStandardCommentWidth = a.ThumbnailForStandardCommentWidth,
                    TwoFactorEnabled = a.TwoFactorEnabled,
                    UserName = a.UserName,
                    CurrentGallerySize = _context.Images.AsNoTracking().Where(c => c.Album.GalleryOwnerId == a.Id).Select(d => d.Size).Sum()
                }
                )
                .Where(x => x.Email == galleryOwnerEmail).SingleOrDefault();
            if (GalleryOwnerReadOnly == null)
            {
                return RedirectToPage("Error");
            }
            else if (GalleryOwnerReadOnly.GalleryAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwnerReadOnly.Id &&
           x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
           FirstOrDefault();

            Image = _context.Images.AsNoTracking().Where(p => p.AlbumId == Album.AlbumId && p.ImageId == imageId).SingleOrDefault();
            if (Image == null)
            {
                return RedirectToPage("Error");
            }
            else if (Image.ImageAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

            return RedirectToPage("/ReadOnlyGallery", null, new {galleryOwnerEmail = galleryOwnerEmail, method = galleryMethod, orderBy = galleryOrderBy }, $"publicImage{imageId}");
        }

        public IActionResult OnGet(string galleryOwnerEmail, string method, OrderBy orderBy)
        {
            GalleryOwnerReadOnly = _userManager.Users.AsNoTracking().
               Select(a => new GalleryOwnerReadModel()
               {
                   UserNameForComments = a.UserNameForComments,
                   AccessFailedCount = a.AccessFailedCount,
                   Albums = a.Albums,
                   GalleryAccessibility = a.GalleryAccessibility,
                   ProfileImageUploadedAt = a.ProfileImageUploadedAt,
                   ThumbnailAspectRatio = a.ThumbnailAspectRatio,
                   ConcurrencyStamp = a.ConcurrencyStamp,
                   DateCreated = a.DateCreated,
                   Email = a.Email,
                   EmailConfirmed = a.EmailConfirmed,
                   Id = a.Id,
                   LockoutEnabled = a.LockoutEnabled,
                   LockoutEnd = a.LockoutEnd,
                   NormalizedEmail = a.NormalizedEmail,
                   NormalizedUserName = a.NormalizedUserName,
                   PasswordHash = a.PasswordHash,
                   PhoneNumber = a.PhoneNumber,
                   PhoneNumberConfirmed = a.PhoneNumberConfirmed,
                   ProfileImageContentType = a.ProfileImageContentType,
                   ProfileImageHeight = a.ProfileImageHeight,
                   ProfileImageOriginalName = a.ProfileImageOriginalName,
                   ProfileImageSize = a.ProfileImageSize,
                   ProfileImageWidth = a.ProfileImageWidth,
                   SecurityStamp = a.SecurityStamp,
                   ThumbnailContentType = a.ThumbnailContentType,
                   ThumbnailForCommentedCommentBlob = a.ThumbnailForCommentedCommentBlob,
                   ThumbnailForCommentedCommentHeight = a.ThumbnailForCommentedCommentHeight,
                   ThumbnailForCommentedCommentWidth = a.ThumbnailForCommentedCommentWidth,
                   ThumbnailForStandardCommentBlob = a.ThumbnailForStandardCommentBlob,
                   ThumbnailForStandardCommentHeight = a.ThumbnailForStandardCommentHeight,
                   ThumbnailForStandardCommentWidth = a.ThumbnailForStandardCommentWidth,
                   TwoFactorEnabled = a.TwoFactorEnabled,
                   UserName = a.UserName,
                   CurrentGallerySize = _context.Images.AsNoTracking().Where(c => c.Album.GalleryOwnerId == a.Id).Select(d => d.Size).Sum()
               }
               )
               .Where(x => x.Email == galleryOwnerEmail).SingleOrDefault();

            if (GalleryOwnerReadOnly == null)
            {
                return Forbid();

            }
            else if (GalleryOwnerReadOnly.GalleryAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwnerReadOnly.Id &&
            x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
            FirstOrDefault();

            ImagesFromService = _helper.GetImagesForCurrentAlbum(Album.AlbumId, method, orderBy, out string Method, out OrderBy OrderBy, false, true);
            GalleryOrderBy = OrderBy;
            GalleryMethod = Method;


            NumberOfAlbums = _context.Albums.AsNoTracking().Where(s => s.GalleryOwnerId == GalleryOwnerReadOnly.Id && s.AlbumId != Album.AlbumId).ToList().Count;
            NumberOfImages = _context.Images.AsNoTracking().Where(s => s.Album.GalleryOwnerId == GalleryOwnerReadOnly.Id).ToList().Count;
            DefaultAlbumNumberOfImages = _context.Images.AsNoTracking().Where(s => s.AlbumId == Album.AlbumId).ToList().Count;
            DefaultAlbumTotalSize = _context.Images.AsNoTracking().Where(s => s.AlbumId == Album.AlbumId).Select(x => x.Size).Sum();

            GalleryMethodOfSortingList = _dropdownList.GetMethodOfSortingDropdown();
            GalleryOrderByList = _dropdownList.GetOrderByDropdown();
            return Page();
        }
        public IActionResult OnGetDownloadAllUnclassifiedImagesFromReadOnlyGallery(string galleryOwnerEmail)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Email == galleryOwnerEmail).SingleOrDefault();

            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }
            else if (GalleryOwner.GalleryAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
            x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
            FirstOrDefault();

            Images = _context.Images.AsNoTracking().Where(x => x.AlbumId == Album.AlbumId && x.ImageAccessibility == Accessibility.Public).AsEnumerable();
            if (Images.Any() == false)
            {
                ErrorMessage = "There is nothing to download.";
                return RedirectToPage("/ReadOnlyGallery", new { galleryOwnerEmail = galleryOwnerEmail });
            }
            else
            {
                DirectoryPath = Path.Combine(_environment.ContentRootPath, @"Uploads\");

                using (FileStream zip = new FileStream(DirectoryPath + $"{GalleryOwner.Email}'s " + "photos.zip", FileMode.Create))
                {
                    zip.Dispose();
                }

                using (ZipArchive archive = ZipFile.Open(DirectoryPath + $"{GalleryOwner.Email}'s " + "photos.zip", ZipArchiveMode.Update))
                {
                    foreach (var item in Images)
                    {
                        archive.CreateEntryFromFile(DirectoryPath + item.ImageId.ToString(), item.OriginalName, CompressionLevel.NoCompression);
                    }
                }

                byte[] result = System.IO.File.ReadAllBytes(DirectoryPath + $"{GalleryOwner.Email}'s " + "photos.zip");
                System.IO.File.Delete(DirectoryPath + $"{GalleryOwner.Email}'s " + "photos.zip");

                return File(result, "application/zip", $"{GalleryOwner.Email}'s " + "photos.zip");
            }
        }

        public IActionResult OnGetThumbnail(string filename, ThumbnailType type, string galleryOwnerEmail)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Email == galleryOwnerEmail).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }
            else if (GalleryOwner.GalleryAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
           x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
           FirstOrDefault();

            Image = _context.Images.AsNoTracking().Where(p => p.AlbumId == Album.AlbumId && p.ImageId == Guid.Parse(filename)).SingleOrDefault();
            if (Image == null)
            {
                return RedirectToPage("Error");
            }
            else if (Image.ImageAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

            Thumbnail = _context.Thumbnails
           .AsNoTracking()
           .Where(t => t.ImageId == Guid.Parse(filename) && t.Type == type)
           .SingleOrDefault();

            if (Thumbnail == null)
            {
                return RedirectToPage("Error");
            }

            return File(Thumbnail.Blob, "image/" + Thumbnail.ThumbnailContentType.ToLower());
        }

        public IActionResult OnPostGallerySettings(string galleryOwnerEmail)
        {
            return RedirectToPage("/ReadOnlyGallery", new { galleryOwnerEmail = galleryOwnerEmail, method = GalleryMethod, orderBy = GalleryOrderBy });
        }

        public IActionResult OnGetDownloadUnclassifiedImage(Guid? id, string galleryOwnerEmail)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Email == galleryOwnerEmail).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }
            else if (GalleryOwner.GalleryAccessibility == Accessibility.Private)
            {
                return Forbid();
            }
            else if (id == null)
            {
                return RedirectToPage("Error");
            }
            else
            {
                DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", id.ToString());
                if (System.IO.File.Exists(DirectoryPath))
                {
                    Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
       x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
       FirstOrDefault();
                    Image = _context.Images.AsNoTracking().Where(x => x.AlbumId == Album.AlbumId && x.ImageId == id).SingleOrDefault();
                    if (Image == null)
                    {
                        return RedirectToPage("Error");
                    }
                    else if (Image.ImageAccessibility == Accessibility.Private)
                    {
                        return Forbid();
                    }
                    else
                    {
                        return PhysicalFile(DirectoryPath, Image.ImageContentType, Image.OriginalName);
                    }
                }
                else
                {
                    return RedirectToPage("Error");
                }
            }
        }


    }
}
