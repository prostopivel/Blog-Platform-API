using BlogPlatform.Blog.API;
using BlogPlatform.Blog.IntegrationTests.DTOs;
using BlogPlatform.Blog.IntegrationTests.Helpers;
using BlogPlatform.Shared.Common.Models;
using BlogPlatform.Tests.Common;
using System.Net.Http.Json;

namespace BlogPlatform.Blog.IntegrationTests
{
    public abstract class BaseBlogIntegrationTests
        : BaseIntegrationTests<BlogApiFactory, Program>
    {
        protected readonly Func<Task> _resetState;

        protected BaseBlogIntegrationTests(BlogApiFactory factory)
            : base(factory)
        {
            _resetState = factory.ResetAsync;
        }

        public override Task InitializeAsync() => Task.CompletedTask;
        public override Task DisposeAsync() => _resetState();

        protected async Task<Guid> CreateTestPost(Guid userId,
            string title = "Test Post",
            string content = "Test Content",
            List<string>? tags = null)
        {
            var request = new
            {
                title,
                content,
                tags = tags ?? ["test", "integration"]
            };

            var response = await _client.PostAsJsonAsync($"/api/posts?userId={userId}", request);
            response.EnsureSuccessStatusCode();

            var postsResponse = await _client.GetAsync($"/api/posts/by-user/{userId}?page=1&pageSize=10");
            var posts = await postsResponse.Content.ReadFromJsonAsync<PaginatedResult<PostResponse>>(_jsonOptions);

            return posts!.Items.First().Id;
        }

        protected async Task<Guid> CreateTestComment(Guid postId,
            Guid userId,
            string content = "Test comment")
        {
            var request = new
            {
                postId,
                content
            };

            var response = await _client.PostAsJsonAsync($"/api/comments?userId={userId}", request);
            response.EnsureSuccessStatusCode();

            var commentsResponse = await _client.GetAsync($"/api/comments/by-post/{postId}?page=1&pageSize=20");
            var comments = await commentsResponse.Content.ReadFromJsonAsync<PaginatedResult<CommentResponse>>(_jsonOptions);

            return comments!.Items.First().Id;
        }
    }
}