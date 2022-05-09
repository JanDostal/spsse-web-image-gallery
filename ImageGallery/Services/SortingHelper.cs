using GalleryDatabase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using GalleryDatabase.Helpers;

namespace GalleryDatabase.Services
{
    public class SortingHelper
    {
        private readonly GalleryDbContext _context;

        public SortingHelper(GalleryDbContext context)
        {
            _context = context;
        }

        private IEnumerable<ImageReadModel> Images { get; set; }

        private IEnumerable<AlbumReadModel> Albums { get; set; }

        public IEnumerable<CommentReadModel> Comments { get; set; }

        private IEnumerable<ImageReadModel> TemplateImages { get; set; }

        private IEnumerable<AlbumReadModel> TemplateAlbums { get; set; }

        public IEnumerable<CommentReadModel> TemplateComments { get; set; }

        public Album Album { get; set; }

        public Comment Comment { get; set; }

        public IEnumerable<ImageReadModel> GetImagesForCurrentAlbum(int albumId, string method, OrderBy orderBy, out string Method, out OrderBy OrderBy, bool addAccessibility = false, bool IsReadOnly = false)
        {
            Album = _context.Albums.AsNoTracking().Where(x => x.AlbumId == albumId).SingleOrDefault();
            if (addAccessibility == true)
            {
                TemplateImages = _context.Images.AsNoTracking().
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
                        NumberOfComments = x.Comments.Where(x => x.DateUserWasRemoved == null).ToList().Count
                    })
                    .Where(x => x.AlbumId == Album.AlbumId && x.ImageAccessibility == Accessibility.Public);
            }
            else
            {
                TemplateImages = _context.Images.AsNoTracking().
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
                        Thumbnail = x.Thumbnails.Where(x => x.Type == ThumbnailType.PreservedAspectRatio).SingleOrDefault(),
                        NumberOfComments = x.Comments.Where(x => x.DateUserWasRemoved == null).ToList().Count
                    })
                    .Where(x => x.AlbumId == Album.AlbumId);
            }

            if (IsReadOnly == true)
            {
                if (orderBy == OrderBy.Ascending)
                {
                    OrderBy = OrderBy.Ascending;
                    switch (method)
                    {
                        case "Size of images":
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenBy(x => x.Size).AsEnumerable();
                            Method = "Size of images";
                            break;
                        case "Date taken":
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenBy(x => x.DateTaken).AsEnumerable();
                            Method = "Date taken";
                            break;
                        case "Original name":
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenBy(x => x.OriginalName).AsEnumerable();
                            Method = "Original name";
                            break;
                        case "Resolution":
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenBy(x => (x.ImageWidth, x.ImageHeight)).AsEnumerable();
                            Method = "Resolution";
                            break;
                        case "Content type":
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenBy(x => x.ImageContentType).AsEnumerable();
                            Method = "Content type";
                            break;
                        case "Image set as cover image (occurrences)":
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenBy(x => x.AlbumsCount).AsEnumerable();
                            Method = "Image set as cover image (occurrences)";
                            break;
                        case "Total number Of comments":
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenBy(x => x.NumberOfComments).AsEnumerable();
                            Method = "Total number Of comments";
                            break;
                        default:
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenBy(x => x.UploadedAt).AsEnumerable();
                            Method = "Date uploaded";
                            break;
                    }
                }
                else
                {
                    OrderBy = OrderBy.Descending;
                    switch (method)
                    {
                        case "Size of images":
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenByDescending(x => x.Size).AsEnumerable();
                            Method = "Size of images";
                            break;
                        case "Date taken":
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenByDescending(x => x.DateTaken).AsEnumerable();
                            Method = "Date taken";
                            break;
                        case "Original name":
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenByDescending(x => x.OriginalName).AsEnumerable();
                            Method = "Original name";
                            break;
                        case "Resolution":
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenByDescending(x => (x.ImageWidth, x.ImageHeight)).AsEnumerable();
                            Method = "Resolution";
                            break;
                        case "Content type":
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenByDescending(x => x.ImageContentType).AsEnumerable();
                            Method = "Content type";
                            break;
                        case "Image set as cover image (occurrences)":
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenByDescending(x => x.AlbumsCount).AsEnumerable();
                            Method = "Image set as cover image (occurrences)";
                            break;
                        case "Total number Of comments":
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenByDescending(x => x.NumberOfComments).AsEnumerable();
                            Method = "Total number Of comments";
                            break;
                        default:
                            Images = TemplateImages.OrderByDescending(x => x.ImageAccessibility).ThenByDescending(x => x.UploadedAt).AsEnumerable();
                            Method = "Date uploaded";
                            break;
                    }
                }
            }
            else
            {
                if (orderBy == OrderBy.Ascending)
                {
                    OrderBy = OrderBy.Ascending;
                    switch (method)
                    {
                        case "Size of images":
                            Images = TemplateImages.OrderBy(x => x.Size).AsEnumerable();
                            Method = "Size of images";
                            break;
                        case "Date taken":
                            Images = TemplateImages.OrderBy(x => x.DateTaken).AsEnumerable();
                            Method = "Date taken";
                            break;
                        case "Original name":
                            Images = TemplateImages.OrderBy(x => x.OriginalName).AsEnumerable();
                            Method = "Original name";
                            break;
                        case "Resolution":
                            Images = TemplateImages.OrderBy(x => (x.ImageWidth, x.ImageHeight)).AsEnumerable();
                            Method = "Resolution";
                            break;
                        case "Content type":
                            Images = TemplateImages.OrderBy(x => x.ImageContentType).AsEnumerable();
                            Method = "Content type";
                            break;
                        case "Image set as cover image (occurrences)":
                            Images = TemplateImages.OrderBy(x => x.AlbumsCount).AsEnumerable();
                            Method = "Image set as cover image (occurrences)";
                            break;
                        case "Total number Of comments":
                            Images = TemplateImages.OrderBy(x => x.NumberOfComments).AsEnumerable();
                            Method = "Total number Of comments";
                            break;
                        default:
                            Images = TemplateImages.OrderBy(x => x.UploadedAt).AsEnumerable();
                            Method = "Date uploaded";
                            break;
                    }
                }
                else
                {
                    OrderBy = OrderBy.Descending;
                    switch (method)
                    {
                        case "Size of images":
                            Images = TemplateImages.OrderByDescending(x => x.Size).AsEnumerable();
                            Method = "Size of images";
                            break;
                        case "Date taken":
                            Images = TemplateImages.OrderByDescending(x => x.DateTaken).AsEnumerable();
                            Method = "Date taken";
                            break;
                        case "Original name":
                            Images = TemplateImages.OrderByDescending(x => x.OriginalName).AsEnumerable();
                            Method = "Original name";
                            break;
                        case "Resolution":
                            Images = TemplateImages.OrderByDescending(x => (x.ImageWidth, x.ImageHeight)).AsEnumerable();
                            Method = "Resolution";
                            break;
                        case "Content type":
                            Images = TemplateImages.OrderByDescending(x => x.ImageContentType).AsEnumerable();
                            Method = "Content type";
                            break;
                        case "Image set as cover image (occurrences)":
                            Images = TemplateImages.OrderByDescending(x => x.AlbumsCount).AsEnumerable();
                            Method = "Image set as cover image (occurrences)";
                            break;
                        case "Total number Of comments":
                            Images = TemplateImages.OrderByDescending(x => x.NumberOfComments).AsEnumerable();
                            Method = "Total number Of comments";
                            break;
                        default:
                            Images = TemplateImages.OrderByDescending(x => x.UploadedAt).AsEnumerable();
                            Method = "Date uploaded";
                            break;
                    }
                }
            }

           
            return Images;
        }

        public IEnumerable<AlbumReadModel> GetAlbumsForCurrentGallery(string galleryOwnerId, string method, OrderBy orderBy, out string Method, out OrderBy OrderBy, bool IsReadOnly = false)
        {
            Album = _context.Albums.AsNoTracking().Where(x => x.GalleryOwnerId == galleryOwnerId &&
            x.Name == "cttSMcROVQhxfkvTfoG7SOWzIKkuSQXDLWhKGruQrc90FmRykNpeklrxooXzdgEyv8lIuQl3eLq4pqvk" +
            "r2YOHeEOtyABF4I9ySvLcoh0i5hL1OS3MwDDcYun9Vvzdko9I0nzlYhfBAWcHAd7LQ9cuw1p5UgXMmSa").FirstOrDefault();

            TemplateAlbums = _context.Albums.AsNoTracking().
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
                      ImagesSizes = x.Images.AsQueryable().Select(x => x.Size).ToList()
                  })
                .Where(x => x.GalleryOwnerId == galleryOwnerId && x.AlbumId != Album.AlbumId);

            if (IsReadOnly == true)
            {
                if (orderBy == OrderBy.Ascending)
                {
                    OrderBy = OrderBy.Ascending;
                    switch (method)
                    {
                        case "Name":
                            Albums = TemplateAlbums.OrderByDescending(x => x.AlbumAccessibility).ThenBy(x => x.Name).AsEnumerable();
                            Method = "Name";
                            break;
                        case "Total size of album":
                            Albums = TemplateAlbums.OrderByDescending(x => x.AlbumAccessibility).ThenBy(x => x.ImagesSizes.Sum()).AsEnumerable();
                            Method = "Total size of album";
                            break;
                        case "Number of images in album":
                            Albums = TemplateAlbums.OrderByDescending(x => x.AlbumAccessibility).ThenBy(x => x.ImagesSizes.Count).AsEnumerable();
                            Method = "Number of images in album";
                            break;
                        default:
                            Albums = TemplateAlbums.OrderByDescending(x => x.AlbumAccessibility).ThenBy(x => x.DateCreated).AsEnumerable();
                            Method = "Date created";
                            break;
                    }
                }
                else
                {
                    OrderBy = OrderBy.Descending;
                    switch (method)
                    {
                        case "Name":
                            Albums = TemplateAlbums.OrderByDescending(x => x.AlbumAccessibility).ThenByDescending(x => x.Name).AsEnumerable();
                            Method = "Name";
                            break;
                        case "Total size of album":
                            Albums = TemplateAlbums.OrderByDescending(x => x.AlbumAccessibility).ThenByDescending(x => x.ImagesSizes.Sum()).AsEnumerable();
                            Method = "Total size of album";
                            break;
                        case "Number of images in album":
                            Albums = TemplateAlbums.OrderByDescending(x => x.AlbumAccessibility).ThenByDescending(x => x.ImagesSizes.Count).AsEnumerable();
                            Method = "Number of images in album";
                            break;
                        default:
                            Albums = TemplateAlbums.OrderByDescending(x => x.AlbumAccessibility).ThenByDescending(x => x.DateCreated).AsEnumerable();
                            Method = "Date created";
                            break;
                    }
                }
            }
            else
            {
                if (orderBy == OrderBy.Ascending)
                {
                    OrderBy = OrderBy.Ascending;
                    switch (method)
                    {
                        case "Name":
                            Albums = TemplateAlbums.OrderBy(x => x.Name).AsEnumerable();
                            Method = "Name";
                            break;
                        case "Total size of album":
                            Albums = TemplateAlbums.OrderBy(x => x.ImagesSizes.Sum()).AsEnumerable();
                            Method = "Total size of album";
                            break;
                        case "Number of images in album":
                            Albums = TemplateAlbums.OrderBy(x => x.ImagesSizes.Count).AsEnumerable();
                            Method = "Number of images in album";
                            break;
                        default:
                            Albums = TemplateAlbums.OrderBy(x => x.DateCreated).AsEnumerable();
                            Method = "Date created";
                            break;
                    }
                }
                else
                {
                    OrderBy = OrderBy.Descending;
                    switch (method)
                    {
                        case "Name":
                            Albums = TemplateAlbums.OrderByDescending(x => x.Name).AsEnumerable();
                            Method = "Name";
                            break;
                        case "Total size of album":
                            Albums = TemplateAlbums.OrderByDescending(x => x.ImagesSizes.Sum()).AsEnumerable();
                            Method = "Total size of album";
                            break;
                        case "Number of images in album":
                            Albums = TemplateAlbums.OrderByDescending(x => x.ImagesSizes.Count).AsEnumerable();
                            Method = "Number of images in album";
                            break;
                        default:
                            Albums = TemplateAlbums.OrderByDescending(x => x.DateCreated).AsEnumerable();
                            Method = "Date created";
                            break;
                    }
                }
            }
            return Albums;
        }

        public IEnumerable<CommentReadModel> GetCommentsForCurrentImage (Guid imageId, string method, OrderBy orderBy, out string Method, out OrderBy OrderBy, string currentUserId)
        {
            TemplateComments = _context.Comments.AsNoTracking().Select(x => new CommentReadModel
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
                User = x.User,
                UserId = x.UserId,
                ImageId = x.ImageId,
                TotalWeightFromReactions = x.TotalWeightFromReactions,
                TotalNumberOfReactions = x.Likes + x.Dislikes,
                CommentUser = _context.CommentUsers.Where(a => a.UserId == currentUserId && a.CommentId == x.CommentId).SingleOrDefault(),
                NumberOfLikes = ConvertToDifferentFormats.GetCommentLikes(x.Likes),
                NumberOfDislikes = ConvertToDifferentFormats.GetCommentDislikes(x.Dislikes),
                TimeDifferenceOfDatePosted = ConvertToDifferentFormats.GetCommentTimeDifferenceOfDatePosted(x.DatePosted),
                TimeDifferenceOfDateUserWasRemoved = ConvertToDifferentFormats.GetCommentTimeDifferenceOfDatePosted(x.DateUserWasRemoved)
            })
             .Where(x => x.ImageId == imageId && x.CommentedCommentId == null);

            if (orderBy == OrderBy.Ascending)
            {
                OrderBy = OrderBy.Ascending;
                switch (method)
                {
                    case "Total number of reactions":
                        Comments = TemplateComments.OrderBy(x => x.TotalNumberOfReactions).AsEnumerable();
                        Method = "Total number of reactions";
                        break;
                    case "Popularity":
                        Comments = TemplateComments.OrderBy(x => (x.ReactionScore, x.TotalWeightFromReactions)).AsEnumerable();
                        Method = "Popularity";
                        break;
                    case "Controversy":
                        Comments = TemplateComments.OrderBy(x => (x.ControversyScore, x.TotalWeightFromReactions)).AsEnumerable();
                        Method = "Controversy";
                        break;
                    default:
                        Comments = TemplateComments.OrderBy(x => x.DatePosted).AsEnumerable();
                        Method = "Date posted";
                        break;
                }
            }
            else
            {
                OrderBy = OrderBy.Descending;
                switch (method)
                {
                    case "Total number of reactions":
                        Comments = TemplateComments.OrderByDescending(x => x.TotalNumberOfReactions).AsEnumerable();
                        Method = "Total number of reactions";
                        break;
                    case "Popularity":
                        Comments = TemplateComments.OrderByDescending(x => (x.ReactionScore, x.TotalWeightFromReactions)).AsEnumerable();
                        Method = "Popularity";
                        break;
                    case "Controversy":
                        Comments = TemplateComments.OrderByDescending(x => (x.ControversyScore, x.TotalWeightFromReactions)).AsEnumerable();
                        Method = "Controversy";
                        break;
                    default:
                        Comments = TemplateComments.OrderByDescending(x => x.DatePosted).AsEnumerable();
                        Method = "Date posted";
                        break;
                }
            }

            return Comments;
        }
    }
}
