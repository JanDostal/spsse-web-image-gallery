using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryDatabase.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        public string Content { get; set; }

        [Required]
        public long Likes { get; set; }

        [Required]
        public long Dislikes { get; set; }

        [Required]
        public decimal ControversyScore { get; set; }

        [Required]
        public decimal ReactionScore { get; set; }

        [Required]

        public double TotalWeightFromReactions{ get; set; }

        [Required]
        public bool WasEdited { get; set; }

        [Required]
        public DateTime DatePosted { get; set; }

        public Comment CommentedComment { get; set; }

        public int? CommentedCommentId { get; set; }

        public ICollection<Comment> CommentsReactingtoThisComment { get; set; }

        public DateTime? DateUserWasRemoved { get; set; }

        [Required]
        public Image Image { get; set; }

        public Guid ImageId { get; set; }

        public GalleryOwner User { get; set; }

        public string UserId { get; set; }

        public ICollection<CommentUser> CommentUsers { get; set; }
    }
}
