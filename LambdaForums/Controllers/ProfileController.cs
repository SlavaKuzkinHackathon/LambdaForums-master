using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LambdaForums.Data;
using LambdaForums.Data.Models;
using LambdaForums.Models.ApplicationUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace LambdaForums.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUser _userService;
        private readonly IUpload _uploadService;
        private readonly IConfiguration _configuration;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IApplicationUser userService,
            IUpload uploadService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _userService = userService;
            _uploadService = uploadService;
            _configuration = configuration;
        }

        public IActionResult Detail(string id)
        {
            
            var user = _userService.GetById(id);
            var userRoles = _userManager.GetRolesAsync(user).Result;

            var model = new ProfileModel()
            {
                UserId = user.Id,
                UserName = user.UserName, 
                UserRating = user.Rating.ToString(),
                Email = user.Email, 
                ProfileImageUrl = user.ProfileImageUrl, 
                MemberSince = user.MemberSince, 
                IsAdmin = userRoles.Contains("Admin")
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfileImage (IFormFile file)
        {
            var userId = _userManager.GetUserId(User);

            //Connect to an Azure Storage Account Container
            var connectionString = _configuration.GetConnectionString("AzureStorageAccount");

            //Get Blob Container
            var container = _uploadService.GetBlobContainer(connectionString, "profile-images");

            //Parse the Content Disposition response header
            var contentDisposition = ContentDispositionHeaderValue.Parse(file.ContentDisposition);

            //Grab the filename
            var fileName = contentDisposition.FileName.Trim('"');
            //Grab a reference to a Block Blob
            var blockBlob = container.GetBlockBlobReference(fileName);

            //On that block blob, Upload our file <-- file uploaded to the cloud
            await blockBlob.UploadFromStreamAsync(file.OpenReadStream());
            //Set the User's profile image to the URI
            await _userService.SetProfileImage(userId, blockBlob.Uri);
            //Redirect to the user's profile page
            return RedirectToAction("Detail", "Profile", new { id = userId });
        }

        [Authorize (Roles = "Admin")]
        public IActionResult Index ()
        {
            //var profiles = new List<ProfileModel>();

            //foreach (var user in _userManager.Users.ToList())
            //{
            //    profiles.Add(new ProfileModel()
            //    {
            //        Email = user.Email,
            //        UserName = user.UserName,
            //        ProfileImageUrl = user.ProfileImageUrl,
            //        UserRating = user.Rating.ToString(),
            //        MemberSince = user.MemberSince
            //    });
            //}


            var profiles = _userService.GetAll()
                .OrderByDescending(user => user.Rating)
                .Select(u => new ProfileModel
                {
                    Email = u.Email,
                    UserName = u.UserName,
                    ProfileImageUrl = u.ProfileImageUrl,
                    UserRating = u.Rating.ToString(),
                    MemberSince = u.MemberSince
                });

            var model = new ProfileListModel
            {
                Profiles = profiles
            };

            return View(model);
        }
    }
}
