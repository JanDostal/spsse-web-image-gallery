using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryDatabase.Models
{
    public class AlbumReadModel
    {
        public int AlbumId { get; set; }

        public string Name { get; set; }

        public Accessibility AlbumAccessibility { get; set; }

        public DateTime DateCreated { get; set; }

        public GalleryOwner GalleryOwner { get; set; }

        public Image Cover { get; set; }

        public string GalleryOwnerId { get; set; }

        public Album DefaultAlbum { get; set; }

        public Guid? CoverImageId { get; set; }

        public ICollection<long> ImagesSizes { get; set; }
    }
}
