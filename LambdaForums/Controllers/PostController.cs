using LambdaForums.Data;
using LambdaForums.Data.Models;
using LambdaForums.Models.Post;
using LambdaForums.Models.Reply;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LambdaForums.Controllers
{
    public class PostController : Controller
    {
        private readonly IPost _postService;
        private readonly IForum _forumService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUser _userService;

        public PostController(
            IPost postService, 
            IForum forumService, 
            UserManager<ApplicationUser> userManager,
            IApplicationUser userService)
        {
            _postService = postService;
            _forumService = forumService;
            _userManager = userManager;
            _userService = userService;
        }

        // GET: /<controller>/
        public IActionResult Index(int id)
        {
            var post = _postService.GetById(id);

            var replies = BuildPostReplies(post.Replies);

            var model = new PostIndexModel
            {
                Id = post.Id,
                Title = post.Title,
                AuthorId = post.User.Id,
                AuthorName = post.User.UserName,
                AuthorImageUrl = post.User.ProfileImageUrl,
                AuthorRating = post.User.Rating,
                Created = post.Created,
                PostContent = post.Content,
                Replies = replies,
                ForumId = post.Forum.Id,
                ForumName = post.Forum.Title, 
                IsAuthorAdmin = IsAuthorAdmin(post.User)
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete (int postId, int forumId)
        {

            await _postService.Delete(postId);
            return RedirectToAction("Topic", "Forum", new { id = forumId });
            
        }

        [Authorize]
        public IActionResult Create(int id)
        {
            
            //Note id is Forum.Id
            var forum = _forumService.GetById(id);

            var model = new NewPostModel
            {
                ForumName = forum.Title,
                ForumId = forum.Id,
                ForumImageUrl = forum.ImageUrl,
                AuthorName = User.Identity.Name
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPost (NewPostModel model)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var post = BuildPost(model, user);

            await _postService.Add(post);

            //Implementation  User Rating Management 
            await _userService.UpdateUserRating(userId, typeof(Post));


            return RedirectToAction("Index", "Post", new {id = post.Id });
        }

        private Post BuildPost(NewPostModel model, ApplicationUser user)
        {
            var forum = _forumService.GetById(model.ForumId);

            return new Post
            {
                Title = model.Title, 
                Content = model.Content, 
                Created = DateTime.Now, 
                User = user, 
                Forum = forum

            };
        }
        private bool IsAuthorAdmin(ApplicationUser user)
        {
            return _userManager.GetRolesAsync(user).Result
                .Contains("Admin");
        }

        private IEnumerable<PostReplyModel> BuildPostReplies(IEnumerable<PostReply> replies)
        {
            return replies.Select(reply => new PostReplyModel 
            { 
            Id = reply.Id, 
            AuthorName = reply.User.UserName, 
            AuthorId = reply.User.Id, 
            AuthorImageUrl = reply.User.ProfileImageUrl,
            AuthorRating = reply.User.Rating, 
            Created = reply.Created,
            ReplyContent = reply.Content, 
            IsAuthorAdmin = IsAuthorAdmin(reply.User)
            });
        }
    }
}
