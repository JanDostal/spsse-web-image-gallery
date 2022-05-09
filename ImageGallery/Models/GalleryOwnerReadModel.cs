using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryDatabase.Models
{
    public class GalleryOwnerReadModel : IdentityUser
    {
        public Accessibility GalleryAccessibility { get; set; }

        public long CurrentGallerySize { get; set; }

        public DateTime DateCreated { get; set; }

        public int NumberOfImages { get; set; }

        public int NumberOfAlbums { get; set; }

        public string UserNameForComments { get; set; }

        public int NumberOfReplies { get; set; }

        public ICollection<CommentUser> CommentUsers { get; set; }

        public ICollection<Album> Albums { get; set; }

        public ICollection<Comment> Comments { get; set; }

        public DateTime? ProfileImageUploadedAt { get; set; }

        public string ProfileImageOriginalName { get; set; }

        public string ProfileImageContentType { get; set; }

        public int? ProfileImageWidth { get; set; }

        public int? ProfileImageHeight { get; set; }

        public long? ProfileImageSize { get; set; }

        public byte[] ThumbnailForStandardCommentBlob { get; set; }

        public double? ThumbnailAspectRatio { get; set; }

        public string ThumbnailContentType { get; set; }

        public int? ThumbnailForStandardCommentWidth { get; set; }

        public int? ThumbnailForStandardCommentHeight { get; set; }

        public byte[] ThumbnailForCommentedCommentBlob { get; set; }

        public int? ThumbnailForCommentedCommentWidth { get; set; }

        public int? ThumbnailForCommentedCommentHeight { get; set; }
    }

}
