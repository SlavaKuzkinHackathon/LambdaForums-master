using LambdaForums.Data;
using LambdaForums.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaForums.Service
{
    public class PostService : IPost
    {
        private readonly ApplicationDbContext _context;
        public PostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Add(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            
        }

        public async Task AddReply(PostReply reply)
        {
            _context.PostReplies.Add(reply);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {

            var post = GetById(id);
            
            foreach(var reply in post.Replies)
            {
                _context.PostReplies.Remove(reply);
            }
            _context.Remove(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteForumPosts(Forum forum)
        {
            foreach (var post in forum.Posts)
            {
                await DeletePostReplies(post);
                _context.Remove(post);
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostReplies(Post post)
        {
            foreach (var reply in post.Replies)
            {
                _context.Remove(reply);
            }

            await _context.SaveChangesAsync();
        }


        public async Task DeleteReply(int id)
        {
            var reply = _context.PostReplies.FirstOrDefault(r=> r.Id == id);
            _context.PostReplies.Remove(reply);
            await _context.SaveChangesAsync();
        }

        public Task EditPostContent(int id, string newContent)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Post> GetAll()
        {
            return _context.Posts
                .Include(post => post.User)
                .Include(post => post.Replies).ThenInclude(reply => reply.User)
                .Include(post => post.Forum);
        }


        public Post GetById(int id)
        {
            return _context.Posts.Where(post => post.Id == id)
                .Include(post => post.User)
                .Include(post => post.Replies).ThenInclude(reply => reply.User)
                .Include(post => post.Forum)
                .First();
        }

        public IEnumerable<Post> GetFilteredPosts(Forum forum, string searchQuery)
        {
            return string.IsNullOrEmpty(searchQuery) 
                ? forum.Posts 
                : forum.Posts.Where(post 
                   => post.Title.Contains(searchQuery)
                   || post.Content.Contains(searchQuery));
        }

        public IEnumerable<Post> GetFilteredPosts(string searchQuery)
        {
            var normalized = searchQuery.ToLower();
            return GetAll().Where(post 
                => post.Title.ToLower().Contains(normalized)
                || post.Content.ToLower().Contains(normalized));
        }

        public IEnumerable<Post> GetLatestPosts(int n)
        {
            return GetAll().OrderByDescending(p => p.Created).Take(n);
        }

        public IEnumerable<Post> GetPostsByForum(int id)
        {
            return _context.Forums
                .Where(forum => forum.Id == id).First()
                .Posts;
        }
    }
}
