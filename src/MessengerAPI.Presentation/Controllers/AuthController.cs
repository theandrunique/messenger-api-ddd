﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using MessengerAPI.Application.Auth.Commands.Register;
using MessengerAPI.Presentation.Schemas.Auth;
using MessengerAPI.Application.Auth.Commands.Login;
using MessengerAPI.Presentation.Common;
using MessengerAPI.Application.Auth.Commands.RefreshToken;
using MessengerAPI.Application.Schemas.Common;
using MessengerAPI.Application.Auth.Common;

namespace MessengerAPI.Presentation.Controllers;

[Route("auth")]
[AllowAnonymous]
public class AuthController : ApiController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Sign up
    /// </summary>
    /// <param name="schema"><see cref="SignUpRequestSchema"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="UserPrivateSchema"/></returns>
    [HttpPost("sign-up")]
    [ProducesResponseType(typeof(UserPrivateSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> SignUp([FromForm] SignUpRequestSchema schema, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(schema.username, schema.globalName, schema.password);

        var registerResult = await _mediator.Send(command, cancellationToken);

        return registerResult.Match(
            success => Ok(success),
            errors => Problem(errors));
    }

    /// <summary>
    /// Sign in
    /// </summary>
    /// <param name="schema"><see cref="SignInRequestSchema"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="TokenPairResponse"/></returns>
    /// <exception cref="Exception">Thrown if IP address is null</exception>
    [HttpPost("sign-in")]
    [ProducesResponseType(typeof(TokenPairResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SignIn([FromForm] SignInRequestSchema schema, CancellationToken cancellationToken)
    {
        string userAgent = Request.Headers.UserAgent.ToString();
        string? ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();

        if (ipAddress == null)
        {
            throw new Exception("IP address expected to be not null");
        }

        var command = new LoginCommand(schema.login, schema.password, userAgent, ipAddress);
        var loginResult = await _mediator.Send(command, cancellationToken);

        return loginResult.Match(
            success =>
            {
                AddRefreshTokenToCookies(success.RefreshToken);
                return Ok(success);
            },
            errors => Problem(errors));
    }

    /// <summary>
    /// Refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="TokenPairResponse"/></returns>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenPairResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken([FromForm] string refreshToken, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(refreshToken);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            success =>
            {
                AddRefreshTokenToCookies(success.RefreshToken);
                return Ok(success);
            },
            errors => Problem(errors));
    }

    /// <summary>
    /// Get refresh token from cookies
    /// </summary>
    /// <returns>Refresh token</returns>
    [HttpGet("token")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Token()
    {
        string? refreshToken = Request.Cookies[CookieConstants.RefreshToken];
        if (refreshToken == null)
        {
            return NotFound();
        }
        return Ok(refreshToken);
    }

    private void AddRefreshTokenToCookies(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            // Secure = true,
        };
        Response.Cookies.Append(CookieConstants.RefreshToken, refreshToken, cookieOptions);
    }
}
