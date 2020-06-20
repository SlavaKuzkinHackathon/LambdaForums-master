using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaForums.Models.Post
{
    public class NewPostModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ForumName { get; set; }
        public int ForumId { get; set; }
        public string AuthorName { get; set; }
        public string ForumImageUrl { get; set; }
    }
}
