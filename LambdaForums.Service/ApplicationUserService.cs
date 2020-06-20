using LambdaForums.Data;
using LambdaForums.Data.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaForums.Service
{
    public class ApplicationUserService : IApplicationUser
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ApplicationUserService(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IEnumerable<ApplicationUser> GetAll()
        {
            var users = _context.Users.ToList();

            return users;

        }

        public ApplicationUser GetById(string id)
        {
            var result = _userManager.Users;
            var model = result.FirstOrDefault(
                user => user.Id == id);
            return model;
        }

        public async Task UpdateUserRating(string userId, Type type)
        {
            //changes change from one rating to the next, i
            //specific implementation of rating is in the method below
            var user = GetById(userId);
            user.Rating = CalcualteUserRating(type, user.Rating);
            await _context.SaveChangesAsync();
        }

        private int CalcualteUserRating(Type type, int userRating)
        {
            var inc = 0;
            if (type == typeof(Post))
            {
                inc = 1;
            }
            else if(type == typeof(PostReply))
            {
                inc = 3;
            }

            return userRating + inc;
        }

        public async Task SetProfileImage(string id, Uri uri)
        {
            var user = GetById(id);
            user.ProfileImageUrl = uri.AbsoluteUri;
            await _userManager.UpdateAsync(user);
        }
    }
}
