using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryDatabase.Models
{
    public class CommentUser
    {
        public string UserId { get; set; }

        public GalleryOwner User { get; set; }

        public int CommentId { get; set; }

        public Comment Comment { get; set; }

        [Required]
        public Reaction ReactionType { get; set; }

        public enum Reaction
        { 
            Dislike,
            Like
        }
    }
}
