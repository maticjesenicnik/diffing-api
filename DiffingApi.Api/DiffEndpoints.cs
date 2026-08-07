using DiffingApi.Api.Contracts;
using DiffingApi.Api.Storage;
using DiffingApi.Core;

namespace DiffingApi.Api;

public static class DiffEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/v1/diff/{id}/left",
            (string id, DataPayload payload, IDiffEntryStore store) => PutSide(id, payload, store, DiffSide.Left)
        );

        app.MapPut("/v1/diff/{id}/right",
            (string id, DataPayload payload, IDiffEntryStore store) => PutSide(id, payload, store, DiffSide.Right)
        );

        app.MapGet("/v1/diff/{id}", (string id, IDiffEntryStore store) =>
        {
            // Assumption: GET returns 404 not only when the ID is entirely unknown, but also when
            // only one side has been uploaded so far - matching the assignment's sample (steps 1 and
            // 3), where GET returns 404 until *both* left and right have been PUT.
            if (!store.TryGet(id, out var entry) || entry is null || !entry.CanCompare)
            {
                return Results.NotFound();
            }

            var result = ByteDiffer.Compare(entry.Left!, entry.Right!);
            return Results.Ok(result);
        });
    }

    private static IResult PutSide(string id, DataPayload payload, IDiffEntryStore store, DiffSide side)
    {
        // Assumption: "data": null (or a missing "data" field) is a 400, per the assignment's
        // sample (step 10). An entirely missing/malformed JSON body is also rejected with 400 by
        // ASP.NET Core's own model binding before this method is even reached.
        if (payload.Data is null)
        {
            return Results.BadRequest();
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(payload.Data);
        }
        catch (FormatException)
        {
            // Assumption: invalid base64 is treated the same as null data - a 400 - since the
            // assignment only specifies the null case explicitly but the same reasoning clearly
            // applies to "data we can't decode at all".
            return Results.BadRequest();
        }

        var entry = store.GetOrCreate(id);
        if (side == DiffSide.Left)
        {
            entry.Left = bytes;
        }
        else
        {
            entry.Right = bytes;
        }

        // Assumption: every successful PUT returns 201 Created, even when it overwrites an
        // existing side (see the assignment's sample, step 6, which PUTs to an ID that already
        // has both sides set and still gets 201 back). This slightly bends typical REST
        // conventions (a 200 OK might be more idiomatic for an update) but follows the spec as
        // given rather than second-guessing it.
        return Results.Created();
    }
}