using DanGian.Application.Common;
using DanGian.Domain.Primitives;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DanGian.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _sender;

    protected ISender Sender =>
        _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User is not authenticated."));

    protected IActionResult OkResult<T>(T data) =>
        Ok(ApiResponse<T>.Ok(data));

    protected IActionResult CreatedResult<T>(string? routeName, object? routeValues, T data) =>
        CreatedAtRoute(routeName, routeValues, ApiResponse<T>.Ok(data));

    protected IActionResult HandleResult<T>(Result<T> result) =>
        result.IsSuccess
            ? Ok(ApiResponse<T>.Ok(result.Value))
            : BadRequest(ApiResponse<T>.Fail(result.Error.Code, result.Error.Message));

    protected IActionResult HandleResult(Result result) =>
        result.IsSuccess
            ? NoContent()
            : BadRequest(ApiResponse<object>.Fail(result.Error.Code, result.Error.Message));
}
