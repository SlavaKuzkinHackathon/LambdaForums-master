﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LambdaForums.Data;
using LambdaForums.Data.Models;
using LambdaForums.Models.Forum;
using LambdaForums.Models.Post;
using LambdaForums.Models.Search;
using Microsoft.AspNetCore.Mvc;

namespace LambdaForums.Controllers
{
    public class SearchController : Controller
    {
        private readonly IPost _postService;

        public SearchController(IPost postService)
        {
            _postService = postService;
        }

        public IActionResult Results(string searchQuery)
        {
            var posts = _postService.GetFilteredPosts(searchQuery);
            var areNoResults = (!string.IsNullOrEmpty(searchQuery) && !posts.Any());

            var postListings = posts.Select(post => new PostListingModel
            {
                Id = post.Id, 
                AuthorId = post.User.Id, 
                AuthorName = post.User.UserName, 
                AuthorRating = post.User.Rating, 
                Title = post.Title, 
                RepliesCount = post.Replies.Count(), 
                Forum = BuildForumListing (post)
            });

            var model = new SearchResultModel
            {
                Posts = postListings, 
                SearchQuery = searchQuery, 
                EmptySearchResults = areNoResults
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Search(string searchQuery)
        {
            return RedirectToAction("Results", new { searchQuery });
        }

        private ForumListingModel BuildForumListing(Post post)
        {
            var forum = post.Forum;

            return new ForumListingModel
            {
                Id = forum.Id, 
                ImageUrl = forum.ImageUrl, 
                Name = forum.Title, 
                Description = forum.Description
            };
        }


    }
}
