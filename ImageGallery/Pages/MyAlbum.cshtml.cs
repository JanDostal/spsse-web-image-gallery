using System;
using System.Collections.Generic;
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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;

namespace GalleryDatabase.Pages
{
    [Authorize]
    public class MyAlbumModel : PageModel
    {

        private UserManager<GalleryOwner> _userManager;
        private readonly DropdownList _dropdownList;
        private GalleryDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly SortingHelper _helper;

        public List<SelectListItem> AccessibilityList { get; private set; }

        public List<SelectListItem> AlbumMethodOfSortingList { get; private set; }

        public List<SelectListItem> AlbumOrderByList { get; private set; }

        public List<SelectListItem> AlbumList { get; private set; }

        public List<SelectListItem> AlbumListForCoverImage { get; private set; }

        public List<SelectListItem> ImageList { get; private set; }

        public MyAlbumModel(UserManager<GalleryOwner> userManager, GalleryDbContext context, IWebHostEnvironment environment, DropdownList dropdownList, SortingHelper helper)
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

        [TempData]
        public string SaveChangesSuccessMessage { get; set; }

        public GalleryOwner GalleryOwner { get; set; }

        [BindProperty]
        public Album Album { get; set; }

        [BindProperty]
        public Models.Image Image { get; set; }


        [BindProperty]
        public string AlbumMethod { get; set; }

        [BindProperty]
        public OrderBy AlbumOrderBy { get; set; }

        public long CurrentAlbumSize { get; set; }

        public int NumberOfImages { get; set; }

        public string CoverImageName { get; set; }

        public string DirectoryPath { get; set; }

        public IEnumerable<Models.Image> Images { get; set; }

        public IEnumerable<ImageReadModel> ImagesFromService { get; set; }

        public IEnumerable<Album> Albums { get; set; }

        public IEnumerable<Thumbnail> Thumbnails { get; set; }

        public Thumbnail Thumbnail { get; set; }

        public int Counter { get; set; }

        [TempData]
        public int ChosenImageSavedSettingsIndex { get; set; }

        public ICollection<IFormFile> Upload { get; set; }

        public string GetUserId()
        {
            return HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? default;
        }

        public IActionResult OnGetMoveToChosenImage(Guid imageId, int albumId, string albumMethod, OrderBy albumOrderBy)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }


            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
           x.AlbumId == albumId).SingleOrDefault();

            if (Album == null)
            {
                return RedirectToPage("Error");

            }

            Image = _context.Images.AsNoTracking().Where(p => p.AlbumId == Album.AlbumId && p.ImageId == imageId).SingleOrDefault();
            if (Image == null)
            {
                return RedirectToPage("Error");
            }


            return RedirectToPage("/MyAlbum", null, new { albumId = albumId, method = albumMethod, orderBy = albumOrderBy }, $"ownerImage{imageId}");
        }



        public IActionResult OnGet(int albumId, string method, OrderBy orderBy)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return NotFound("Gallery owner not found.");
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return NotFound("Album not found.");
            }

            var defaultAlbum = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == Album.GalleryOwnerId && x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2Y" +
                                   "OHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").FirstOrDefault();

            if (Album.AlbumId == defaultAlbum.AlbumId)
            {
                return RedirectToPage("Error");
            }

            ImagesFromService = _helper.GetImagesForCurrentAlbum(Album.AlbumId, method, orderBy, out string Method, out OrderBy OrderBy);
            AlbumMethod = Method;
            AlbumOrderBy = OrderBy;

            CurrentAlbumSize = _context.Images.AsNoTracking().Where(x => x.AlbumId == Album.AlbumId).Sum(x => x.Size);
            NumberOfImages = _context.Images.AsNoTracking().Where(s => s.AlbumId == Album.AlbumId).ToList().Count;
            CoverImageName = _context.Images.AsNoTracking().Where(x => x.ImageId == Album.CoverImageId).Select(x => x.OriginalName).SingleOrDefault();
            if (CoverImageName == null)
            {
                CoverImageName = "None";
            }
            else if (CoverImageName.Length > 23)
            {
                var positionOfExtension = CoverImageName.LastIndexOf(".");
                CoverImageName = CoverImageName.Substring(0, 15) + "..." + CoverImageName.Substring(positionOfExtension - 1);
            }

            AccessibilityList = _dropdownList.GetAccessibilityDropdown();
            AlbumMethodOfSortingList = _dropdownList.GetMethodOfSortingDropdown();
            AlbumOrderByList = _dropdownList.GetOrderByDropdown();
            AlbumList = _dropdownList.GetAlbumDropdown(Album.AlbumId, false);
            AlbumListForCoverImage = _dropdownList.GetAlbumDropdown(Album.AlbumId, true);
            ImageList = _dropdownList.GetImageDropdown(GalleryOwner.Id, Album.AlbumId);

            return Page();
        }
        public IActionResult OnGetDownloadAllImagesFromAlbum(int albumId)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();

            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return RedirectToPage("Error");
            }

            Images = _context.Images.AsNoTracking().Where(x => x.AlbumId == Album.AlbumId).AsEnumerable();
            if (Images.Any() == false)
            {
                ErrorMessage = "There is nothing to download.";
                return RedirectToPage("/MyAlbum", new { albumId = albumId });
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
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return RedirectToPage("Error");
            }

            Image = _context.Images.AsNoTracking().Where(p => p.AlbumId == Album.AlbumId && p.ImageId == Guid.Parse(filename)).SingleOrDefault();
            if (Image == null)
            {
                return RedirectToPage("Error");
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
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return NotFound("Album not found.");
            }

            return RedirectToPage("/MyAlbum", new { albumId = albumId, method = AlbumMethod, orderBy = AlbumOrderBy });
        }


        public async Task<IActionResult> OnPostImageDetailsSettings(Guid id, string chosenAlbumId, Accessibility? accessibility, string[] listOfAlbums, string albumMethod, OrderBy albumOrderBy,
            int albumId, int chosenImageSavedSettingsIndex)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return RedirectToPage("Error");
            }

            if (accessibility == null || (accessibility != Accessibility.Private && accessibility != Accessibility.Public))
            {
                return RedirectToPage("Error");
            }
            else if (listOfAlbums == null)
            {
                return RedirectToPage("Error");
            }


            var unclassifiedAlbum = await _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
         x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
         FirstOrDefaultAsync();

            Image = _context.Images.Where(x => x.AlbumId == Album.AlbumId && x.ImageId == id).SingleOrDefault();
            if (Image == null)
            {
                return RedirectToPage("Error");
            }

            Albums = _context.Albums.Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId != unclassifiedAlbum.AlbumId).AsEnumerable();

            if (accessibility == Accessibility.Private)
            {
                Image.ImageAccessibility = Accessibility.Private;
            }
            else
            {
                Image.ImageAccessibility = Accessibility.Public;
            }

            if (listOfAlbums.Length == 1 && listOfAlbums.Contains("None") == true)
            {
                foreach (var item in Albums)
                {
                    if (item.CoverImageId == Image.ImageId)
                    {
                        item.CoverImageId = null;
                        _context.Albums.Update(item);
                    }
                }
            }
            else if (listOfAlbums.Any() == true && listOfAlbums.Contains("None") == false)
            {
                foreach (var item in Albums)
                {
                    if (item.CoverImageId == Image.ImageId)
                    {
                        item.CoverImageId = null;
                        _context.Albums.Update(item);
                    }
                }
                foreach (var item in listOfAlbums)
                {
                    var album = _context.Albums.Where(x => x.AlbumId == int.Parse(item)).SingleOrDefault();
                    album.CoverImageId = Image.ImageId;
                    _context.Albums.Update(album);
                }
            }

            if (chosenAlbumId != Album.AlbumId.ToString())
            {
                Image.AlbumId = int.Parse(chosenAlbumId);
                _context.Images.Update(Image);
                await _context.SaveChangesAsync();
                return RedirectToPage("/MyAlbum", new { albumId = albumId });
            }

            _context.Images.Update(Image);
            await _context.SaveChangesAsync();

            SaveChangesSuccessMessage = "Your image settings has been updated.";
            ChosenImageSavedSettingsIndex = chosenImageSavedSettingsIndex;
            return RedirectToPage("/MyAlbum", null, new { albumId = albumId, method = albumMethod, orderBy = albumOrderBy }, $"chosenAlbumSettings{id}");
        }

        public async Task<IActionResult> OnPostAlbumUpload(int albumId)
        {
            GalleryOwner = _userManager.Users.Where(x => x.Id == GetUserId()).SingleOrDefault();

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return RedirectToPage("Error");
            }

            int successfulProcessing = 0;
            int failedProcessing = 0;

            foreach (var uploadedFile in Upload)
            {
                var image = new Models.Image
                {
                    Size = uploadedFile.Length,
                    ImageContentType = uploadedFile.ContentType,
                    OriginalName = uploadedFile.FileName,
                    AlbumId = Album.AlbumId,
                    ImageAccessibility = Image.ImageAccessibility,
                    UploadedAt = DateTime.Now
                };

                _context.Images.Add(image);

                try
                {
                    if (image.ImageContentType.Contains("xml") == true || image.ImageContentType.StartsWith("image") == false || image.Size == 0)
                    {
                        _context.Images.Remove(image);
                        await _context.SaveChangesAsync();
                        throw new Exception();
                    }

                    FileStream fileStream;
                    MemoryStream ims = new MemoryStream();
                    MemoryStream square = new MemoryStream();
                    MemoryStream preservedAspectRatio = new MemoryStream();
                    image.Thumbnails = new List<Thumbnail>();
                    DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", image.ImageId.ToString());

                    uploadedFile.CopyTo(ims);
                    using (fileStream = new FileStream(DirectoryPath, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(fileStream);
                    };

                    SixLabors.ImageSharp.Image img = SixLabors.ImageSharp.Image.Load(DirectoryPath, out IImageFormat format);
                    SixLabors.ImageSharp.Image pic;
                    var result = img.Metadata;
                    DateTime imageDatetime;

                    if (img.Height > 1080 || img.Width > 1920)
                    {
                        using (pic = SixLabors.ImageSharp.Image.Load(ims.ToArray()))
                        {
                            if (img.Width < img.Height)
                            {
                                pic.Mutate(x => x.Resize(0, 1080));
                            }
                            else if (img.Width == img.Height)
                            {
                                pic.Mutate(x => x.Resize(1080, 0));
                            }
                            else
                            {
                                pic.Mutate(x => x.Resize(1920, 0));
                            }

                            System.IO.File.Delete(DirectoryPath);
                            using (fileStream = new FileStream(DirectoryPath, FileMode.Create))
                            {
                                pic.Save(fileStream, format);
                                image.Size = fileStream.Length;
                            }
                            using (ims = new MemoryStream())
                            {
                                pic.Save(ims, format);
                            }
                            image.ImageHeight = pic.Height;
                            image.ImageWidth = pic.Width;
                        }
                    }
                    else
                    {
                        image.ImageHeight = img.Height;
                        image.ImageWidth = img.Width;
                    }

                    if (result.ExifProfile == null ||
                        result.ExifProfile.GetValue(ExifTag.DateTimeOriginal) == null ||
                        result.ExifProfile.GetValue(ExifTag.DateTimeOriginal).Value == null ||
                        DateTime.TryParseExact(result.ExifProfile.GetValue(ExifTag.DateTimeOriginal).Value, "yyyy:MM:dd HH:mm:ss",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out imageDatetime) == false)
                    {
                        image.DateTaken = image.UploadedAt;
                    }
                    else
                    {
                        image.DateTaken = DateTime.ParseExact(result.ExifProfile.GetValue(ExifTag.DateTimeOriginal).Value, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                    }

                    await _userManager.UpdateAsync(GalleryOwner);


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
                        pic.SaveAsJpeg(square, new JpegEncoder { Quality = 70 });
                        SixLabors.ImageSharp.Image.Load(square.ToArray(), out format);
                        image.Thumbnails.Add(new Thumbnail
                        {
                            AspectRatio = (double)pic.Width / pic.Height,
                            Type = ThumbnailType.Square,
                            ThumbnailContentType = format.Name,
                            ThumbnailHeight = pic.Height,
                            ThumbnailWidth = pic.Width,
                            Blob = square.ToArray()
                        });
                    }
                    using (pic = SixLabors.ImageSharp.Image.Load(ims.ToArray()))
                    {
                        if (pic.Width > 800 || pic.Height > 800)
                        {
                            if (pic.Width < pic.Height)
                            {
                                pic.Mutate(x => x.Resize(0, 800));
                            }
                            else
                            {
                                pic.Mutate(x => x.Resize(800, 0));
                            }

                        }
                        pic.SaveAsJpeg(preservedAspectRatio, new JpegEncoder { Quality = 70 });
                        SixLabors.ImageSharp.Image.Load(preservedAspectRatio.ToArray(), out format);
                        image.Thumbnails.Add(new Thumbnail
                        {
                            AspectRatio = (double)pic.Width / pic.Height,
                            Type = ThumbnailType.PreservedAspectRatio,
                            ThumbnailContentType = format.Name,
                            ThumbnailHeight = pic.Height,
                            ThumbnailWidth = pic.Width,
                            Blob = preservedAspectRatio.ToArray()
                        });
                    }

                    _context.Images.Update(image);
                    await _context.SaveChangesAsync();
                    successfulProcessing++;
                }
                catch
                {
                    DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", image.ImageId.ToString());
                    System.IO.File.Delete(DirectoryPath);
                    failedProcessing++;
                }
            }
            if (failedProcessing == 0)
            {
                SuccessMessage = "All images has been uploaded successfuly.";
            }
            else
            {
                ErrorMessage = "There were " + failedProcessing + " errors during uploading and processing of images.";
            }

            return RedirectToPage("/MyAlbum", new { albumId = albumId });
        }
        public async Task<IActionResult> OnGetDeleteImage(Guid id, int albumId, string albumMethod, OrderBy albumOrderBy)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return RedirectToPage("Error");
            }

            Image = _context.Images.
                Select(x => new Models.Image
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
                .Where(x => x.AlbumId == Album.AlbumId && x.ImageId == id).SingleOrDefault();
            if (Image == null)
            {
                return RedirectToPage("Error");
            }

            Thumbnails = _context.Thumbnails.Where(x => x.ImageId == id);

            _context.Thumbnails.RemoveRange(Thumbnails.AsQueryable());

            var imageCommentUsers = _context.CommentUsers.Where(x => x.Comment.ImageId == id);

            _context.CommentUsers.RemoveRange(imageCommentUsers);

            _context.Comments.RemoveRange(Image.Comments.AsQueryable());

            DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", Image.ImageId.ToString());

            _context.Images.Remove(Image);
            System.IO.File.Delete(DirectoryPath);
            await _context.SaveChangesAsync();

            SuccessMessage = "Chosen image has been successfully deleted.";

            return RedirectToPage("/MyAlbum", new { albumId = albumId, method = albumMethod, orderBy = albumOrderBy });
        }

        public IActionResult OnGetDownloadImage(Guid? id, int albumId)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
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
                    Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId == albumId).SingleOrDefault();
                    if (Album == null)
                    {
                        return RedirectToPage("Error");
                    }

                    Image = _context.Images.AsNoTracking().Where(x => x.AlbumId == Album.AlbumId && x.ImageId == id).SingleOrDefault();
                    if (Image == null)
                    {
                        return RedirectToPage("Error");
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

        public async Task<IActionResult> OnGetDeleteAllImagesFromAlbum(int albumId)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();

            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }
            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return RedirectToPage("Error");
            }

            Images = _context.Images.
                Select(x => new Models.Image
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

                }).Where(x => x.AlbumId == Album.AlbumId).AsEnumerable();

            if (Images.Any() == false)
            {
                ErrorMessage = "There is nothing to delete.";
            }
            else
            {
                Thumbnails = _context.Thumbnails.Where(x => x.Image.AlbumId == Album.AlbumId);

                _context.Thumbnails.RemoveRange(Thumbnails.AsQueryable());

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
            }

            return RedirectToPage("/MyAlbum", new { albumId = albumId });

        }

        public async Task<IActionResult> OnPostChosenAlbumAddImages(string[] listOfImages, int albumId, string albumMethod, OrderBy albumOrderBy)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();

            Album = _context.Albums.Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return RedirectToPage("Error");
            }

            if (listOfImages == null)
            {
                return RedirectToPage("Error");

            }

            if (listOfImages.Any() == true)
            {
                foreach (var item in listOfImages)
                {
                    Image = _context.Images.Where(x => x.ImageId == Guid.Parse(item)).SingleOrDefault();
                    Image.AlbumId = Album.AlbumId;
                    _context.Images.Update(Image);
                }
                SuccessMessage = "Images from different albums have been successfully moved to current album.";
            }

            await _context.SaveChangesAsync();

            if (listOfImages.Any() == true)
            {
                return RedirectToPage("/MyAlbum", new { albumId = albumId });

            }
            else
            {
                return RedirectToPage("/MyAlbum", new { albumId = albumId, method = albumMethod, orderBy = albumOrderBy });

            }
        }


        public async Task<IActionResult> OnPostChosenAlbumAccessibility(int albumId, Accessibility? albumAccessibility, string albumMethod, OrderBy albumOrderBy)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();

            Album = _context.Albums.Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId == albumId).SingleOrDefault();
            if (Album == null)
            {
                return RedirectToPage("Error");
            }

            if (albumAccessibility == null || (albumAccessibility != Accessibility.Private && albumAccessibility != Accessibility.Public))
            {
                return RedirectToPage("Error");

            }


            Images = _context.Images.Where(x => x.AlbumId == Album.AlbumId).AsEnumerable();

            Album.AlbumAccessibility = (Accessibility)albumAccessibility;
            _context.Albums.Update(Album);

            foreach (var item in Images)
            {
                item.ImageAccessibility = Album.AlbumAccessibility;
                _context.Images.Update(item);
            }

            await _context.SaveChangesAsync();


            return RedirectToPage("/MyAlbum", new { albumId = albumId, method = albumMethod, orderBy = albumOrderBy });
        }
    }
}
