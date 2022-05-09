using System;
using System.Collections.Generic;
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
    public class ReadOnlyAlbumModel : PageModel
    {
        private readonly UserManager<GalleryOwner> _userManager;
        private readonly DropdownList _dropdownList;
        private readonly GalleryDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly SortingHelper _helper;

        public List<SelectListItem> AlbumOrderByList { get; private set; }

        public List<SelectListItem> AlbumMethodOfSortingList { get; private set; }

        public ReadOnlyAlbumModel(UserManager<GalleryOwner> userManager, GalleryDbContext context, IWebHostEnvironment environment, DropdownList dropdownList, SortingHelper helper)
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

        public Album Album { get; set; }

        public Image Image { get; set; }

        public Thumbnail Thumbnail { get; set; }

        [BindProperty]
        public string AlbumMethod { get; set; }

        [BindProperty]
        public OrderBy AlbumOrderBy { get; set; }

        public long CurrentAlbumSize { get; set; }

        public int NumberOfImages { get; set; }

        public string CoverImageName { get; set; }

        public string DirectoryPath { get; set; }

        public IEnumerable<Image> Images { get; set; }

        public IEnumerable<ImageReadModel> ImagesFromService { get; set; }

        public int Counter { get; set; }

        public IActionResult OnGetMoveToChosenImage(Guid imageId, int albumId, string albumMethod, OrderBy albumOrderBy)
        {
            Album = _context.Albums.AsNoTracking().Where(x => x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return RedirectToPage("Error");
            }
            else if (Album.AlbumAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

            Image = _context.Images.AsNoTracking().Where(p => p.AlbumId == Album.AlbumId && p.ImageId == imageId).SingleOrDefault();
            if (Image == null)
            {
                return RedirectToPage("Error");
            }
            else if (Image.ImageAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

            return RedirectToPage("/ReadOnlyAlbum", null, new { albumId = albumId, method = albumMethod, orderBy = albumOrderBy }, $"publicAlbumImage{imageId}");
        }


        public async Task<IActionResult> OnGet(int albumId, string method, OrderBy orderBy)
        {
            Album = _context.Albums.AsNoTracking().Where(x => x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return NotFound("Album not found");
            }
            else if (Album.AlbumAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

            var defaultAlbum = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == Album.GalleryOwnerId && x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2Y" +
                                   "OHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").FirstOrDefault();

            if (Album.AlbumId == defaultAlbum.AlbumId)
            {
                return RedirectToPage("Error");
            }

            ImagesFromService = _helper.GetImagesForCurrentAlbum(Album.AlbumId, method, orderBy, out string Method, out OrderBy OrderBy, false, true);
            AlbumMethod = Method;
            AlbumOrderBy = OrderBy;

            GalleryOwner = await _userManager.Users.AsNoTracking().Where(x => x.Id == Album.GalleryOwnerId).SingleOrDefaultAsync();
            CurrentAlbumSize = _context.Images.AsNoTracking().Where(x => x.AlbumId == Album.AlbumId).Sum(x => x.Size);
            NumberOfImages = _context.Images.AsNoTracking().Where(s => s.AlbumId == Album.AlbumId).ToList().Count;

            Image = _context.Images.AsNoTracking().Where(x => x.ImageId == Album.CoverImageId).SingleOrDefault();

            if (Image == null)
            {
                CoverImageName = "None";
            }
            else if (Image.ImageAccessibility == Accessibility.Private)
            {
                CoverImageName = "Restricted";
            }
            else
            {
                if (Image.OriginalName.Length > 23)
                {
                    var positionOfExtension = Image.OriginalName.LastIndexOf(".");
                    CoverImageName = Image.OriginalName.Substring(0, 15) + "..." + Image.OriginalName.Substring(positionOfExtension - 1);
                }
                else
                {
                    CoverImageName = Image.OriginalName;
                }
            }

            AlbumMethodOfSortingList = _dropdownList.GetMethodOfSortingDropdown();
            AlbumOrderByList = _dropdownList.GetOrderByDropdown();

            return Page();
        }
        public IActionResult OnGetDownloadAllImagesFromReadOnlyAlbum(int albumId)
        {
            Album = _context.Albums.AsNoTracking().Where(x => x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return RedirectToPage("Error");
            }
            else if (Album.AlbumAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

            Images = _context.Images.AsNoTracking().Where(x => x.AlbumId == Album.AlbumId && x.ImageAccessibility == Accessibility.Public).AsEnumerable();
            if (Images.Any() == false)
            {
                ErrorMessage = "There is nothing to download.";
                return RedirectToPage("/ReadOnlyAlbum", new { albumId = albumId });
            }
            else
            {
                DirectoryPath = Path.Combine(_environment.ContentRootPath, @"Uploads\");

                using (FileStream zip = new FileStream(DirectoryPath + Album.Name + ".zip", FileMode.Create))
                {
                    zip.Dispose();
                }

                using (ZipArchive archive = ZipFile.Open(DirectoryPath + Album.Name + ".zip", ZipArchiveMode.Update))
                {
                    foreach (var item in Images)
                    {
                        archive.CreateEntryFromFile(DirectoryPath + item.ImageId.ToString(), item.OriginalName, CompressionLevel.NoCompression);
                    }
                }

                byte[] result = System.IO.File.ReadAllBytes(DirectoryPath + Album.Name + ".zip");
                System.IO.File.Delete(DirectoryPath + Album.Name + ".zip");

                return File(result, "application/zip", Album.Name + ".zip");
            }
        }

        public IActionResult OnGetThumbnail(string filename, ThumbnailType type, int albumId)
        {

            Album = _context.Albums.AsNoTracking().Where(x => x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return RedirectToPage("Error");
            }
            else if (Album.AlbumAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

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

        public IActionResult OnPostChosenAlbumSorting(int albumId)
        {
            return RedirectToPage("/ReadOnlyAlbum", new { albumId = albumId, method = AlbumMethod, orderBy = AlbumOrderBy });
        }

        public IActionResult OnGetDownloadImage(Guid? id, int albumId)
        {
            Album = _context.Albums.AsNoTracking().Where(x => x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return RedirectToPage("Error");
            }
            else if (Album.AlbumAccessibility == Accessibility.Private)
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
