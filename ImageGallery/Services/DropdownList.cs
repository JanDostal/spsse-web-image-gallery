using GalleryDatabase.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalleryDatabase.Services
{
    public class DropdownList
    {
        private List<SelectListItem> dropdown;

        private readonly GalleryDbContext _context;

        public DropdownList(GalleryDbContext context)
        {
            _context = context;
        }

        private IEnumerable<Image> Images { get; set; }

        private IEnumerable<Album> Albums { get; set; }

        public List<SelectListItem> GetAccessibilityDropdown()
        {
            dropdown = new List<SelectListItem>
            {
                new SelectListItem("Private", $"{Accessibility.Private}"),
                new SelectListItem("Public", $"{Accessibility.Public}")
            };

            return dropdown;
        }
        public List<SelectListItem> GetMethodOfSortingDropdown()
        {
            dropdown = new List<SelectListItem>
            {
                new SelectListItem("Date uploaded", "Date uploaded"),
                new SelectListItem("Date taken", "Date taken"),
                new SelectListItem("Original name", "Original name"),
                new SelectListItem("Size of images", "Size of images"),
                new SelectListItem("Resolution", "Resolution"),
                new SelectListItem("Content type", "Content type"),
                new SelectListItem("Total number Of comments", "Total number Of comments"),
                new SelectListItem("Image set as cover image (occurrences)", "Image set as cover image (occurrences)")
            };

            return dropdown;
        }
        public List<SelectListItem> GetMethodOfSortingForComments()
        {
            dropdown = new List<SelectListItem>
            {
                new SelectListItem("Date posted", "Date posted"),
                new SelectListItem("Popularity", "Popularity"),
                new SelectListItem("Controversy", "Controversy"),
                new SelectListItem("Total number of reactions", "Total number of reactions")
            };

            return dropdown;
        }

        public List<SelectListItem> GetMethodOfSortingDropdownForAlbums()
        {
            dropdown = new List<SelectListItem>
            {
                new SelectListItem("Date created", "Date created"),
                new SelectListItem("Name", "Name"),
                new SelectListItem("Total size of album", "Total size of album"),
                new SelectListItem("Number of images in album", "Number of images in album")
            };

            return dropdown;
        }

        public List<SelectListItem> GetOrderByDropdown()
        {
            dropdown = new List<SelectListItem>
            {
                new SelectListItem("Descending", $"{OrderBy.Descending}"),
                new SelectListItem("Ascending", $"{OrderBy.Ascending}")

            };

            return dropdown;
        }
        public List<SelectListItem> GetImageDropdown(string galleryId, int? albumId = null)
        {
            dropdown = new List<SelectListItem>();

            if (albumId != null)
            {
                Images = _context.Images.AsNoTracking().Where(p => p.Album.GalleryOwnerId == galleryId && p.AlbumId != albumId).OrderBy(x => x.OriginalName).AsEnumerable();
            }
            else
            {
                Images = _context.Images.AsNoTracking().Where(p => p.Album.GalleryOwnerId == galleryId).OrderBy(x => x.OriginalName).AsEnumerable();
                dropdown.Add(new SelectListItem("None", "None"));
            }

            foreach (var item in Images)
            {
                dropdown.Add(new SelectListItem($"{item.OriginalName}", $"{item.ImageId}"));
            };

            return dropdown;
        }

        public List<SelectListItem> GetAlbumDropdown(int albumId, bool IsForCoverImage)
        {
            dropdown = new List<SelectListItem>();
            var currentAlbum = _context.Albums.AsNoTracking().Where(x => x.AlbumId == albumId).SingleOrDefault();
            var unclassifiedAlbum = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == currentAlbum.GalleryOwnerId &&
            x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIu" +
            "Ql3eLq4pqvkr2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").FirstOrDefault();
            Albums = _context.Albums.AsNoTracking().Where(p => p.GalleryOwnerId == currentAlbum.GalleryOwnerId && p.AlbumId != unclassifiedAlbum.AlbumId).OrderBy(x => x.Name)
                .AsEnumerable();

            if (IsForCoverImage == true)
            {
                dropdown.Add(new SelectListItem("None (Reset)", "None"));
            }
            else
            {
                dropdown.Add(new SelectListItem("None", $"{unclassifiedAlbum.AlbumId}"));

            }

            foreach (var item in Albums)
            {
                dropdown.Add(new SelectListItem($"{item.Name}", $"{item.AlbumId}"));
            };

            return dropdown;
        }
    }
}
