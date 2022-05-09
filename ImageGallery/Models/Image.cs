using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryDatabase.Models
{
    public class Image
    {
        public Guid ImageId { get; set; }

        [Required]
        public DateTime UploadedAt { get; set; }

        [Required]
        public string OriginalName { get; set; }

        [Required]
        public string ImageContentType { get; set; }

        [Required]
        public int ImageWidth { get; set; }

        [Required]
        public int ImageHeight { get; set; }

        [Required]
        public long Size { get; set; }

        [Required]
        public Accessibility ImageAccessibility { get; set; }

        [Required]
        public DateTime DateTaken { get; set; }

        [Required]
        public Album Album { get; set; }

        public int AlbumId { get; set; }

        [Required]
        public ICollection<Thumbnail> Thumbnails { get; set; }

        public ICollection<Album> Albums { get; set; }

        public ICollection<Comment> Comments { get; set; }

    }
}
