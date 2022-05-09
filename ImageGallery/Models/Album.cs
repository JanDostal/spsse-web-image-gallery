using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GalleryDatabase.Models
{
    public class Album
    {
        public int AlbumId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public Accessibility AlbumAccessibility { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        public Image Cover { get; set; }

        [Required]
        public GalleryOwner GalleryOwner { get; set; }

        public string GalleryOwnerId { get; set; }

        public Guid? CoverImageId { get; set; }

        public ICollection<Image> Images { get; set; }

    }
}
