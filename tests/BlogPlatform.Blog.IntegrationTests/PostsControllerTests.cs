using BlogPlatform.Blog.IntegrationTests.DTOs;
using BlogPlatform.Blog.IntegrationTests.Helpers;
using BlogPlatform.Shared.Common.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace BlogPlatform.Blog.IntegrationTests
{
    public class PostsControllerTests : BaseBlogIntegrationTests
    {
        public PostsControllerTests(BlogApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task CreatePost_WithValidData_ReturnsCreated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new
            {
                title = "Test Post Title",
                content = "Test Post Content",
                tags = new List<string> { "test", "integration" }
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/posts?userId={userId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task GetPost_WithExistingId_ReturnsPost()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var postId = await CreateTestPost(userId);

            // Act
            var response = await _client.GetAsync($"/api/posts/{postId}");
            var post = (await response.Content.ReadFromJsonAsync<PostResponse>(_jsonOptions));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            post.Should().NotBeNull();
            post!.Id.Should().Be(postId);
            post.Title.Should().Be("Test Post");
        }

        [Fact]
        public async Task GetPost_WithNonExistingId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/posts/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPostsByTags_WithExistingTags_ReturnsPosts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            await CreateTestPost(userId, "Post 1", "Content 1", ["csharp", "dotnet"]);
            await CreateTestPost(userId, "Post 2", "Content 2", ["csharp", "testing"]);

            // Act
            var response = await _client.GetAsync("/api/posts/by-tags?tags=csharp&page=1&pageSize=10");
            var result = await response.Content.ReadFromJsonAsync<PaginatedResult<PostResponse>>(_jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result!.Items.Should().HaveCount(2);
            result.Items.All(p => p.Tags.Select(t => t.Name).Contains("csharp")).Should().BeTrue();
        }

        [Fact]
        public async Task GetPostsByUser_WithExistingUser_ReturnsPosts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            await CreateTestPost(userId, "User Post 1");
            await CreateTestPost(userId, "User Post 2");

            // Act
            var response = await _client.GetAsync($"/api/posts/by-user/{userId}?page=1&pageSize=10");
            var result = await response.Content.ReadFromJsonAsync<PaginatedResult<PostResponse>>(_jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result!.Items.Should().HaveCount(2);
            result.Items.All(p => p.UserId == userId).Should().BeTrue();
        }

        [Fact]
        public async Task UpdatePost_WithValidData_ReturnsUpdatedPost()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var postId = await CreateTestPost(userId);

            var updateRequest = new
            {
                title = "Updated Title",
                content = "Updated Content",
                tags = new List<string> { "updated", "tag" }
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/posts/{postId}?userId={userId}", updateRequest);
            var updatedPost = await response.Content.ReadFromJsonAsync<PostResponse>(_jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            updatedPost.Should().NotBeNull();
            updatedPost!.Title.Should().Be("Updated Title");
            updatedPost.Content.Should().Be("Updated Content");
            updatedPost.Tags.Select(t => t.Name).Should().Contain("updated");
        }

        [Fact]
        public async Task UpdatePost_WithWrongUser_ReturnsForbidden()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var postId = await CreateTestPost(ownerId);

            var updateRequest = new
            {
                title = "Updated Title",
                content = "Updated Content",
                tags = new List<string> { "updated" }
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/posts/{postId}?userId={otherUserId}", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeletePost_WithExistingPost_ReturnsNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var postId = await CreateTestPost(userId);

            // Act
            var response = await _client.DeleteAsync($"/api/posts/{postId}?userId={userId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ToggleLike_AddsAndRemovesLike()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var postId = await CreateTestPost(userId);

            // Act & Assert - Добавляем лайк
            var likeResponse = await _client.PostAsync($"/api/posts/{postId}/like/toggle?userId={userId}", null);
            likeResponse.EnsureSuccessStatusCode();
            var likeResult = await likeResponse.Content.ReadFromJsonAsync<LikeResult>(_jsonOptions);
            likeResult!.Liked.Should().BeTrue();
            likeResult.LikesCount.Should().Be(1);

            // Проверяем статус лайка
            var statusResponse = await _client.GetAsync($"/api/posts/{postId}/like/status?userId={userId}");
            var status = await statusResponse.Content.ReadFromJsonAsync<LikeResult>(_jsonOptions);
            status!.Liked.Should().BeTrue();

            // Удаляем лайк
            var unlikeResponse = await _client.PostAsync($"/api/posts/{postId}/like/toggle?userId={userId}", null);
            unlikeResponse.EnsureSuccessStatusCode();
            var unlikeResult = await unlikeResponse.Content.ReadFromJsonAsync<LikeResult>(_jsonOptions);
            unlikeResult!.Liked.Should().BeFalse();
            unlikeResult.LikesCount.Should().Be(0);
        }
    }
}