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
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;

namespace GalleryDatabase.Pages
{
    [Authorize]
    public class MyGalleryModel : PageModel
    {
        private readonly UserManager<GalleryOwner> _userManager;
        private readonly DropdownList _dropdownList;
        private readonly GalleryDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly SortingHelper _helper;

        public List<SelectListItem> AccessibilityList { get; private set; }

        public List<SelectListItem> GalleryMethodOfSortingList { get; private set; }

        public List<SelectListItem> GalleryOrderByList { get; private set; }

        public List<SelectListItem> AlbumList { get; private set; }

        public List<SelectListItem> AlbumListForCoverImage { get; private set; }

        public List<SelectListItem> ImageList { get; private set; }

        public MyGalleryModel(UserManager<GalleryOwner> userManager, GalleryDbContext context, IWebHostEnvironment environment, DropdownList dropdownList, SortingHelper helper)
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

        [BindProperty]
        public GalleryOwner GalleryOwner { get; set; }

        [BindProperty]
        public GalleryOwnerReadModel GalleryOwnerReadOnly { get; set; }

        [BindProperty]
        public Album Album { get; set; }

        [BindProperty]
        public Models.Image Image { get; set; }

        [BindProperty]
        public string GalleryMethod { get; set; }
        [BindProperty]
        public OrderBy GalleryOrderBy { get; set; }

        public int NumberOfAlbums { get; set; }

        public int NumberOfImages { get; set; }

        public int DefaultAlbumNumberOfImages { get; set; }

        public long DefaultAlbumTotalSize { get; set; }

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

        public IActionResult OnGetMoveToChosenImage(Guid imageId, string galleryMethod, OrderBy galleryOrderBy)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
           x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
           FirstOrDefault();

            Image = _context.Images.AsNoTracking().Where(p => p.AlbumId == Album.AlbumId && p.ImageId == imageId).SingleOrDefault();
            if (Image == null)
            {
                return RedirectToPage("Error");
            }

            return RedirectToPage("/MyGallery", null, new { method = galleryMethod, orderBy = galleryOrderBy }, $"ownerImage{imageId}");
        }


        public IActionResult OnGet(string method, OrderBy orderBy)
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
                ).Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwnerReadOnly == null)
            {
                return NotFound("Gallery owner not found");
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwnerReadOnly.Id &&
           x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
           FirstOrDefault();

            ImagesFromService = _helper.GetImagesForCurrentAlbum(Album.AlbumId, method, orderBy, out string Method, out OrderBy OrderBy);
            GalleryOrderBy = OrderBy;
            GalleryMethod = Method;

            NumberOfAlbums = _context.Albums.AsNoTracking().Where(s => s.GalleryOwnerId == GalleryOwnerReadOnly.Id && s.AlbumId != Album.AlbumId).ToList().Count;
            NumberOfImages = _context.Images.AsNoTracking().Where(s => s.Album.GalleryOwnerId == GalleryOwnerReadOnly.Id).ToList().Count;
            DefaultAlbumNumberOfImages = _context.Images.AsNoTracking().Where(s => s.AlbumId == Album.AlbumId).ToList().Count;
            DefaultAlbumTotalSize = _context.Images.AsNoTracking().Where(s => s.AlbumId == Album.AlbumId).Select(x => x.Size).Sum();

            AccessibilityList = _dropdownList.GetAccessibilityDropdown();
            GalleryMethodOfSortingList = _dropdownList.GetMethodOfSortingDropdown();
            GalleryOrderByList = _dropdownList.GetOrderByDropdown();
            AlbumList = _dropdownList.GetAlbumDropdown(Album.AlbumId, false);
            AlbumListForCoverImage = _dropdownList.GetAlbumDropdown(Album.AlbumId, true);
            ImageList = _dropdownList.GetImageDropdown(GalleryOwnerReadOnly.Id, Album.AlbumId);
            return Page();
        }
        public IActionResult OnGetDownloadAllUnclassifiedImages()
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();

            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
            x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
            FirstOrDefault();

            Images = _context.Images.AsNoTracking().Where(x => x.AlbumId == Album.AlbumId).AsEnumerable();
            if (Images.Any() == false)
            {
                ErrorMessage = "There is nothing to download.";
                return RedirectToPage("/MyGallery");
            }
            else
            {

                DirectoryPath = Path.Combine(_environment.ContentRootPath, @"Uploads\");

                using (FileStream zip = new FileStream(DirectoryPath + "Photos.zip", FileMode.Create))
                {
                    zip.Dispose();
                }

                using (ZipArchive archive = ZipFile.Open(DirectoryPath + "Photos.zip", ZipArchiveMode.Update))
                {
                    foreach (var item in Images)
                    {
                        archive.CreateEntryFromFile(DirectoryPath + item.ImageId.ToString(), item.OriginalName, CompressionLevel.NoCompression);
                    }
                }

                byte[] result = System.IO.File.ReadAllBytes(DirectoryPath + "Photos.zip");
                System.IO.File.Delete(DirectoryPath + "Photos.zip");

                return File(result, "application/zip", "Photos.zip");
            }
        }

        public IActionResult OnGetThumbnail(string filename, ThumbnailType type)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
           x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
           FirstOrDefault();

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

        public IActionResult OnPostGallerySortingSettings()
        {
            return RedirectToPage("/MyGallery", new { method = GalleryMethod, orderBy = GalleryOrderBy });
        }

        public async Task<IActionResult> OnPostGalleryAccessibility(Accessibility? galleryAccessibility, string galleryMethod, OrderBy galleryOrderBy)
        {
            GalleryOwner = await _userManager.Users.Where(x => x.Id == GetUserId()).SingleOrDefaultAsync();

            if (galleryAccessibility == null || (galleryAccessibility != Accessibility.Private && galleryAccessibility != Accessibility.Public))
            {
                return NotFound("Accessibility input is invalid");
            }

            Albums = _context.Albums.Where(x => x.GalleryOwnerId == GalleryOwner.Id).AsEnumerable();
            Images = _context.Images.Where(x => x.Album.GalleryOwnerId == GalleryOwner.Id).AsEnumerable();

            GalleryOwner.GalleryAccessibility = (Accessibility)galleryAccessibility;
            await _userManager.UpdateAsync(GalleryOwner);

            foreach (var item in Albums)
            {
                item.AlbumAccessibility = GalleryOwner.GalleryAccessibility;
                _context.Albums.Update(item);
            }
            foreach (var item in Images)
            {
                item.ImageAccessibility = GalleryOwner.GalleryAccessibility;
                _context.Images.Update(item);
            }
            await _context.SaveChangesAsync();

            return RedirectToPage("/MyGallery", new { method = galleryMethod, orderBy = galleryOrderBy });
        }


        public async Task<IActionResult> OnPostUnclassifiedAlbumAddImages(string[] listOfImages, string galleryMethod, OrderBy galleryOrderBy)
        {
            GalleryOwner = await _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefaultAsync();

            if (listOfImages == null)
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
            x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
            FirstOrDefault();

            if (listOfImages.Any() == true)
            {
                foreach (var item in listOfImages)
                {
                    Image = _context.Images.Where(x => x.ImageId == Guid.Parse(item)).SingleOrDefault();
                    Image.AlbumId = Album.AlbumId;
                    _context.Images.Update(Image);
                }
                SuccessMessage = "Images from different albums have been successfully changed to unclassified.";
            }

            await _context.SaveChangesAsync();

            if (listOfImages.Any() == true)
            {
                return RedirectToPage("/MyGallery");

            }
            else
            {
                return RedirectToPage("/MyGallery", new { method = galleryMethod, orderBy = galleryOrderBy });

            }

        }

        public async Task<IActionResult> OnPostUnclassifiedAlbumAccessibility(Accessibility? albumAccessibility, string galleryMethod, OrderBy galleryOrderBy)
        {
            GalleryOwner = await _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefaultAsync();

            if (albumAccessibility == null || (albumAccessibility != Accessibility.Private && albumAccessibility != Accessibility.Public))
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
            x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
            FirstOrDefault();
            Images = _context.Images.Where(x => x.AlbumId == Album.AlbumId).AsEnumerable();

            Album.AlbumAccessibility = (Accessibility)albumAccessibility;
            _context.Albums.Update(Album);

            foreach (var item in Images)
            {
                item.ImageAccessibility = Album.AlbumAccessibility;
                _context.Images.Update(item);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/MyGallery", new { method = galleryMethod, orderBy = galleryOrderBy });
        }

        public async Task<IActionResult> OnPostImageDetailsSettings(Guid id, string albumId, Accessibility? accessibility, string[] listOfAlbums, 
            Guid? indexOfImage, int chosenImageSavedSettingsIndex, string galleryMethod, OrderBy galleryOrderBy)
        {
            GalleryOwner = await _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefaultAsync();

            if (accessibility == null || (accessibility != Accessibility.Private && accessibility != Accessibility.Public))
            {
                return RedirectToPage("Error");
            }
            else if (listOfAlbums == null)
            {
                return RedirectToPage("Error");
            }
            else if (albumId == null)
            {
                return RedirectToPage("Error");
            }
            else if (indexOfImage == null)
            {
                return RedirectToPage("Error");
            }
            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
         x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
         FirstOrDefault();

            Image = _context.Images.Where(x => x.AlbumId == Album.AlbumId && x.ImageId == id).SingleOrDefault();
            if (Image == null)
            {
                return RedirectToPage("Error");
            }

            Albums = _context.Albums.Where(x => x.GalleryOwnerId == GalleryOwner.Id && x.AlbumId != Album.AlbumId).AsEnumerable();

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

            if (albumId != Album.AlbumId.ToString())
            {
                Image.AlbumId = int.Parse(albumId);
                _context.Images.Update(Image);
                await _context.SaveChangesAsync();
                return RedirectToPage("/MyGallery");
            }

            _context.Images.Update(Image);
            await _context.SaveChangesAsync();

            SaveChangesSuccessMessage = "Your image settings has been updated.";
            ChosenImageSavedSettingsIndex = chosenImageSavedSettingsIndex;
            return RedirectToPage("/MyGallery", null, new { method = galleryMethod, orderBy = galleryOrderBy }, $"galleryImageDetails{indexOfImage}");
        }

        public async Task<IActionResult> OnPostGalleryUpload()
        {
            List<Models.Image> imagesList = new List<Models.Image>();

            GalleryOwner = _userManager.Users.Where(x => x.Id == GetUserId()).SingleOrDefault();
            Album = await _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
            x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa")
                .FirstOrDefaultAsync();

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

                var tempImage = image;

                image = new Models.Image
                {
                    Size = tempImage.Size,
                    ImageContentType = tempImage.ImageContentType,
                    OriginalName = tempImage.OriginalName,
                    AlbumId = Album.AlbumId,
                    ImageAccessibility = tempImage.ImageAccessibility,
                    UploadedAt = tempImage.UploadedAt,
                    ImageId = tempImage.ImageId
                };

                _context.Images.Remove(tempImage);

                imagesList.Add(image);

                try
                {
                    if (image.ImageContentType.Contains("xml") == true || image.ImageContentType.StartsWith("image") == false || image.Size == 0)
                    {
                        imagesList.Remove(image);
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
                        pic.SaveAsJpeg(square, new JpegEncoder { Quality = 100 });
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
                        if (pic.Width > 1280 || pic.Height > 1280)
                        {
                            if (pic.Width < pic.Height)
                            {
                                pic.Mutate(x => x.Resize(0, 1280));
                            }
                            else
                            {
                                pic.Mutate(x => x.Resize(1280, 0));
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
                    
                    successfulProcessing++;
                }
                catch
                {
                    DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", image.ImageId.ToString());
                    System.IO.File.Delete(DirectoryPath);
                    failedProcessing++;
                }
            }

            _context.Images.AddRange(imagesList);

            await _context.SaveChangesAsync();

            if (failedProcessing == 0)
            {
                SuccessMessage = "All images has been uploaded successfuly.";
            }
            else
            {
                ErrorMessage = "There were " + failedProcessing + " errors during uploading and processing of images.";
            }
            return RedirectToPage("/MyGallery");
        }
        public async Task<IActionResult> OnGetDeleteUnclassifiedImage(Guid id, string galleryMethod, OrderBy galleryOrderBy)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
         x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
         FirstOrDefault();

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
                    
                }).
                Where(x => x.AlbumId == Album.AlbumId && x.ImageId == id).SingleOrDefault();

            if (Image == null)
            {
                return RedirectToPage("Error");
            }

            Thumbnails = _context.Thumbnails.Where(x => x.ImageId == id);

            _context.Thumbnails.RemoveRange(Thumbnails.AsQueryable());

            var imageCommentUsers = _context.CommentUsers.Where(x => x.Comment.ImageId == Image.ImageId);

            _context.CommentUsers.RemoveRange(imageCommentUsers);

            _context.Comments.RemoveRange(Image.Comments.AsQueryable());

            DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", Image.ImageId.ToString());

            _context.Images.Remove(Image);
            System.IO.File.Delete(DirectoryPath);
            await _context.SaveChangesAsync();

            SuccessMessage = "Chosen image has been successfully deleted.";

            return RedirectToPage("/MyGallery", new { method = galleryMethod, orderBy = galleryOrderBy });
        }

        public IActionResult OnGetDownloadUnclassifiedImage(Guid? id)
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
                    Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id &&
       x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
       FirstOrDefault();
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

        public async Task<IActionResult> OnGetDeleteAllUnclassifiedImages()
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Id == GetUserId()).SingleOrDefault();

            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == GalleryOwner.Id 
                && x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").
                FirstOrDefault();

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
                    
                }).Where(x => x.AlbumId == Album.AlbumId);

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

                    _context.CommentUsers.RemoveRange(imageCommentUsers.AsQueryable());

                    _context.Comments.RemoveRange(item.Comments.AsQueryable());

                    DirectoryPath = Path.Combine(_environment.ContentRootPath, "Uploads", item.ImageId.ToString());
                    _context.Images.Remove(item);
                    System.IO.File.Delete(DirectoryPath);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/MyGallery");
        }

       
    }
}
