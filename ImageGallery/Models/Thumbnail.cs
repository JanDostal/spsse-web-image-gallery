using System;
using System.ComponentModel.DataAnnotations;

namespace GalleryDatabase.Models
{
    public class Thumbnail
    {
        public int ThumbnailId { get; set; }

        [Required]
        public byte[] Blob { get; set; }

        [Required]
        public double AspectRatio { get; set; }

        [Required]
        public string ThumbnailContentType { get; set; }

        [Required]
        public int ThumbnailWidth { get; set; }

        [Required]
        public int ThumbnailHeight { get; set; }

        [Required]
        public Image Image { get; set; }

        [Required]
        public ThumbnailType Type { get; set; }

        public Guid ImageId { get; set; }
    }

    public enum ThumbnailType
    {
        Square,
        PreservedAspectRatio
    }
}
