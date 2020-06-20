using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaForums.Models.Forum
{
    public class ForumIndexModel
    {
        //this we will pass to view
        public IEnumerable<ForumListingModel> ForumList { get; set; }
    }
}
