using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
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
    public class ReadOnlyAlbumsModel : PageModel
    {
        private readonly UserManager<GalleryOwner> _userManager;
        private readonly DropdownList _dropdownList;
        private readonly GalleryDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly SortingHelper _helper;

        public List<SelectListItem> AlbumsMethodOfSortingList { get; private set; }

        public List<SelectListItem> AlbumsOrderByList { get; private set; }

        public ReadOnlyAlbumsModel(UserManager<GalleryOwner> userManager, GalleryDbContext context, IWebHostEnvironment environment, DropdownList dropdownList, SortingHelper helper)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
            _dropdownList = dropdownList;
            _helper = helper;
        }

        public Album Album { get; set; }

        public Image AlbumCoverImage { get; set; }

        public Image Image { get; set; }

        public GalleryOwner GalleryOwner { get; set; }

        public Thumbnail Thumbnail { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        [BindProperty]
        public string AlbumsMethod { get; set; }
        [BindProperty]
        public OrderBy AlbumsOrderBy { get; set; }

        public IEnumerable<Album> Albums { get; set; }

        public IEnumerable<AlbumReadModel> AlbumsFromService { get; set; }

        public IEnumerable<Image> Images { get; set; }

        public string DirectoryPath { get; set; }

        public int PositionOfExtension { get; set; }

        public int Counter { get; set; }

        public string AlbumViewId {get; set;}

        public string GetAlbumViewId (int? counter)
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

        public IActionResult OnGetMoveToChosenAlbum(int albumViewId, string galleryOwnerEmail, string albumsMethod, OrderBy albumsOrderBy)
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

            return RedirectToPage("/ReadOnlyAlbums", null, new { galleryOwnerEmail = galleryOwnerEmail, method = albumsMethod, orderBy = albumsOrderBy}, $"publicAlbum{albumViewId}");
        }

        public IActionResult OnGet(string method, OrderBy orderBy, string galleryOwnerEmail)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Email == galleryOwnerEmail).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return NotFound("Gallery owner not found");
            }
            else if (GalleryOwner.GalleryAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

            AlbumsFromService = _helper.GetAlbumsForCurrentGallery(GalleryOwner.Id, method, orderBy, out string Method, out OrderBy OrderBy, true);
            AlbumsMethod = Method;
            AlbumsOrderBy = OrderBy;

            AlbumsMethodOfSortingList = _dropdownList.GetMethodOfSortingDropdownForAlbums();
            AlbumsOrderByList = _dropdownList.GetOrderByDropdown();

            return Page();
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

            Image = _context.Images.AsNoTracking().Where(p => p.Album.GalleryOwnerId == GalleryOwner.Id && p.ImageId == Guid.Parse(filename)).SingleOrDefault();
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

        public IActionResult OnGetDownloadAllImagesFromReadOnlyAlbums(string galleryOwnerEmail)
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
            x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa")
                .FirstOrDefault();
            Images = _context.Images.AsNoTracking().Where(x => x.Album.GalleryOwnerId == GalleryOwner.Id && x.AlbumId != Album.AlbumId && x.ImageAccessibility == Accessibility.Public)
                .AsEnumerable();
            if (Images.Any() == false)
            {
                ErrorMessage = "There is nothing to download.";
                return RedirectToPage("/ReadOnlyAlbums", new { galleryOwnerEmail = galleryOwnerEmail });
            }
            else
            {
                DirectoryPath = Path.Combine(_environment.ContentRootPath, @"Uploads\");

                using (FileStream zip = new FileStream(DirectoryPath + $"{GalleryOwner.Email}'s albums.zip", FileMode.Create))
                {
                    zip.Dispose();
                }

                using (ZipArchive archive = ZipFile.Open(DirectoryPath + $"{GalleryOwner.Email}'s albums.zip", ZipArchiveMode.Update))
                {
                    foreach (var item in Images)
                    {
                        Album = _context.Albums.AsNoTracking().Where(x => x.AlbumId == item.AlbumId).SingleOrDefault();
                        if (Album.AlbumAccessibility == Accessibility.Private)
                        {
                            archive.CreateEntryFromFile(DirectoryPath + item.ImageId.ToString(), "Images from private albums" + @"\" + item.OriginalName, CompressionLevel.NoCompression);
                        }
                        else
                        {
                            archive.CreateEntryFromFile(DirectoryPath + item.ImageId.ToString(), Album.Name + @"\" + item.OriginalName, CompressionLevel.NoCompression);

                        }
                    }

                }

                byte[] result = System.IO.File.ReadAllBytes(DirectoryPath + $"{GalleryOwner.Email}'s albums.zip");
                System.IO.File.Delete(DirectoryPath + $"{GalleryOwner.Email}'s albums.zip");

                return File(result, "application/zip", $"{GalleryOwner.Email}'s albums.zip");
            }
        }

        public IActionResult OnPostAlbumsSettings(string galleryOwnerEmail)
        {
            return RedirectToPage("/ReadOnlyAlbums", new { method = AlbumsMethod, orderBy = AlbumsOrderBy, galleryOwnerEmail = galleryOwnerEmail });
        }

    }
}
