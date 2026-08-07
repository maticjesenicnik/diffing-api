using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DiffingApi.Core;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DiffingApi.Tests.Integration;

public class DiffEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    // Mirrors the JSON options configured server-side in Program.cs (camelCase + string enums),
    // since HttpClient's ReadFromJsonAsync defaults don't automatically inherit it.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client = factory.CreateClient();

    // Each test uses its own random ID since the underlying store is a singleton shared across all tests in this
    // fixture - this keeps tests independent of run order.
    private static string NewId() => Guid.NewGuid().ToString();

    [Fact]
    public async Task Get_UnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/v1/diff/{NewId()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_OnlyLeftUploaded_StillReturns404()
    {
        var id = NewId();

        var putResponse = await _client.PutAsJsonAsync($"/v1/diff/{id}/left", new { data = "AAAAAA==" },
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, putResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/v1/diff/{id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task FullSampleScenario_MatchesAssignmentPdfExactly()
    {
        // This replays the exact request/response table from the assignment PDF, step by step,
        // as an executable spec rather than something only checked by hand.
        var id = NewId();

        // 1. GET before anything is uploaded -> 404
        Assert.Equal(HttpStatusCode.NotFound,
            (await _client.GetAsync($"/v1/diff/{id}", TestContext.Current.CancellationToken)).StatusCode);

        // 2. PUT left -> 201
        Assert.Equal(HttpStatusCode.Created,
            (await _client.PutAsJsonAsync($"/v1/diff/{id}/left", new { data = "AAAAAA==" },
                TestContext.Current.CancellationToken)).StatusCode);

        // 3. GET, right still missing -> 404
        Assert.Equal(HttpStatusCode.NotFound,
            (await _client.GetAsync($"/v1/diff/{id}", TestContext.Current.CancellationToken)).StatusCode);

        // // 4. PUT right -> 201
        Assert.Equal(HttpStatusCode.Created,
            (await _client.PutAsJsonAsync($"/v1/diff/{id}/right", new { data = "AAAAAA==" },
                cancellationToken: TestContext.Current.CancellationToken)).StatusCode);

        // // 5. GET -> 200, Equals
        var equalsResult = await ReadDiffResultAsync(
            await _client.GetAsync($"/v1/diff/{id}", TestContext.Current.CancellationToken), HttpStatusCode.OK);
        Assert.Equal(DiffResultType.Equals, equalsResult.DiffResultType);
        Assert.Null(equalsResult.Diffs);

        // // 6. Overwrite right with different content -> 201
        Assert.Equal(HttpStatusCode.Created,
            (await _client.PutAsJsonAsync($"/v1/diff/{id}/right", new { data = "AQABAQ==" },
                cancellationToken: TestContext.Current.CancellationToken)).StatusCode);

        // // 7. GET -> 200, ContentDoNotMatch with the exact diffs from the PDF
        var contentMismatchResult =
            await ReadDiffResultAsync(await _client.GetAsync($"/v1/diff/{id}", TestContext.Current.CancellationToken),
                HttpStatusCode.OK);
        Assert.Equal(DiffResultType.ContentDoNotMatch, contentMismatchResult.DiffResultType);
        Assert.Equal([new DiffSegment(0, 1), new DiffSegment(2, 2)], contentMismatchResult.Diffs);

        // // 8. Overwrite left with a shorter payload -> 201
        Assert.Equal(HttpStatusCode.Created,
            (await _client.PutAsJsonAsync($"/v1/diff/{id}/left", new { data = "AAA=" },
                cancellationToken: TestContext.Current.CancellationToken)).StatusCode);

        // // 9. GET -> 200, SizeDoNotMatch
        var sizeMismatchResult =
            await ReadDiffResultAsync(await _client.GetAsync($"/v1/diff/{id}", TestContext.Current.CancellationToken),
                HttpStatusCode.OK);
        Assert.Equal(DiffResultType.SizeDoNotMatch, sizeMismatchResult.DiffResultType);
        Assert.Null(sizeMismatchResult.Diffs);

        // // 10. PUT with null data -> 400
        Assert.Equal(HttpStatusCode.BadRequest,
            (await _client.PutAsJsonAsync($"/v1/diff/{id}/left", new { data = (string?)null },
                cancellationToken: TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Put_InvalidBase64_Returns400()
    {
        var id = NewId();

        var response = await _client.PutAsJsonAsync($"/v1/diff/{id}/left", new { data = "not-valid-base64!!" },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_MissingDataField_Returns400()
    {
        var id = NewId();

        var response =
            await _client.PutAsJsonAsync($"/v1/diff/{id}/left", new { }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_OverwritingExistingSide_StillReturns201()
    {
        // Per the PDF sample (step 6): overwriting a side that was already set returns 201,
        // same as the first PUT - not 200.
        var id = NewId();
        await _client.PutAsJsonAsync($"/v1/diff/{id}/left", new { data = "AAA=" },
            cancellationToken: TestContext.Current.CancellationToken);

        var response = await _client.PutAsJsonAsync($"/v1/diff/{id}/left", new { data = "//8=" },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static async Task<DiffResult> ReadDiffResultAsync(HttpResponseMessage response,
        HttpStatusCode expectedStatus)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DiffResult>(JsonOptions);
        Assert.NotNull(result);
        return result!;
    }
}