using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryDatabase.Models
{
    public class ImageReadModel
    {
        public Guid ImageId { get; set; }

        public DateTime UploadedAt { get; set; }

        public string OriginalName { get; set; }

        public string ImageContentType { get; set; }

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public long Size { get; set; }

        public Accessibility ImageAccessibility { get; set; }

        public DateTime DateTaken { get; set; }

        public Album Album { get; set; }

        public int AlbumId { get; set; }

        public Thumbnail Thumbnail { get; set; }

        public Album DefaultAlbum { get; set; }

        public string GalleryOwnerEmail { get; set; }

        public string GalleryOwnerUserName { get; set; }

        public int AlbumsCount { get; set; }

        public ICollection<Comment> Comments { get; set; }

        public int NumberOfComments { get; set; }
    }
}
