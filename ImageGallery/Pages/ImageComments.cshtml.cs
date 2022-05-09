using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalleryDatabase.Models;
using GalleryDatabase.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GalleryDatabase.Helpers;

namespace GalleryDatabase.Pages
{
    public class ImageCommentsModel : PageModel
    {
        private UserManager<GalleryOwner> _userManager;
        private readonly DropdownList _dropdownList;
        private GalleryDbContext _context;
        private readonly SortingHelper _helper;

        public List<SelectListItem> CommentsMethodOfSortingList { get; private set; }

        public List<SelectListItem> CommentsOrderByList { get; private set; }

        public ImageCommentsModel(UserManager<GalleryOwner> userManager, DropdownList dropdownList, GalleryDbContext context, SortingHelper helper)
        {
            _context = context;
            _dropdownList = dropdownList;
            _userManager = userManager;
            _helper = helper;
        }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public ImageReadModel Image { get; set; }

        public IEnumerable<CommentReadModel> Replies { get; set; }

        public GalleryOwner GalleryOwner { get; set; }

        public Album Album { get; set; }

        public CommentUser CommentUser { get; set; }

        [BindProperty]
        public Comment Comment { get; set; }

        [BindProperty]
        public Comment Reply { get; set; }

        public CommentReadModel ModifiedComment { get; set; }

        [TempData]
        public string CommentStateSuccessMessage { get; set; }

        [TempData]
        public string CommentStateErrorMessage { get; set; }

        [TempData]

        public int ChosenCommentIndex { get; set; }

        [TempData]

        public int ChosenReplyIndex { get; set; }

        public string SourcePath { get; set; }

        public string SourceImagePath { get; set; }

        public int? SourceAlbumId { get; set; }

        public string SourceGalleryOwnerEmail { get; set; }

        [BindProperty]
        public string CommentsMethod { get; set; }

        [BindProperty]
        public OrderBy CommentsOrderBy { get; set; }

        public string CommentViewId { get; set; }

        public int Counter { get; set; }

        public string PreviousCommentViewId { get; set; }

        public int PreviousLevelCounter { get; set; }

        public GalleryOwner CurrentUser { get; set; }

        public IEnumerable<CommentReadModel> CommentsFromService { get; set; }


        public IEnumerable<CommentReadModel> GetCommentReplies(int commentId)
        {
            var comments = _context.Comments.AsNoTracking().
                Select(x => new CommentReadModel
                {
                    CommentedComment = x.CommentedComment,
                    CommentedCommentId = x.CommentedCommentId,
                    CommentId = x.CommentId,
                    Content = x.Content,
                    WasEdited = x.WasEdited,
                    ControversyScore = x.ControversyScore,
                    DatePosted = x.DatePosted,
                    Dislikes = x.Dislikes,
                    DateUserWasRemoved = x.DateUserWasRemoved,
                    Image = x.Image,
                    Likes = x.Likes,
                    ReactionScore = x.ReactionScore,
                    User = x.User,
                    UserId = x.UserId,
                    ImageId = x.ImageId,
                    TotalWeightFromReactions = x.TotalWeightFromReactions,
                    CommentedCommentUserName = x.CommentedComment.User.UserNameForComments,
                    CommentUser = _context.CommentUsers.Where(a => a.UserId == GetUserId() && a.CommentId == x.CommentId).SingleOrDefault(),
                    NumberOfLikes = ConvertToDifferentFormats.GetCommentLikes(x.Likes),
                    NumberOfDislikes = ConvertToDifferentFormats.GetCommentDislikes(x.Dislikes),
                    TimeDifferenceOfDatePosted = ConvertToDifferentFormats.GetCommentTimeDifferenceOfDatePosted(x.DatePosted),
                    TimeDifferenceOfDateUserWasRemoved = ConvertToDifferentFormats.GetCommentTimeDifferenceOfDatePosted(x.DateUserWasRemoved)

                }).Where(x => x.CommentedCommentId == commentId).OrderByDescending(x => x.DatePosted).AsEnumerable();

            return comments;
        }



        public async Task<IActionResult> OnPostEditComment (int chosenCommentIndex, string content,int commentId, string commentsMethod, OrderBy commentsOrderBy,
            string commentViewId, Guid imageId, string sourcePath, string sourceImagePath,
            string galleryOwnerEmail = null,
            int? albumId = null)
        {
            if (content == null)
            {
                CommentStateErrorMessage = "Comment's content cannot be empty.";
            }
            else
            {
                Comment = _context.Comments.AsNoTracking().Where(x => x.CommentId == commentId).SingleOrDefault();

                Comment.WasEdited = true;
                Comment.Content = content;
                _context.Comments.Update(Comment);
                await _context.SaveChangesAsync();
                CommentStateSuccessMessage = "Comment content was successfully changed.";
            }

            ChosenCommentIndex = chosenCommentIndex;
            return RedirectToPage("/ImageComments", null, new
            {
                imageId,
                sourcePath,
                sourceImagePath,
                galleryOwnerEmail,
                albumId,
                method = commentsMethod,
                orderBy = commentsOrderBy
            }, $"commentEditSettings{commentViewId}");
        }


        public IActionResult OnPostRestoreComment(int chosenCommentIndex, string commentViewId, Guid imageId, string sourcePath, string commentsMethod, OrderBy commentsOrderBy,
            string sourceImagePath, string galleryOwnerEmail = null, int? 
            albumId = null, 
            bool isReply = false)
        {
            if (isReply == true)
            {
                ChosenReplyIndex = chosenCommentIndex;
                CommentStateSuccessMessage = "Comment content was successfully cleared.";
                return RedirectToPage("/ImageComments", null, new
                {
                    imageId = imageId,
                    sourcePath = sourcePath,
                    sourceImagePath = sourceImagePath,
                    galleryOwnerEmail = galleryOwnerEmail,
                    albumId = albumId,
                    method = commentsMethod,
                    orderBy = commentsOrderBy
                }, $"commentReplySettings{commentViewId}");

            }
            else
            {
                ChosenCommentIndex = chosenCommentIndex;
                CommentStateSuccessMessage = "Comment content was successfully restored to original state.";
                return RedirectToPage("/ImageComments", null, new
                {
                    imageId = imageId,
                    sourcePath = sourcePath,
                    sourceImagePath = sourceImagePath,
                    galleryOwnerEmail = galleryOwnerEmail,
                    albumId = albumId,
                    method = commentsMethod,
                    orderBy = commentsOrderBy
                }, $"commentEditSettings{commentViewId}");
            }
        }


        public string GetCommentViewId(int? counter)
        {
            if (counter <= 0 || counter == null)
            {
                return "";
            }
            else
            {
                var stringBuilder = new StringBuilder(counter.ToString());
                if (stringBuilder.Length > 1)
                {
                    for (int c = 0; c < stringBuilder.Length; c++)
                    {
                        switch (stringBuilder[c])
                        {
                            case '0':
                                stringBuilder[c] = 'a';
                                break;
                            case '1':
                                stringBuilder[c] = 'b';
                                break;
                            case '2':
                                stringBuilder[c] = 'c';
                                break;
                            case '3':
                                stringBuilder[c] = 'd';
                                break;
                            case '4':
                                stringBuilder[c] = 'e';
                                break;
                            case '5':
                                stringBuilder[c] = 'f';
                                break;
                            case '6':
                                stringBuilder[c] = 'g';
                                break;
                            case '7':
                                stringBuilder[c] = 'h';
                                break;
                            case '8':
                                stringBuilder[c] = 'i';
                                break;
                            case '9':
                                stringBuilder[c] = 'j';
                                break;
                        }
                    }
                }

                var counterStringRepresentation = stringBuilder.ToString();

                return counterStringRepresentation;
            }
        }

        public async Task<IActionResult> OnGetDeleteComment (int currentCounter, string commentViewId, int commentId, Guid imageId, string commentsMethod, OrderBy commentsOrderBy,
            string sourcePath, string sourceImagePath, 
            string galleryOwnerEmail = null, int? albumId = null)
        {

            if (GetUserId() == default)
            {
                return RedirectToPage("Error");
            }


            ModifiedComment = _context.Comments.AsNoTracking().
                Select(x => new CommentReadModel
                {
                    CommentId = x.CommentId,
                    ControversyScore = x.ControversyScore,
                    Dislikes = x.Dislikes,
                    Likes = x.Likes,
                    ReactionScore = x.ReactionScore,
                    CommentUsers = x.CommentUsers,
                    TotalWeightFromReactions = x.TotalWeightFromReactions,
                    CommentedComment = x.CommentedComment,
                    CommentedCommentId = x.CommentedCommentId,
                    Content = x.Content,
                    DatePosted = x.DatePosted,
                    Image = x.Image,
                    ImageId = x.ImageId,
                    User = x.User,
                    UserId = x.UserId,
                    WasEdited = x.WasEdited,
                    NumberOfCommentsReactingtoThisComment = x.CommentsReactingtoThisComment.Count
                })
               .Where(x => x.CommentId == commentId && x.ImageId == imageId).SingleOrDefault();

            if (ModifiedComment == null)
            {
                return RedirectToPage("Error");
            }

            Image = _context.Images.AsNoTracking().
                 Select(x => new ImageReadModel
                 {
                     AlbumId = x.AlbumId,
                     Album = x.Album,
                     ImageAccessibility = x.ImageAccessibility,
                     UploadedAt = x.UploadedAt,
                     DateTaken = x.DateTaken,
                     ImageContentType = x.ImageContentType,
                     ImageHeight = x.ImageHeight,
                     ImageWidth = x.ImageWidth,
                     OriginalName = x.OriginalName,
                     Size = x.Size,
                     ImageId = x.ImageId,
                     NumberOfComments = x.Comments.Where(x => x.DateUserWasRemoved == null).ToList().Count
                 })
                .Where(x => x.ImageId == imageId).SingleOrDefault();


            if (ModifiedComment.UserId != GetUserId() && Image.Album.GalleryOwnerId != GetUserId())
            {
                return Forbid();
            }

            Comment = _context.Comments.Where(x => x.CommentId == commentId).SingleOrDefault();

            if (ModifiedComment.NumberOfCommentsReactingtoThisComment == 0)
            {
                foreach (var item in ModifiedComment.CommentUsers)
                {
                    _context.CommentUsers.Remove(item);
                }

                _context.Comments.Remove(Comment);

                await _context.SaveChangesAsync();

                if (currentCounter == 1)
                {
                    return RedirectToPage("/ImageComments", new
                    {
                        imageId = imageId,
                        sourcePath = sourcePath,
                        sourceImagePath = sourceImagePath,
                        galleryOwnerEmail = galleryOwnerEmail,
                        albumId = albumId,
                        method = commentsMethod,
                        orderBy = commentsOrderBy
                    });
                }
                else
                {
                    currentCounter--;
                    PreviousCommentViewId = GetCommentViewId(currentCounter);
                    return RedirectToPage("/ImageComments", null, new
                    {
                        imageId = imageId,
                        sourcePath = sourcePath,
                        sourceImagePath = sourceImagePath,
                        galleryOwnerEmail = galleryOwnerEmail,
                        albumId = albumId,
                        method = commentsMethod,
                        orderBy = commentsOrderBy
                    }, $"comment{PreviousCommentViewId}");
                }

            }
            else
            {
                foreach (var item in ModifiedComment.CommentUsers)
                {
                    _context.CommentUsers.Remove(item);
                }

                Comment.Content = null;
                Comment.ControversyScore = 0;
                Comment.DateUserWasRemoved = DateTime.Now;
                Comment.Dislikes = 0;
                Comment.Likes = 0;
                Comment.ReactionScore = 0;
                Comment.TotalWeightFromReactions = 0;
                Comment.UserId = null;

                _context.Comments.Update(Comment);

                await _context.SaveChangesAsync();

                return RedirectToPage("/ImageComments", null, new
                {
                    imageId = imageId,
                    sourcePath = sourcePath,
                    sourceImagePath = sourceImagePath,
                    galleryOwnerEmail = galleryOwnerEmail,
                    albumId = albumId,
                    method = commentsMethod,
                    orderBy = commentsOrderBy
                }, $"comment{commentViewId}");
             
            }
        }

        public async Task<IActionResult> OnGetUnlikeComment(string commentViewId,int commentId, Guid imageId, string sourcePath, string commentsMethod, OrderBy commentsOrderBy,
            string sourceImagePath, string galleryOwnerEmail = null, int? albumId = null)
        {
            if (GetUserId() == default)
            {
                return RedirectToPage("Error");
            }

            Comment = _context.Comments.
                Select(x => new Comment
                {
                    CommentId = x.CommentId,
                    ControversyScore = x.ControversyScore,
                    Dislikes = x.Dislikes,
                    Likes = x.Likes,
                    ReactionScore = x.ReactionScore,
                    CommentUsers = x.CommentUsers,
                    TotalWeightFromReactions = x.TotalWeightFromReactions,
                    CommentedComment = x.CommentedComment,
                    CommentedCommentId = x.CommentedCommentId,
                    Content = x.Content,
                    DatePosted = x.DatePosted,
                    Image = x.Image,
                    ImageId = x.ImageId,
                    User = x.User,
                    UserId = x.UserId,
                    WasEdited = x.WasEdited
                })
                .Where(x => x.CommentId == commentId && x.ImageId == imageId).SingleOrDefault();

            if (Comment == null)
            {
                return RedirectToPage("Error");
            }

            CommentUser = Comment.CommentUsers.Where(x => x.CommentId == commentId && x.UserId == GetUserId() && x.ReactionType == CommentUser.Reaction.Dislike).SingleOrDefault();

            if (CommentUser != null)
            {
                return RedirectToPage("Error");
            }

            CommentUser = Comment.CommentUsers.Where(x => x.CommentId == commentId && x.UserId == GetUserId() && x.ReactionType == CommentUser.Reaction.Like).SingleOrDefault();

            if (CommentUser != null)
            {
                Comment.Likes--;
                _context.Comments.Update(Comment);
                Comment.ReactionScore = GetReactionScore(Comment.Likes, Comment.Dislikes);
                Comment.TotalWeightFromReactions = GetTotalWeightFromReactions(Comment.Likes, Comment.Dislikes);
                Comment.ControversyScore = GetControversyScore(Comment.Likes, Comment.Dislikes);
                _context.Comments.Update(Comment);

                _context.CommentUsers.Remove(CommentUser);

                await _context.SaveChangesAsync();
            }
            else
            {
                return RedirectToPage("Error");
            }


            return RedirectToPage("/ImageComments", null, new
            {
                imageId = imageId,
                sourcePath = sourcePath,
                sourceImagePath = sourceImagePath,
                galleryOwnerEmail = galleryOwnerEmail,
                albumId = albumId,
                method = commentsMethod,
                orderBy = commentsOrderBy
            }, $"comment{commentViewId}");
        }





        public async Task<IActionResult> OnGetRemoveDislikeForComment(string commentViewId, int commentId, Guid imageId, string commentsMethod, OrderBy commentsOrderBy,
            string sourcePath, string sourceImagePath, string galleryOwnerEmail = null, int? albumId = null)
        {
            if (GetUserId() == default)
            {
                return RedirectToPage("Error");
            }

            Comment = _context.Comments.
                Select(x => new Comment
                {
                    CommentId = x.CommentId,
                    ControversyScore = x.ControversyScore,
                    Dislikes = x.Dislikes,
                    Likes = x.Likes,
                    ReactionScore = x.ReactionScore,
                    CommentUsers = x.CommentUsers,
                    TotalWeightFromReactions = x.TotalWeightFromReactions,
                    CommentedComment = x.CommentedComment,
                    CommentedCommentId = x.CommentedCommentId,
                    Content = x.Content,
                    DatePosted = x.DatePosted,
                    Image = x.Image,
                    ImageId = x.ImageId,
                    User = x.User,
                    UserId = x.UserId,
                    WasEdited = x.WasEdited
                })
                .Where(x => x.CommentId == commentId && x.ImageId == imageId).SingleOrDefault();

            if (Comment == null)
            {
                return RedirectToPage("Error");
            }

            CommentUser = Comment.CommentUsers.Where(x => x.CommentId == commentId && x.UserId == GetUserId() && x.ReactionType == CommentUser.Reaction.Like).SingleOrDefault();

            if (CommentUser != null)
            {
                return RedirectToPage("Error");
            }

            CommentUser = Comment.CommentUsers.Where(x => x.CommentId == commentId && x.UserId == GetUserId() && x.ReactionType == CommentUser.Reaction.Dislike).SingleOrDefault();

            if (CommentUser != null)
            {
                Comment.Dislikes--;
                _context.Comments.Update(Comment);
                Comment.ReactionScore = GetReactionScore(Comment.Likes, Comment.Dislikes);
                Comment.TotalWeightFromReactions = GetTotalWeightFromReactions(Comment.Likes, Comment.Dislikes);
                Comment.ControversyScore = GetControversyScore(Comment.Likes, Comment.Dislikes);
                _context.Comments.Update(Comment);

                _context.CommentUsers.Remove(CommentUser);

                await _context.SaveChangesAsync();
            }
            else
            {
                return RedirectToPage("Error");
            }

            return RedirectToPage("/ImageComments", null, new
            {
                imageId = imageId,
                sourcePath = sourcePath,
                sourceImagePath = sourceImagePath,
                galleryOwnerEmail = galleryOwnerEmail,
                albumId = albumId,
                method = commentsMethod,
                orderBy = commentsOrderBy
            }, $"comment{commentViewId}");
        }


        public async Task<IActionResult> OnGetLikeComment(string commentViewId, int commentId, Guid imageId, string sourcePath, string commentsMethod, OrderBy commentsOrderBy,
            string sourceImagePath, string galleryOwnerEmail = null, int? albumId = null)
        {
            if (GetUserId() == default)
            {
                return RedirectToPage("Error");
            }

            Comment = _context.Comments.
                Select(x => new Comment
                {
                    CommentId = x.CommentId,
                    ControversyScore = x.ControversyScore,
                    Dislikes = x.Dislikes,
                    Likes = x.Likes,
                    ReactionScore = x.ReactionScore,
                    CommentUsers = x.CommentUsers,
                    TotalWeightFromReactions = x.TotalWeightFromReactions,
                    CommentedComment = x.CommentedComment,
                    CommentedCommentId = x.CommentedCommentId,
                    Content = x.Content,
                    DatePosted = x.DatePosted,
                    Image = x.Image,
                    ImageId = x.ImageId,
                    User = x.User,
                    UserId = x.UserId,
                    WasEdited = x.WasEdited
                })
                .Where(x => x.CommentId == commentId && x.ImageId == imageId).SingleOrDefault();

            if (Comment == null)
            {
                return RedirectToPage("Error");
            }

            CommentUser = Comment.CommentUsers.Where(x => x.CommentId == commentId && x.UserId == GetUserId() && x.ReactionType == CommentUser.Reaction.Like).SingleOrDefault();

            if (CommentUser != null)
            {
                return RedirectToPage("Error");
            }

            CommentUser = Comment.CommentUsers.Where(x => x.CommentId == commentId && x.UserId == GetUserId() && x.ReactionType == CommentUser.Reaction.Dislike).SingleOrDefault();

            if (CommentUser != null)
            {
                Comment.Dislikes--;
                Comment.Likes++;
                _context.Comments.Update(Comment);
                Comment.ReactionScore = GetReactionScore(Comment.Likes, Comment.Dislikes);
                Comment.TotalWeightFromReactions = GetTotalWeightFromReactions(Comment.Likes, Comment.Dislikes);
                Comment.ControversyScore = GetControversyScore(Comment.Likes, Comment.Dislikes);
                _context.Comments.Update(Comment);


                CommentUser.ReactionType = CommentUser.Reaction.Like;
                _context.CommentUsers.Update(CommentUser);

                await _context.SaveChangesAsync();
            }
            else
            {
                Comment.Likes++;
                _context.Comments.Update(Comment);
                Comment.ReactionScore = GetReactionScore(Comment.Likes, Comment.Dislikes);
                Comment.TotalWeightFromReactions = GetTotalWeightFromReactions(Comment.Likes, Comment.Dislikes);
                Comment.ControversyScore = GetControversyScore(Comment.Likes, Comment.Dislikes);
                _context.Comments.Update(Comment);


                CommentUser = new CommentUser { CommentId = commentId, UserId = GetUserId(), ReactionType = CommentUser.Reaction.Like };
                _context.CommentUsers.Add(CommentUser);

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/ImageComments", null, new
            {
                imageId = imageId,
                sourcePath = sourcePath,
                sourceImagePath = sourceImagePath,
                galleryOwnerEmail = galleryOwnerEmail,
                albumId = albumId,
                method = commentsMethod,
                orderBy = commentsOrderBy
            }, $"comment{commentViewId}");
        }


        public async Task<IActionResult> OnGetDislikeComment(string commentViewId, int commentId, Guid imageId, string commentsMethod, OrderBy commentsOrderBy,
            string sourcePath, string sourceImagePath, string galleryOwnerEmail = null, int? albumId = null)
        {
            if (GetUserId() == default)
            {
                return RedirectToPage("Error");
            }

            Comment = _context.Comments.
                Select(x => new Comment
                {
                    CommentId = x.CommentId,
                    ControversyScore = x.ControversyScore,
                    Dislikes = x.Dislikes,
                    Likes = x.Likes,
                    ReactionScore = x.ReactionScore,
                    CommentUsers = x.CommentUsers,
                    TotalWeightFromReactions = x.TotalWeightFromReactions,
                    CommentedComment = x.CommentedComment,
                    CommentedCommentId = x.CommentedCommentId,
                    Content = x.Content,
                    DatePosted = x.DatePosted,
                    Image = x.Image,
                    ImageId = x.ImageId,
                    User = x.User,
                    UserId = x.UserId,
                    WasEdited = x.WasEdited
                })
                .Where(x => x.CommentId == commentId && x.ImageId == imageId).SingleOrDefault();

            if (Comment == null)
            {
                return RedirectToPage("Error");
            }

            CommentUser = Comment.CommentUsers.Where(x => x.CommentId == commentId && x.UserId == GetUserId() && x.ReactionType == CommentUser.Reaction.Dislike).SingleOrDefault();

            if (CommentUser != null)
            {
                return RedirectToPage("Error");
            }

            CommentUser = Comment.CommentUsers.Where(x => x.CommentId == commentId && x.UserId == GetUserId() && x.ReactionType == CommentUser.Reaction.Like).SingleOrDefault();

            if (CommentUser != null)
            {
                Comment.Likes--;
                Comment.Dislikes++;
                _context.Comments.Update(Comment);
                Comment.ReactionScore = GetReactionScore(Comment.Likes, Comment.Dislikes);
                Comment.TotalWeightFromReactions = GetTotalWeightFromReactions(Comment.Likes, Comment.Dislikes);
                Comment.ControversyScore = GetControversyScore(Comment.Likes, Comment.Dislikes);
                _context.Comments.Update(Comment);


                CommentUser.ReactionType = CommentUser.Reaction.Dislike;
                _context.CommentUsers.Update(CommentUser);

                await _context.SaveChangesAsync();
            }
            else
            {
                Comment.Dislikes++;
                _context.Comments.Update(Comment);
                Comment.ReactionScore = GetReactionScore(Comment.Likes, Comment.Dislikes);
                Comment.TotalWeightFromReactions = GetTotalWeightFromReactions(Comment.Likes, Comment.Dislikes);
                Comment.ControversyScore = GetControversyScore(Comment.Likes, Comment.Dislikes);
                _context.Comments.Update(Comment);


                CommentUser = new CommentUser { CommentId = commentId, UserId = GetUserId(), ReactionType = CommentUser.Reaction.Dislike };
                _context.CommentUsers.Add(CommentUser);

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/ImageComments", null, new
            {
                imageId = imageId,
                sourcePath = sourcePath,
                sourceImagePath = sourceImagePath,
                galleryOwnerEmail = galleryOwnerEmail,
                albumId = albumId,
                method = commentsMethod,
                orderBy = commentsOrderBy
            }, $"comment{commentViewId}");
        }

        private static decimal GetReactionScore (long likes, long dislikes)
        {
            var numberOfReactions = likes + dislikes;
            var onePercentValue = (decimal)numberOfReactions / 100;
            
            if (onePercentValue == 0)
            {
                return 0;
            }
            return likes / onePercentValue;
        }

        private static double GetTotalWeightFromReactions(long likes, long dislikes)
        {
            var numberOfReactions = likes + dislikes;

            return (double)numberOfReactions / 100;
        }

        private static decimal GetControversyScore(long likes, long dislikes)
        {
            if (likes > dislikes)
            {
                return (decimal)dislikes / likes;
            }
            else
            {
                if (likes == 0 || dislikes == 0)
                {
                    return 0;
                }

                return (decimal)likes / dislikes;
            }
        }

        public async Task<IActionResult> OnPostCreateComment (string userId, Guid imageId, string parentCommentViewId, string commentsMethod, OrderBy commentsOrderBy,
            int currentCounter, int parentCommentId, string sourcePath, string sourceImagePath, 
            string galleryOwnerEmail = null, int? albumId = null, 
            bool isReply = false)

        {
            if (isReply == true)
            {
                if (Reply.Content == null)
                {
                    ChosenReplyIndex = currentCounter;
                    CommentStateErrorMessage = "Reply content cannot be empty";
                    return RedirectToPage("/ImageComments", null, new
                    {
                        imageId = imageId,
                        sourcePath = sourcePath,
                        sourceImagePath = sourceImagePath,
                        galleryOwnerEmail = galleryOwnerEmail,
                        albumId = albumId,
                        method = commentsMethod,
                        orderBy = commentsOrderBy
                    }, $"commentReplySettings{parentCommentViewId}");
                }

                currentCounter++;
                CommentViewId = GetCommentViewId(currentCounter);
                Reply.UserId = userId;
                Reply.ImageId = imageId;
                Reply.DatePosted = DateTime.Now;
                Reply.CommentedCommentId = parentCommentId;

                _context.Comments.Add(Reply);
                await _context.SaveChangesAsync();

                return RedirectToPage("/ImageComments", null, new
                {
                    imageId = imageId,
                    sourcePath = sourcePath,
                    sourceImagePath = sourceImagePath,
                    galleryOwnerEmail = galleryOwnerEmail,
                    albumId = albumId,
                    method = commentsMethod,
                    orderBy = commentsOrderBy
                }, $"comment{CommentViewId}");
            }
            else
            {
                if (Comment.Content == null)
                {
                    ErrorMessage = "Comment content cannot be empty";

                }
                else
                {
                    Comment.UserId = userId;
                    Comment.ImageId = imageId;
                    Comment.DatePosted = DateTime.Now;
                    SuccessMessage = "Comment was successfully added.";
                    _context.Comments.Add(Comment);
                    await _context.SaveChangesAsync();
                }

                return RedirectToPage("/ImageComments", new { imageId = imageId, sourcePath = sourcePath, sourceImagePath = sourceImagePath, galleryOwnerEmail = galleryOwnerEmail, albumId = albumId });
            }
            
        }

        public IActionResult OnGetProfileImageThumbnail(string userEmail, ProfileImageThumbnailType type)
        {
            GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Email == userEmail).SingleOrDefault();
            if (GalleryOwner == null)
            {
                return RedirectToPage("Error");
            }

            if (type == ProfileImageThumbnailType.BigRounded)
            {
                return File(GalleryOwner.ThumbnailForStandardCommentBlob, "image/" + GalleryOwner.ThumbnailContentType.ToLower());
            }
            else if (type == ProfileImageThumbnailType.SmallRounded)
            {
                return File(GalleryOwner.ThumbnailForCommentedCommentBlob, "image/" + GalleryOwner.ThumbnailContentType.ToLower());
            }

            return RedirectToPage("Error");
        }

        public string GetUserId()
        {
            return HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? default;
        }

        public IActionResult OnPostCommentsSettings (Guid imageId, string sourcePath, string sourceImagePath, string galleryOwnerEmail, int? albumId)
        {
            return RedirectToPage("/ImageComments", new { imageId = imageId, method = CommentsMethod, orderBy = CommentsOrderBy,
                sourcePath = sourcePath, sourceImagePath = sourceImagePath, galleryOwnerEmail = galleryOwnerEmail, albumId = albumId });
        }


        public async Task<IActionResult> OnGet(Guid imageId, string method, OrderBy orderBy, string sourcePath, string sourceImagePath, string galleryOwnerEmail = null, int? albumId = null)
        {

            Image = _context.Images.AsNoTracking().
                 Select(x => new ImageReadModel
                 {
                     AlbumId = x.AlbumId,
                     Album = x.Album,
                     ImageAccessibility = x.ImageAccessibility,
                     UploadedAt = x.UploadedAt,
                     DateTaken = x.DateTaken,
                     ImageContentType = x.ImageContentType,
                     ImageHeight = x.ImageHeight,
                     ImageWidth = x.ImageWidth,
                     OriginalName = x.OriginalName,
                     Size = x.Size,
                     ImageId = x.ImageId,
                     NumberOfComments = x.Comments.Where(x => x.DateUserWasRemoved == null).ToList().Count
                 })
                .Where(x => x.ImageId == imageId).SingleOrDefault();

            if (Image == null)
            {
                return NotFound("Image not found.");
            }
            else if (Image.ImageAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

            CommentsFromService = _helper.GetCommentsForCurrentImage(imageId, method, orderBy, out string Method, out OrderBy OrderBy, GetUserId());

            CommentsMethod = Method;
            CommentsOrderBy = OrderBy;

            CurrentUser = await _userManager.GetUserAsync(User);
            CommentsMethodOfSortingList = _dropdownList.GetMethodOfSortingForComments();
            CommentsOrderByList = _dropdownList.GetOrderByDropdown();
            SourceGalleryOwnerEmail = galleryOwnerEmail;
            SourceAlbumId = albumId;
            SourceImagePath = sourceImagePath;
            SourcePath = sourcePath;

            return Page();
        }

        public IActionResult OnGetCancelComment (Guid imageId, string sourcePath, string sourceImagePath, string galleryOwnerEmail, string commentsMethod, OrderBy commentsOrderBy, int? albumId)
        {
            var Image = _context.Images.AsNoTracking().Where(x => x.ImageId == imageId).SingleOrDefault();

            if (Image == null)
            {
                return RedirectToPage("Error");
            }
            else if (Image.ImageAccessibility == Accessibility.Private || GetUserId() == null)
            {
                return Forbid();
            }



            return RedirectToPage("/ImageComments", new { imageId = imageId, sourcePath = sourcePath,
                sourceImagePath = sourceImagePath, galleryOwnerEmail = galleryOwnerEmail, albumId = albumId, method = commentsMethod, orderBy = commentsOrderBy });
        }


        public IActionResult OnGetReturnBack(string sourcePath, string sourceImagePath, Guid imageId, string galleryOwnerEmail = null, int? albumId = null)
        {
            var Image = _context.Images.AsNoTracking().Where(x => x.ImageId == imageId).SingleOrDefault();

            if (Image == null)
            {
                return RedirectToPage("Error");
            }
            else if (Image.ImageAccessibility == Accessibility.Private)
            {
                return Forbid();
            }

            switch (sourcePath)
            {
                case "/":
                    if (sourceImagePath.StartsWith("publicImageDetails") == false)
                    {
                        return RedirectToPage("Error");
                    }
                    else
                    {
                        SourcePath = "/Index";
                        SourceImagePath = $"{sourceImagePath + Image.ImageId}";
                    }
                    break;
                case "/MyAlbum":
                    if (sourceImagePath.StartsWith("chosenAlbumSettings") == false)
                    {
                        return RedirectToPage("Error");

                    }
                    else
                    {
                        Album = _context.Albums.AsNoTracking().Where(x => x.AlbumId == albumId).SingleOrDefault();
                        if (Album == null)
                        {
                            return RedirectToPage("Error");
                        }

                        var image = _context.Images.AsNoTracking().Where(x => x.ImageId == Image.ImageId && x.AlbumId == Album.AlbumId).SingleOrDefault();
                        if (image == null)
                        {
                            return RedirectToPage("Error");
                        }
                        SourcePath = sourcePath;
                        SourceImagePath = $"{sourceImagePath + Image.ImageId}";
                        SourceAlbumId = albumId;
                    }
                    break;
                case "/MyGallery":
                    if (sourceImagePath.StartsWith("galleryImageDetails") == false)
                    {
                        return RedirectToPage("Error");

                    }
                    else
                    {
                        SourcePath = sourcePath;
                        SourceImagePath = $"{sourceImagePath + Image.ImageId}";
                    }
                    break;
                case "/ReadOnlyGallery":
                    if (sourceImagePath.StartsWith("publicGalleryImageDetails") == false)
                    {
                        return RedirectToPage("Error");

                    }
                    else
                    {
                        GalleryOwner = _userManager.Users.AsNoTracking().Where(x => x.Email == galleryOwnerEmail).SingleOrDefault();
                        if (GalleryOwner == null)
                        {
                            return RedirectToPage("Error");
                        }

                        var image = _context.Images.AsNoTracking().Where(x => x.ImageId == Image.ImageId && x.Album.GalleryOwnerId == GalleryOwner.Id).SingleOrDefault();
                        if (image == null)
                        {
                            return RedirectToPage("Error");
                        }
                        SourcePath = sourcePath;
                        SourceImagePath = $"{sourceImagePath + Image.ImageId}";
                        SourceGalleryOwnerEmail = galleryOwnerEmail;
                    }
                    break;
                case "/ReadOnlyAlbum":
                    if (sourceImagePath.StartsWith("chosenPublicAlbumSettings") == false)
                    {
                        return RedirectToPage("Error");

                    }
                    else
                    {
                        Album = _context.Albums.AsNoTracking().Where(x => x.AlbumId == albumId).SingleOrDefault();
                        if (Album == null)
                        {
                            return RedirectToPage("Error");
                        }

                        var image = _context.Images.AsNoTracking().Where(x => x.ImageId == Image.ImageId && x.AlbumId == Album.AlbumId).SingleOrDefault();
                        if (image == null)
                        {
                            return RedirectToPage("Error");
                        }

                        SourcePath = sourcePath;
                        SourceImagePath = $"{sourceImagePath + Image.ImageId}";
                        SourceAlbumId = albumId;
                    }
                    break;
                default:
                    return RedirectToPage("Error");
            }

            if (SourceAlbumId != null && SourceGalleryOwnerEmail == null)
            {
                return RedirectToPage(SourcePath, null, new { albumId = SourceAlbumId }, SourceImagePath);

            }
            else if (SourceAlbumId == null && SourceGalleryOwnerEmail != null)
            {
                return RedirectToPage(SourcePath, null, new { galleryOwnerEmail = SourceGalleryOwnerEmail }, SourceImagePath);
            }
            else
            {
                return RedirectToPage(SourcePath, null, null, SourceImagePath);
            }
        }

        public IActionResult OnGetMoveToChosenComment(string commentViewId, Guid imageId, string sourcePath, string sourceImagePath, string commentsMethod, OrderBy commentsOrderBy,
            string galleryOwnerEmail = null, 
            int? albumId = null)
        {
            var Image = _context.Images.AsNoTracking().Where(x => x.ImageId == imageId).SingleOrDefault();

            if (Image == null)
            {
                return RedirectToPage("Error");
            }
            else if (Image.ImageAccessibility == Accessibility.Private)
            {
                return Forbid();
            }


            return RedirectToPage("/ImageComments", null, new
            {
                imageId = imageId,
                sourcePath = sourcePath,
                sourceImagePath = sourceImagePath,
                galleryOwnerEmail = galleryOwnerEmail,
                albumId = albumId,
                method = commentsMethod,
                orderBy = commentsOrderBy
            }, $"comment{commentViewId}");
        }

    }
}
