using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryDatabase.Helpers
{
    public static class ConvertToDifferentFormats
    {
        public static string GetCommentLikes(long numberOfLikes)
        {
            switch (numberOfLikes)
            {
                case >= 1000000000:
                    return $"{numberOfLikes / 1000000000}B";
                case >= 1000000:
                    return $"{numberOfLikes / 1000000}M";
                case >= 1000:
                    return $"{numberOfLikes / 1000}K";
                case 0:
                    return null;
                default:
                    return $"{numberOfLikes}";
            }
        }

        public static string GetCommentDislikes(long numberOfDislikes)
        {
            switch (numberOfDislikes)
            {
                case >= 1000000000:
                    return $"{numberOfDislikes / 1000000000}B";
                case >= 1000000:
                    return $"{numberOfDislikes / 1000000}M";
                case >= 1000:
                    return $"{numberOfDislikes / 1000}K";
                case 0:
                    return null;
                default:
                    return $"{numberOfDislikes}";
            }
        }

        public static string GetCommentTimeDifferenceOfDatePosted(DateTime? commentDatePosted)
        {
            if (commentDatePosted == null)
            {
                return null;
            }
            else
            {
                DateTime _commentDatePosted = (DateTime)commentDatePosted;
                var CommentDatePostedToNowDifference = DateTime.Now.Subtract(_commentDatePosted);

                int monthsApart = 12 * (_commentDatePosted.Year - DateTime.Now.Year) + _commentDatePosted.Month - DateTime.Now.Month;

                int years = (int)CommentDatePostedToNowDifference.TotalDays / 365;

                int weeks = (int)CommentDatePostedToNowDifference.TotalDays / 7;

                int seconds = (int)CommentDatePostedToNowDifference.TotalSeconds;

                int minutes = (int)CommentDatePostedToNowDifference.TotalMinutes;

                int hours = (int)CommentDatePostedToNowDifference.TotalHours;

                int days = (int)CommentDatePostedToNowDifference.TotalDays;

                if (seconds < 60)
                {
                    if (seconds == 1)
                    {
                        return $"{seconds} second ago";
                    }
                    else if (seconds < 1)
                    {
                        return "just now";
                    }

                    return $"{seconds} seconds ago";
                }
                else if (minutes < 60)
                {
                    if (minutes == 1)
                    {
                        return $"{minutes} minute ago";
                    }
                    return $"{minutes} minutes ago";
                }
                else if (hours < 24)
                {
                    if (hours == 1)
                    {
                        return $"{hours} hour ago";
                    }
                    return $"{hours} hours ago";
                }
                else if (days < 7)
                {
                    if (days == 1)
                    {
                        return $"{days} day ago";
                    }

                    return $"{days} days ago";
                }
                else if (weeks < 4)
                {
                    if (weeks == 1)
                    {
                        return $"{weeks} week ago";
                    }

                    return $"{weeks} weeks ago";

                }
                else if (monthsApart < 12)
                {
                    if (monthsApart == 1)
                    {
                        return $"{monthsApart} month ago";
                    }
                    return $"{monthsApart} months ago";
                }
                else
                {
                    if (years == 1)
                    {
                        return $"{years} year ago";
                    }
                    return $"{years} years ago";
                }
            }
        }
    }
}
