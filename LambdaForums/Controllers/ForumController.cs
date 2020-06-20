using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LambdaForums.Data;
using LambdaForums.Data.Models;
using LambdaForums.Models.Forum;
using LambdaForums.Models.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;

namespace LambdaForums.Controllers
{
    public class ForumController : Controller
    {
        private readonly IForum _forumService;
        private readonly IUpload _uploadService;
        private readonly IConfiguration _configuration;
        private readonly IPost _postService;
        public ForumController(
            IForum forumService, 
            IPost postService,
            IUpload uploadService,
            IConfiguration configuration)
        {
            _forumService = forumService;
            _uploadService = uploadService;
            _configuration = configuration;
            _postService = postService;
        }
        

        public IActionResult Index() 
        {
            var forums = _forumService.GetAll()
                .Select(forum => new ForumListingModel { 
                    Id = forum.Id,
                    Name = forum.Title,
                    Description = forum.Description,
                    //if form.Posts is not null, take that as a value(including .Count)
                    //if it's null, take 0 as value
                    NumberOfPosts = forum.Posts?.Count() ?? 0,
                    NumberOfUsers = _forumService.GetActiveUsers(forum.Id).Count(),
                    ImageUrl = forum.ImageUrl,
                    HasRecentPost = _forumService.HasRecentPost(forum.Id)
                });


            var model = new ForumIndexModel
            {
                ForumList = forums.OrderBy(f => f.Name)
            };

            return View(model);
        }

        public IActionResult Topic (int id, string searchQuery)
        {
            
            var forum = _forumService.GetById(id);
            var posts = new List<Post>();

            posts = _postService.GetFilteredPosts(forum, searchQuery).ToList();


            var postListings = posts.Select(post => new PostListingModel
            {
                Id = post.Id,
                AuthorId = post.User.Id,
                AuthorName = post.User.UserName,
                AuthorRating = post.User.Rating,
                Title = post.Title,
                DatePosted = post.Created.ToString(),
                RepliesCount = post.Replies.Count(),
                Forum = BuildForumListing(post)
            });

            var model = new ForumTopicModel
            {
                Posts = postListings,
                Forum = BuildForumListing(forum)
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Search (int id, string searchQuery)
        {
            return RedirectToAction("Topic", new { id, searchQuery });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create (int? forumId)
        {
            if (forumId.HasValue)
            {
                var forum = _forumService.GetById((int)forumId);

                var model = new AddForumModel
                {
                    ForumId = forum.Id,
                    Title = forum.Title, 
                    Description = forum.Description, 
                    ImageUrl = forum.ImageUrl
                };

                return View(model);
            }
            else
            {
                var model = new AddForumModel();
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<IActionResult> AddForum(AddForumModel model)
        {

            //set default image for the new forums
            var imageUri = "/images/users/default.png";

            if (model.ImageUpload != null)
            {
                var blockBlob = UploadForumImage(model.ImageUpload);
                imageUri = blockBlob.Uri.AbsoluteUri;
            }

            var forum = new Forum
            {
                Id = model.ForumId,
                Title = model.Title,
                Description = model.Description,
                Created = DateTime.Now,
                ImageUrl = imageUri
            };

            if (forum.Id > 0)
            {
                await _forumService.UpdateForum(forum);
            }
            else
            {
                await _forumService.Create(forum);
            }

            return RedirectToAction("Index", "Forum");
        }

        public async Task<IActionResult> Delete (int forumId)
        {
            await DeleteForumPosts(forumId);
            await _forumService.Delete(forumId);
            return RedirectToAction("Index", "Forum");
        }

        private async Task<Forum> DeleteForumPosts (int forumId)
        {
            var forum = _forumService.GetById(forumId);
            await _postService.DeleteForumPosts(forum);
            return forum;
        }


        private CloudBlockBlob UploadForumImage(IFormFile file)
        {
            var connectionString = _configuration.GetConnectionString("AzureStorageAccount");
            var container = _uploadService.GetBlobContainer(connectionString, "forum-images");
            var contentDisposition = ContentDispositionHeaderValue.Parse(file.ContentDisposition);
            var fileName = contentDisposition.FileName.Trim('"');
            var blockBlob = container.GetBlockBlobReference(fileName);
            blockBlob.UploadFromStreamAsync(file.OpenReadStream()).Wait();

            return blockBlob;
        }

        //below does not have any purpose really
        private ForumListingModel BuildForumListing(Post post)
        {
            var forum = post.Forum;

            return BuildForumListing(forum);
        }

        private ForumListingModel BuildForumListing(Forum forum)
        {
            return new ForumListingModel
            {
                Id = forum.Id,
                Name = forum.Title,
                Description = forum.Description,
                ImageUrl = forum.ImageUrl
            };
        }
    }
}







