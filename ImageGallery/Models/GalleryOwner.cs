using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GalleryDatabase.Models
{
    public class GalleryOwner : IdentityUser
    {
        [Required]
        public string UserNameForComments{ get; set; }

        [Required]
        public Accessibility GalleryAccessibility { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

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
