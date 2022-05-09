using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using GalleryDatabase.Models;
using GalleryDatabase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GalleryDatabase.Pages
{
    [Authorize]
    public class MyAlbumsModel : PageModel
    {
        private UserManager<GalleryOwner> _userManager;
        private readonly DropdownList _dropdownList;
        private GalleryDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly SortingHelper _helper;

        public List<SelectListItem> AccessibilityList { get; private set; }

        public List<SelectListItem> AlbumCoverImage { get; private set; }

        public List<SelectListItem> AlbumsMethodOfSortingList { get; private set; }

        public List<SelectListItem> AlbumsOrderByList { get; private set; }

        public MyAlbumsModel(UserManager<GalleryOwner> userManager, GalleryDbContext context, IWebHostEnvironment environment, DropdownList dropdownList, SortingHelper helper)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
            _dropdownList = dropdownList;
            _helper = helper;
        }

        [BindProperty]
        public Album Album { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        [TempData]
        public string SaveChangesSuccessMessage { get; set; }

        [BindProperty]
        public string AlbumsMethod { get; set; }
        [BindProperty]
        public OrderBy AlbumsOrderBy { get; set; }

        public GalleryOwner GalleryOwner { get; set; }

        public IEnumerable<Album> Albums { get; set; }

        public IEnumerable<AlbumReadModel> AlbumsFromService { get; set; }

        public IEnumerable<Image> Images { get; set; }

        public IEnumerable<Thumbnail> Thumbnails { get; set; }

        public Image Image { get; set; }

        public string DirectoryPath { get; set; }

        public int Counter { get; set; }

        [TempData]
        public int ChosenAlbumSavedSettingsIndex { get; set; }

        public string GetUserId()
        {
            return HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? default;
        }

        public IActionResult OnGetMoveToChosenAlbum(int albumId, string albumsMethod, OrderBy albumsOrderBy)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.AsNoTracking().Where(p => p.AlbumId == albumId && p.GalleryOwnerId == GalleryOwner.Id).SingleOrDefault();
            if (Album == null)
            {
                return RedirectToPage("Error");
            }


            return RedirectToPage("/MyAlbums", null, new { method = albumsMethod, orderBy = albumsOrderBy }, $"ownerAlbum{albumId}");
        }


        public IActionResult OnGet(string method, OrderBy orderBy)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return NotFound("Gallery owner not found.");
            }

            AlbumsFromService = _helper.GetAlbumsForCurrentGallery(GalleryOwner.Id, method, orderBy, out string Method, out OrderBy OrderBy);
            AlbumsMethod = Method;
            AlbumsOrderBy = OrderBy;

            AccessibilityList = _dropdownList.GetAccessibilityDropdown();
            AlbumCoverImage = _dropdownList.GetImageDropdown(GetUserId());
            AlbumsMethodOfSortingList = _dropdownList.GetMethodOfSortingDropdownForAlbums();
            AlbumsOrderByList = _dropdownList.GetOrderByDropdown();
            return Page();
        }


        public IActionResult OnGetThumbnail(string filename, ThumbnailType type)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            Image = _context.Images.AsNoTracking().Where(p => p.Album.GalleryOwnerId == GalleryOwner.Id && p.ImageId == Guid.Parse(filename)).SingleOrDefault();
            if (Image == null)
            {
                return RedirectToPage("Error");
            }

            Thumbnail thumbnail = _context.Thumbnails
                .AsNoTracking()
                .Where(t => t.ImageId == Guid.Parse(filename) && t.Type == type)
                .SingleOrDefault();
            if (thumbnail == null)
            {
                return RedirectToPage("Error");
            }

            return File(thumbnail.Blob, "image/" + thumbnail.ThumbnailContentType.ToLower());
        }

        public async Task<IActionResult> OnPostCreateAlbum(string albumName)
        {
            GalleryOwner = await _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefaultAsync();

            if (albumName == null)
            {
                ErrorMessage = "Album name cannot be empty.";
            }
            else if (albumName.Contains('<') || albumName.Contains('>') || albumName.Contains(':') || albumName.Contains('"') || albumName.Contains('/') ||
                albumName.Contains(@"\") || albumName.Contains('|') || albumName.Contains('?') || albumName.Contains('*'))
            {
                ErrorMessage = "Invalid characters were used when naming album.";

            }
            else
            {

                Album.GalleryOwnerId = GalleryOwner.Id;
                Album.DateCreated = DateTime.Now;
                Album.Name = albumName;
                _context.Albums.Add(Album);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/MyAlbums");
        }

        public IActionResult OnGetDownloadAllImagesFromAlbums()
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
            x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa")
                .FirstOrDefault();
            Images = _context.Images.AsNoTracking().Where(x => x.Album.GalleryOwnerId == GalleryOwner.Id && x.AlbumId != Album.AlbumId).AsEnumerable();
            if (Images.Any() == false)
            {
                ErrorMessage = "There is nothing to download.";
                return RedirectToPage("/MyAlbums");
            }
            else
            {
                DirectoryPath = Path.Combine(_environment.ContentRootPath, @"Uploads\");

                using (FileStream zip = new FileStream(DirectoryPath + "Albums.zip", FileMode.Create))
                {
                    zip.Dispose();
                }

                using (ZipArchive archive = ZipFile.Open(DirectoryPath + "Albums.zip", ZipArchiveMode.Update))
                {
                    foreach (var item in Images)
                    {
                        Album = _context.Albums.AsNoTracking().Where(x => x.AlbumId == item.AlbumId).SingleOrDefault();
                        archive.CreateEntryFromFile(DirectoryPath + item.ImageId.ToString(), Album.Name + @"\" + item.OriginalName, CompressionLevel.NoCompression);
                    }

                }

                byte[] result = System.IO.File.ReadAllBytes(DirectoryPath + "Albums.zip");
                System.IO.File.Delete(DirectoryPath + "Albums.zip");


                return File(result, "application/zip", "Albums.zip");
            }
        }

        public IActionResult OnPostAlbumsSettings()
        {
            return RedirectToPage("/MyAlbums", new { method = AlbumsMethod, orderBy = AlbumsOrderBy });
        }

        public async Task<IActionResult> OnGetDeleteAllAlbums()
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
            x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa")
                .FirstOrDefault();

            Albums = _context.Albums.Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId != Album.AlbumId).AsEnumerable();
            if (Albums.Any() == false)
            {
                ErrorMessage = "There is nothing to delete.";
            }
            else
            {
                Images = _context.Images.
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

                    }).
                    Where(x => x.Album.GalleryOwnerId == GalleryOwner.Id && x.AlbumId != Album.AlbumId).AsEnumerable();

                Thumbnails = _context.Thumbnails.Where(x => x.Image.Album.GalleryOwnerId == GalleryOwner.Id && x.Image.AlbumId != Album.AlbumId).AsEnumerable();



                _context.Thumbnails.RemoveRange(Thumbnails.AsQueryable());


                foreach (var item in Images)
                {
                    var commentUsers = _context.CommentUsers.Where(x => x.Comment.ImageId == item.ImageId);

                    _context.CommentUsers.RemoveRange(commentUsers);

                    _context.Comments.RemoveRange(item.Comments.AsQueryable());

                    DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", item.ImageId.ToString());
                    _context.Images.Remove(item);
                    System.IO.File.Delete(DirectoryPath);
                }

                await _context.SaveChangesAsync();


                _context.Albums.RemoveRange(Albums.AsQueryable());


                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/MyAlbums");
        }
        public async Task<IActionResult> OnGetDeleteAlbum(int albumId, string albumsMethod, OrderBy albumsOrderBy)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.Where(x => x.AlbumId == albumId).SingleOrDefault();

            if (Album == null)
            {
                return RedirectToPage("Error");
            }

            Thumbnails = _context.Thumbnails.Where(x => x.Image.AlbumId == albumId).AsEnumerable();

            _context.Thumbnails.RemoveRange(Thumbnails.AsQueryable());

            Images = _context.Images.
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
                     ImageId = x.ImageId
                 }).
                Where(x => x.AlbumId == albumId).AsEnumerable();
           
            foreach (var item in Images)
            {
                var imageCommentUsers = _context.CommentUsers.Where(x => x.Comment.ImageId == item.ImageId);

                _context.CommentUsers.RemoveRange(imageCommentUsers);

                _context.Comments.RemoveRange(item.Comments.AsQueryable());

                DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", item.ImageId.ToString());
                _context.Images.Remove(item);
                System.IO.File.Delete(DirectoryPath);
            }

            await _context.SaveChangesAsync();

            _context.Albums.Remove(Album);
            await _context.SaveChangesAsync();

            SuccessMessage = "Chosen album has been successfully deleted.";
            return RedirectToPage("/MyAlbums", new { method = albumsMethod, orderBy = albumsOrderBy });
        }
        public async Task<IActionResult> OnPostAlbumSettings(int albumId, int chosenAlbumSavedSettingsIndex, string coverImageId, Accessibility? accessibility, 
            string albumsMethod, OrderBy albumsOrderBy)
        {

            GalleryOwner = await _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefaultAsync();

            if (coverImageId == null)
            {
                return RedirectToPage("Error");
            }
            else if (accessibility == null || (accessibility != Accessibility.Private && accessibility != Accessibility.Public))
            {
                return RedirectToPage("Error");

            }


            Album = _context.Albums.Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return RedirectToPage("Error");
            }

            Images = _context.Images.Where(x => x.AlbumId == Album.AlbumId).AsEnumerable();
            string name = Request.Form["albumName"];

            if (name.Any() == false)
            {
                SaveChangesSuccessMessage = "Album name cannot be empty.";
            }
            else if (name.Contains('<') || name.Contains('>') || name.Contains(':') || name.Contains('"') || name.Contains('/') ||
                name.Contains(@"\") || name.Contains('|') || name.Contains('?') || name.Contains('*'))
            {
                SaveChangesSuccessMessage = "Invalid characters were used when naming album.";
            }
            else
            {
                if (accessibility == Accessibility.Private)
                {
                    Album.AlbumAccessibility = Accessibility.Private;
                }
                else
                {
                    Album.AlbumAccessibility = Accessibility.Public;
                }

                Album.Name = name;

                if (coverImageId == "None")
                {
                    Album.CoverImageId = null;
                }
                else
                {
                    Album.CoverImageId = Guid.Parse(coverImageId);
                }

                _context.Albums.Update(Album);

                foreach (var item in Images)
                {
                    item.ImageAccessibility = Album.AlbumAccessibility;
                    _context.Images.Update(item);
                }
                await _context.SaveChangesAsync();
                SaveChangesSuccessMessage = "Your album has been updated.";

            }

            ChosenAlbumSavedSettingsIndex = chosenAlbumSavedSettingsIndex;
            return RedirectToPage("/MyAlbums", null, new { method = albumsMethod, orderBy = albumsOrderBy }, $"albumSettings{albumId}");
        }
    }
}
