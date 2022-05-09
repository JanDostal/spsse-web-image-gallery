using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryDatabase.Models
{
    public class CommentReadModel
    {
        public int CommentId { get; set; }

        public string Content { get; set; }

        public long Likes { get; set; }

        public long Dislikes { get; set; }

        public decimal ControversyScore { get; set; }

        public bool WasEdited { get; set; }

        public decimal ReactionScore { get; set; }

        public double TotalWeightFromReactions { get; set; }

        public DateTime DatePosted { get; set; }

        public Comment CommentedComment { get; set; }

        public string CommentedCommentUserName { get; set; }

        public int? CommentedCommentId { get; set; }

        public int NumberOfCommentsReactingtoThisComment { get; set; }

        public int NumberOfRepliesOfParentComment { get; set; }

        public DateTime? DateUserWasRemoved { get; set; }

        public ICollection<Comment> CommentsReactingtoThisComment { get; set; }

        public Image Image { get; set; }

        public Guid ImageId { get; set; }

        public GalleryOwner User { get; set; }

        public string UserId { get; set; }

        public ICollection<CommentUser> CommentUsers { get; set; }

        public long TotalNumberOfReactions { get; set; }

        public string NumberOfLikes { get; set; }

        public string NumberOfDislikes { get; set; }

        public CommentUser CommentUser { get; set; }

        public string TimeDifferenceOfDatePosted { get; set; } 

        public string TimeDifferenceOfDateUserWasRemoved { get; set; }
    }
}
