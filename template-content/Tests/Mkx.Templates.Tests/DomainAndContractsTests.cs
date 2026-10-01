using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Mkx.Templates.Domain.TestAggregate;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Sdk.Server.Shared.Data;
using Mkx.Templates.Sdk.Shared.Exceptions;
using Mkx.Templates.Shared.Authorization;
using Mkx.Templates.Shared.Routes;
using DomainTest = Mkx.Templates.Domain.TestAggregate.Test;

namespace Mkx.Templates.Tests;

public class DomainAndContractsTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AggregateRejectsEmptyName(string name) => Assert.Throws<ArgumentException>(() => DomainTest.Create(name, null));
    [Fact] public void AggregateRejectsOverlongContent() { Assert.Throws<ArgumentException>(() => DomainTest.Create(new string('a', 201), null)); Assert.Throws<ArgumentException>(() => DomainTest.Create("name", new string('a', 501))); }
    [Fact] public void UpdatingAggregateChangesVersion() { var entity = DomainTest.Create(" before ", " "); var version = entity.Version; Assert.Equal("before", entity.Name); Assert.Null(entity.Description); entity.Update("after", null); Assert.NotEqual(version, entity.Version); }
    [Theory]
    [InlineData(-3, 1000, 0, 100)]
    [InlineData(null, null, 0, 25)]
    [InlineData(0, 0, 0, 1)]
    public void PagingIsBounded(int? skip, int? take, int expectedSkip, int expectedTake) { var result = new RequestFilter(skip, take, new string('s', 300)).Normalize(); Assert.Equal(expectedSkip, result.Skip); Assert.Equal(expectedTake, result.Take); Assert.Equal(200, result.Search!.Length); }
    [Fact] public void UrlsEncodeSearch() { var url = ApiUrls.Tests.List(new RequestFilter(Search: "a&b #?")); Assert.Contains("Search=a%26b", url, StringComparison.OrdinalIgnoreCase); }
    private static ServiceProvider Authorization(bool register) { var services = new ServiceCollection(); services.AddLogging(); services.AddAuthorizationCore(); services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>(); if (register) services.AddSingleton<IApplicationPolicyProvider, AppPolicyProvider>(); return services.BuildServiceProvider(); }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UnknownPoliciesNeverBecomeDefault(bool register) { await using var services = Authorization(register); var auth = services.GetRequiredService<IAuthorizationService>(); var user = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "user")], "test")); await Assert.ThrowsAsync<InvalidOperationException>(() => auth.AuthorizeAsync(user, null, "Tests.Misspelled")); }
    [Fact] public async Task AuthenticatedUserWithoutCapabilityIsDenied() { await using var services = Authorization(true); var auth = services.GetRequiredService<IAuthorizationService>(); var user = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name, "user")], "test")); Assert.False((await auth.AuthorizeAsync(user, null, AppPolicies.Tests.View)).Succeeded); }
    [Theory]
    [InlineData(401, typeof(HttpRequestAuthenticationFailedException))]
    [InlineData(403, typeof(HttpRequestAuthorizationFailedException))]
    [InlineData(409, typeof(HttpRequestFailedException))]
    public async Task HttpErrorsRetainStatus(int code, Type type) { using var response = new HttpResponseMessage((HttpStatusCode)code) { Content = new StringContent("not-json") }; var error = await HttpRequestFailedException.GetExceptionAsync(response, TestContext.Current.CancellationToken); Assert.IsType(type, error); Assert.Equal(code, (int)((HttpRequestFailedException)error).StatusCode); }
    [Fact] public async Task ValidationFieldsAreAvailable() { using var response = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("{\"errors\":{\"Name\":[\"required\"]}}") }; var error = Assert.IsType<HttpRequestValidationException>(await HttpRequestFailedException.GetExceptionAsync(response, TestContext.Current.CancellationToken)); Assert.Equal("required", Assert.Single(error.Errors["Name"])); }
    [Fact] public void DomainAssembliesDoNotDependOnEfOrInfrastructure() { foreach (var assembly in new[] { typeof(DomainTest).Assembly, typeof(Mkx.Templates.Sdk.Server.Domain.EntityBase).Assembly }) Assert.DoesNotContain(assembly.GetReferencedAssemblies(), name => name.Name!.Contains("EntityFramework", StringComparison.OrdinalIgnoreCase) || name.Name.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase)); }
    [Fact] public void IdentityModelsBelongToSdkDomain() => Assert.Equal(typeof(Mkx.Templates.Sdk.Server.Domain.EntityBase).Assembly, typeof(Mkx.Templates.Sdk.Server.Domain.Identity.AppUser).Assembly);
}
