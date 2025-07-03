using Microsoft.Extensions.Caching.Memory;
using MsRestApiComment.Models;
using System.Text.Json;

namespace MsRestApiComment.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(int userId, string authToken);
        Task<List<UserDto>> GetUsersByIdsOptimizedAsync(List<int> userIds, string authToken);
    }

    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10); // Cache for 10 minutes

        public UserService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId, string authToken)
        {
            // ✅ Check cache first
            var cacheKey = $"user_{userId}";
            if (_cache.TryGetValue(cacheKey, out UserDto? cachedUser))
            {
                return cachedUser;
            }

            try
            {
                // ✅ Validate token parameter
                if (string.IsNullOrEmpty(authToken))
                {
                    return null; // Cannot authenticate without token
                }

                var authServiceUrl = _configuration["Services:AuthService:BaseUrl"] ?? "https://localhost:7001";
                
                // ✅ Create request with JWT Bearer token from parameter
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{authServiceUrl}/api/auth/User/service/{userId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var user = JsonSerializer.Deserialize<UserDto>(jsonString, options);
                    
                    // ✅ Cache the result
                    if (user != null)
                    {
                        _cache.Set(cacheKey, user, _cacheExpiration);
                    }
                    
                    return user;
                }
                
                return null;
            }
            catch
            {
                // Log the exception in production
                return null;
            }
        }

        // ✅ Enhanced method with batch endpoint support
        public async Task<List<UserDto>> GetUsersByIdsOptimizedAsync(List<int> userIds, string authToken)
        {
            if (!userIds.Any()) return new List<UserDto>();

            var distinctUserIds = userIds.Distinct().ToList();
            
            // Try batch endpoint first (if available)
            try
            {
                var batchResult = await GetUsersBatchAsync(distinctUserIds, authToken);
                if (batchResult.Any())
                {
                    return batchResult;
                }
            }
            catch
            {
                // Fallback to parallel individual calls if batch fails
            }

            // Fallback: Parallel individual calls
            var userTasks = distinctUserIds.Select(userId => GetUserByIdAsync(userId, authToken)).ToArray();
            var userResults = await Task.WhenAll(userTasks);
            
            return userResults.Where(user => user != null).ToList()!;
        }

        // ✅ Batch endpoint method with caching
        private async Task<List<UserDto>> GetUsersBatchAsync(List<int> userIds, string authToken)
        {
            var uncachedUserIds = new List<int>();
            var cachedUsers = new List<UserDto>();

            // Check cache for each user ID
            foreach (var userId in userIds)
            {
                var cacheKey = $"user_{userId}";
                if (_cache.TryGetValue(cacheKey, out UserDto? cachedUser) && cachedUser != null)
                {
                    cachedUsers.Add(cachedUser);
                }
                else
                {
                    uncachedUserIds.Add(userId);
                }
            }

            // If all users were found in cache, return them
            if (!uncachedUserIds.Any())
            {
                return cachedUsers;
            }

            try
            {
                // ✅ Validate token parameter
                if (string.IsNullOrEmpty(authToken))
                {
                    return cachedUsers; // Return only cached users if cannot authenticate
                }

                var authServiceUrl = _configuration["Services:AuthService:BaseUrl"] ?? "https://localhost:7001";
                
                // ✅ Build query string with userIds parameters
                var queryParams = string.Join("&", uncachedUserIds.Select(id => $"userIds={id}"));
                var requestUrl = $"{authServiceUrl}/api/auth/User?{queryParams}";
                
                // ✅ Create GET request with JWT Bearer token from parameter
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var fetchedUsers = JsonSerializer.Deserialize<List<UserDto>>(jsonString, options) ?? new List<UserDto>();
                    
                    // ✅ Cache the newly fetched users
                    foreach (var user in fetchedUsers)
                    {
                        var cacheKey = $"user_{user.Id}";
                        _cache.Set(cacheKey, user, _cacheExpiration);
                    }
                    
                    // Combine cached and fetched users
                    cachedUsers.AddRange(fetchedUsers);
                    return cachedUsers;
                }
                
                return cachedUsers; // Return only cached users if API call fails
            }
            catch
            {
                return cachedUsers; // Return only cached users if exception occurs
            }
        }
    }
} 