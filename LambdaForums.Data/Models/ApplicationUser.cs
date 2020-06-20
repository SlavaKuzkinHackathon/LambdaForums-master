using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaForums.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        //along with properties inherited, we add following
        public int Rating { get; set; } = 0;
        public string ProfileImageUrl { get; set; }
        public DateTime MemberSince { get; set; }
        public bool IsActive { get; set; }
    }
}
