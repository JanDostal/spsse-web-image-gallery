using GalleryDatabase.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GalleryDatabase.Pages
{
    public class IndexModel : PageModel
    {
        private readonly GalleryDbContext _context;

        private readonly UserManager<GalleryOwner> _userManager;

        private readonly IWebHostEnvironment _environment;

        public IndexModel(UserManager<GalleryOwner> userManager, GalleryDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;

        }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public GalleryOwner GalleryOwner { get; set; }

        public IEnumerable<ImageReadModel> Images { get; set; }

        public IEnumerable<AlbumReadModel> Albums { get; set; }

        public IEnumerable<GalleryOwnerReadModel> GalleryOwners { get; set; }

        public Image Image { get; set; }

        public Album Album { get; set; }

        public Thumbnail Thumbnail { get; set; }

        public string AlbumPath { get; set; }

        public string GalleryPath { get; set; }

        public string DirectoryPath { get; set; }

        public int PositionOfExtension { get; set; }

        public int Counter { get; set; }

        public string ViewId { get; set; }

        public string GetUserId()
        {
            return HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? default;
        }

        public string GetViewId(int? counter)
        {
            if (counter <= 0 || counter == null)
            {
                return "";
            }
            else
            {
                string counterStringRepresentation = Counter.ToString();
                if (counterStringRepresentation.Length > 1)
                {
                    var modifiedString = string.Join("z", counterStringRepresentation.ToArray());
                    return modifiedString;
                }
                else
                {
                    return counterStringRepresentation;
                }
            }
        }

        public IActionResult OnGetMoveToChosenImage(Guid imageId)
        {
            Image = _context.Images.AsNoTracking().Where(p => p.ImageId == imageId).SingleOrDefault();
            if (Image == null)
            {
                return RedirectToPage("Error");
            }
            else if (Image.ImageAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

            return RedirectToPage("/Index", null, null, $"publicImage{imageId}");
        }

        public IActionResult OnGetMoveToChosenAlbum(int albumViewId)
        {
            return RedirectToPage("/Index", null, null, $"publicAlbum{albumViewId}");
        }

        public IActionResult OnGetMoveToChosenGallery(int galleryViewId)
        { 
            return RedirectToPage("/Index", null, null, $"publicGallery{galleryViewId}");
        }


        public IActionResult OnGet()
        {
            Images = _context.Images.AsNoTracking().
                Select(x => new ImageReadModel
                {
                    AlbumId = x.AlbumId,
                    Album = x.Album,
                    DefaultAlbum = _context.Albums.AsNoTracking().Where(s => s.GalleryOwnerId == x.Album.GalleryOwnerId
                    && s.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2Y" +
                                  "OHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").FirstOrDefault(),
                    ImageAccessibility = x.ImageAccessibility,
                    UploadedAt = x.UploadedAt,
                    DateTaken = x.DateTaken,
                    ImageContentType = x.ImageContentType,
                    ImageHeight = x.ImageHeight,
                    ImageWidth = x.ImageWidth,
                    OriginalName = x.OriginalName,
                    Size = x.Size,
                    GalleryOwnerEmail = x.Album.GalleryOwner.Email,
                    GalleryOwnerUserName = x.Album.GalleryOwner.UserNameForComments,
                    AlbumsCount = x.Albums.Count,
                    ImageId = x.ImageId,
                    NumberOfComments = x.Comments.Where(x => x.DateUserWasRemoved == null).ToList().Count
                }).
                Where(x => x.ImageAccessibility == Accessibility.Public).OrderByDescending(x => x.UploadedAt).Take(12).AsEnumerable();
            Albums = _context.Albums.AsNoTracking().
                Select(x => new AlbumReadModel
                {
                    AlbumId = x.AlbumId,
                    AlbumAccessibility = x.AlbumAccessibility,
                    DateCreated = x.DateCreated,
                    Name = x.Name,
                    Cover = x.Cover,
                    CoverImageId = x.CoverImageId,
                    GalleryOwner = x.GalleryOwner,
                    GalleryOwnerId = x.GalleryOwnerId,
                    ImagesSizes = x.Images.AsQueryable().Select(x => x.Size).ToList(),
                    DefaultAlbum = _context.Albums.AsNoTracking().Where(s => s.GalleryOwnerId == x.GalleryOwnerId
                    && s.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2Y" +
                                  "OHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").FirstOrDefault()
                })
                .Where(x => x.AlbumAccessibility == Accessibility.Public).
            OrderByDescending(x => x.DateCreated).Take(12).AsEnumerable();
            GalleryOwners = _userManager.Users.AsNoTracking().
                Select(x => new GalleryOwnerReadModel
                {
                    AccessFailedCount = x.AccessFailedCount,
                    GalleryAccessibility = x.GalleryAccessibility,
                    ConcurrencyStamp = x.ConcurrencyStamp,
                    CurrentGallerySize = _context.Images.AsNoTracking().Where(a => a.Album.GalleryOwnerId == x.Id).Select(c => c.Size).Sum(),
                    Email = x.Email,
                    EmailConfirmed = x.EmailConfirmed,
                    Id = x.Id,
                    LockoutEnabled = x.LockoutEnabled,
                    LockoutEnd = x.LockoutEnd,
                    UserName = x.UserName,
                    TwoFactorEnabled = x.TwoFactorEnabled,
                    NormalizedEmail = x.NormalizedEmail,
                    NormalizedUserName = x.NormalizedUserName,
                    PasswordHash = x.PasswordHash,
                    PhoneNumber = x.PhoneNumber,
                    PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                    SecurityStamp = x.SecurityStamp,
                    NumberOfAlbums = x.Albums.Count - 1,
                    NumberOfImages = _context.Images.AsNoTracking().Where(s => s.Album.GalleryOwnerId == x.Id).ToList().Count,
                    DateCreated = x.DateCreated,
                    UserNameForComments = x.UserNameForComments
                })
                .Where(x => x.GalleryAccessibility == Accessibility.Public).OrderByDescending(x => x.DateCreated).Take(12).AsEnumerable();

            return Page();
        }

        public IActionResult OnGetThumbnail(string filename, ThumbnailType type)
        {
            Image = _context.Images.AsNoTracking().Where(p => p.ImageId == Guid.Parse(filename)).SingleOrDefault();

            if (Image == null)
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.AlbumId == Image.AlbumId).SingleOrDefault();

            if (Image.ImageAccessibility == Accessibility.Private && GetUserId() != Album.GalleryOwnerId)
            {
                return Forbid();
            }

            DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", Image.ImageId.ToString());

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

        public IActionResult OnGetDownloadAllNewestPublicImages()
        {
            var images = _context.Images.AsNoTracking().Where(x => x.ImageAccessibility == Accessibility.Public).OrderByDescending(x => x.UploadedAt).Take(12).AsEnumerable();
            if (images.Any() == false)
            {
                ErrorMessage = "There is nothing to download.";
                return RedirectToPage("/Index");
            }
            else
            {

                DirectoryPath = Path.Combine(_environment.ContentRootPath, @"Uploads\");

                using (FileStream zip = new FileStream(DirectoryPath + "Newest public photos.zip", FileMode.Create))
                {
                    zip.Dispose();
                }

                using (ZipArchive archive = ZipFile.Open(DirectoryPath + "Newest public photos.zip", ZipArchiveMode.Update))
                {
                    foreach (var item in images)
                    {
                        archive.CreateEntryFromFile(DirectoryPath + item.ImageId.ToString(), item.OriginalName, CompressionLevel.NoCompression);
                    }
                }

                byte[] result = System.IO.File.ReadAllBytes(DirectoryPath + "Newest public photos.zip");
                System.IO.File.Delete(DirectoryPath + "Newest public photos.zip");

                return File(result, "application/zip", "Newest public photos.zip");
            }
        }

        public IActionResult OnGetDownloadImage(Guid? id)
        {
            if (id == null)
            {
                return RedirectToPage("Error");
            }

            DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", id.ToString());
            if (System.IO.File.Exists(DirectoryPath))
            {
                Image = _context.Images.AsNoTracking().Where(x => x.ImageId == id).SingleOrDefault();

                if (Image == null)
                {
                    return RedirectToPage("Error");
                }

                Album = _context.Albums.AsNoTracking().Where(x => x.AlbumId == Image.AlbumId).SingleOrDefault();

                if (Image.ImageAccessibility == Accessibility.Private && GetUserId() != Album.GalleryOwnerId)
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
