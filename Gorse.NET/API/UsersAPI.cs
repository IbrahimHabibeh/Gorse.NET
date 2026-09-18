using System.Globalization;
using Gorse.NET.Models;
using RestSharp;

namespace Gorse.NET;

public partial class Gorse
{
    public Result InsertUser(User user)
    {
        return _client.Request<Result, User>(Method.Post, "api/user", user)!;
    }

    public Task<Result> InsertUserAsync(User user, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, User>(Method.Post, "api/user", user, cancellationToken)!;
    }

    public Result InsertUsers(IEnumerable<User> users)
    {
        return _client.Request<Result, IEnumerable<User>>(Method.Post, "api/users", users)!;
    }

    public Task<Result> InsertUsersAsync(IEnumerable<User> users, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, IEnumerable<User>>(Method.Post, "api/users", users, cancellationToken)!;
    }

    public User GetUser(string userId)
    {
        return _client.Request<User, object>(Method.Get, $"api/user/{Seg(userId)}", null)!;
    }

    public Task<User> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<User, object>(Method.Get, $"api/user/{Seg(userId)}", null, cancellationToken)!;
    }

    public UsersResponse GetUsers(int n, string cursor = "")
    {
        return _client.Request<UsersResponse, object>(Method.Get, UsersResource(n, cursor), null)!;
    }

    public Task<UsersResponse> GetUsersAsync(int n, string cursor = "", CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<UsersResponse, object>(Method.Get, UsersResource(n, cursor), null, cancellationToken)!;
    }

    public Result DeleteUser(string userId)
    {
        return _client.Request<Result, object>(Method.Delete, $"api/user/{Seg(userId)}", null)!;
    }

    public Task<Result> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, object>(Method.Delete, $"api/user/{Seg(userId)}", null, cancellationToken)!;
    }

    /// <summary>
    /// Patches a user with every property of <paramref name="userToUpdate"/>, including an
    /// empty Comment. Prefer the <see cref="UserPatch"/> overload.
    /// </summary>
    public Result UpdateUser(string userId, User userToUpdate)
    {
        return _client.Request<Result, object>(Method.Patch, $"api/user/{Seg(userId)}", userToUpdate)!;
    }

    /// <inheritdoc cref="UpdateUser(string, User)"/>
    public Task<Result> UpdateUserAsync(string userId, User userToUpdate, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, object>(Method.Patch, $"api/user/{Seg(userId)}", userToUpdate, cancellationToken)!;
    }

    /// <summary>Patches only the properties set on <paramref name="patch"/>.</summary>
    public Result UpdateUser(string userId, UserPatch patch)
    {
        return _client.Request<Result, UserPatch>(Method.Patch, $"api/user/{Seg(userId)}", patch)!;
    }

    /// <inheritdoc cref="UpdateUser(string, UserPatch)"/>
    public Task<Result> UpdateUserAsync(string userId, UserPatch patch, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, UserPatch>(Method.Patch, $"api/user/{Seg(userId)}", patch, cancellationToken)!;
    }

    private static string UsersResource(int n, string cursor) =>
        "api/users" + Query(("n", n.ToString(CultureInfo.InvariantCulture)), ("cursor", cursor));
}
