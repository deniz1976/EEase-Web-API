using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EEaseWebAPI.Domain.Entities.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EEaseWebAPI.IntegrationTests
{
    internal static class Travellers
    {
        public const string Password = "Passw0rd!x";

        public static object Registration(string username) => new
        {
            name = "Ada",
            surname = "Lovelace",
            username,
            email = $"{username}@example.com",
            password = Password,
            passwordConfirm = Password,
            gender = "Female",
            bornDate = "1990-01-01"
        };

        public static async Task<HttpClient> SignedInAsync(EEaseApiFactory factory, string username)
        {
            var anonymous = factory.CreateClient();

            (await anonymous.PostAsJsonAsync("/api/users", Registration(username)))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            await ConfirmEmailAsync(factory, username);

            var login = await anonymous.PostAsJsonAsync(
                "/api/auth/login", new { usernameOrEmail = username, password = Password });

            login.StatusCode.Should().Be(HttpStatusCode.OK);

            var token = JsonDocument.Parse(await login.Content.ReadAsStringAsync())
                .RootElement.GetProperty("body").GetProperty("token")
                .GetProperty("accessToken").GetString();

            token.Should().NotBeNullOrWhiteSpace();

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        public static async Task<JsonElement> BodyAsync(HttpResponseMessage response)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            return JsonDocument.Parse(await response.Content.ReadAsStringAsync())
                .RootElement.GetProperty("body");
        }

        private static async Task ConfirmEmailAsync(EEaseApiFactory factory, string username)
        {
            using var scope = factory.Services.CreateScope();

            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = await users.FindByNameAsync(username);

            user.Should().NotBeNull();
            user!.EmailConfirmed = true;

            await users.UpdateAsync(user);
        }
    }
}
