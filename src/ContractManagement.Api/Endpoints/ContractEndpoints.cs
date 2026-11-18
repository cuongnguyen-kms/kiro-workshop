using System.Globalization;
using ContractManagement.Api.Contracts;
using ContractManagement.Api.Services;
using ContractManagement.Api.Validation;

namespace ContractManagement.Api.Endpoints;

/// <summary>The three contract endpoints under <c>/api/v1/contracts</c> (contract C1).</summary>
public static class ContractEndpoints
{
    /// <summary>Route prefix of version 1 of the API.</summary>
    public const string Prefix = "/api/v1";

    /// <summary>Maps create, list and read. Sign-in can later be added once to the returned group (NFR3.2).</summary>
    public static RouteGroupBuilder MapContractEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(Prefix);

        group.MapPost("/contracts", CreateAsync).WithName("createContract");
        group.MapPost("/contracts/preview", Preview).WithName("previewContractSchedule");
        group.MapGet("/contracts", ListAsync).WithName("listContracts");
        group.MapGet("/contracts/{id}", GetAsync).WithName("getContract");
        return group;
    }

    private static async Task<IResult> CreateAsync(CreateContractRequest? request, ContractService service, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return Results.ValidationProblem(result.Errors!);
        }

        var detail = ContractMapper.ToDetail(result.Contract!);
        return Results.Created($"{Prefix}/contracts/{detail.Id}", detail);
    }

    private static IResult Preview(CreateContractRequest? request, string? kind, ContractService service)
    {
        // An absent kind means expense; a value that is not payment or expense also takes the default.
        var previewKind = string.Equals(kind, "payment", StringComparison.OrdinalIgnoreCase) ? PreviewKind.Payment : PreviewKind.Expense;
        var result = service.Preview(request, previewKind);
        return result.Succeeded ? Results.Ok(result.Preview) : Results.ValidationProblem(result.Errors!);
    }

    private static async Task<IResult> ListAsync(string? page, string? pageSize, ContractService service, CancellationToken cancellationToken)
    {
        // An absent value takes the default; a value that is not a whole number fails the same rule as a wrong number (BR5.3).
        var pageNumber = page is null ? 1 : int.TryParse(page, NumberStyles.Integer, CultureInfo.InvariantCulture, out var p) ? p : 0;
        var size = pageSize is null ? 10 : int.TryParse(pageSize, NumberStyles.Integer, CultureInfo.InvariantCulture, out var s) ? s : 0;

        var errors = ContractValidator.ValidatePaging(pageNumber, size);
        if (!errors.IsEmpty)
        {
            return Results.ValidationProblem(errors.ToDictionary());
        }

        var result = await service.ListAsync(pageNumber, size, cancellationToken);
        return Results.Ok(ContractMapper.ToPage(result, pageNumber, size));
    }

    private static async Task<IResult> GetAsync(string id, ContractService service, CancellationToken cancellationToken)
    {
        var contract = Guid.TryParse(id, out var contractId) ? await service.GetAsync(contractId, cancellationToken) : null;
        return contract is null
            ? Results.Problem(title: "Contract not found.", statusCode: StatusCodes.Status404NotFound)
            : Results.Ok(ContractMapper.ToDetail(contract));
    }
}
