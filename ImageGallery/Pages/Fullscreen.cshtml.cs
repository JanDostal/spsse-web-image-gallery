using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GalleryDatabase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Identity;
using GalleryDatabase.Services;

namespace GalleryDatabase.Pages
{
    public class FullscreenModel : PageModel
    {
        private readonly GalleryDbContext _context;

        private readonly IWebHostEnvironment _environment;

        private readonly SortingHelper _helper;

        private readonly UserManager<GalleryOwner> _userManager;


        public FullscreenModel(GalleryDbContext context, IWebHostEnvironment environment, UserManager<GalleryOwner> userManager,
            SortingHelper helper)
        {
            _context = context;
            _environment = environment;
            _helper = helper;
            _userManager = userManager;
        }

        public Image Image { get; set; }

        public Album Album { get; set; }

        public IEnumerable<ImageReadModel> Images { get; set; }

        public string DirectoryPath { get; set; }

        public int Counter { get; set; }

        public int PositionOfExtension { get; set; }

        public string ImageName { get; set; }

        public GalleryOwner GalleryOwner { get; set; }

        public string GetUserId()
        {
            return HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? default;
        }

        public IActionResult OnGet(Guid filename, string method, OrderBy orderBy, string category)
        {

            Image = _context.Images.AsNoTracking().Where(x => x.ImageId == filename).SingleOrDefault();
            if (Image == null)
            {
                return NotFound("Image not found.");

            }

            Album = _context.Albums.AsNoTracking().Where(x => x.AlbumId == Image.AlbumId).SingleOrDefault();

            if (GetUserId() == Album.GalleryOwnerId)
            {
                if (category == "allAlbumImages")
                {
                    Images = _helper.GetImagesForCurrentAlbum(Album.AlbumId, method, orderBy, out string Method, out OrderBy OrderBy);
                    return Page();
                }
            }
            else
            {
                if (Image.ImageAccessibility == Accessibility.Private)
                {
                    return Forbid();
                }

                if (category == "publicAlbumImages")
                {
                    Images = _helper.GetImagesForCurrentAlbum(Album.AlbumId, method, orderBy, out string Method, out OrderBy OrderBy, true);
                    return Page();
                }
            }

            switch (category)
            {
                case "homepageImages":
                    Images = _context.Images.AsNoTracking().
                         Select(x => new ImageReadModel
                         {
                             AlbumId = x.AlbumId,
                             Album = x.Album,
                             ImageAccessibility = x.ImageAccessibility,
                             UploadedAt = x.UploadedAt,
                             DateTaken = x.DateTaken,
                             ImageContentType = x.ImageContentType,
                             ImageHeight = x.ImageHeight,
                             ImageWidth = x.ImageWidth,
                             OriginalName = x.OriginalName,
                             Size = x.Size,
                             AlbumsCount = x.Albums.Count,
                             ImageId = x.ImageId,
                         })
                        .Where(a => a.ImageAccessibility == Accessibility.Public).OrderByDescending(c => c.UploadedAt).Take(12).AsEnumerable();
                    break;
                case "singleImage":
                    Images = _context.Images.AsNoTracking().
                         Select(x => new ImageReadModel
                         {
                             AlbumId = x.AlbumId,
                             Album = x.Album,
                             ImageAccessibility = x.ImageAccessibility,
                             UploadedAt = x.UploadedAt,
                             DateTaken = x.DateTaken,
                             ImageContentType = x.ImageContentType,
                             ImageHeight = x.ImageHeight,
                             ImageWidth = x.ImageWidth,
                             OriginalName = x.OriginalName,
                             Size = x.Size,
                             AlbumsCount = x.Albums.Count,
                             ImageId = x.ImageId,
                         })
                        .Where(a => a.ImageId == Image.ImageId).AsEnumerable();
                    break;

                default:
                    return BadRequest("Category not selected.");
            }


            return Page();
        }

        public IActionResult OnGetFullscreen(Guid filename)
        {
            Image = _context.Images.AsNoTracking().Where(p => p.ImageId == filename).SingleOrDefault();

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

            return PhysicalFile(DirectoryPath, Image.ImageContentType);
        }
    }
}
