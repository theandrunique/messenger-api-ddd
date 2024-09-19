using MediatR;
using MessengerAPI.Application.Files.Commands.UploadFile;
using MessengerAPI.Application.Files.Queries.GetFiles;
using MessengerAPI.Application.Schemas.Common;
using MessengerAPI.Domain.Common.Errors;
using MessengerAPI.Infrastructure.Common.FileStorage;
using MessengerAPI.Presentation.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MessengerAPI.Presentation.Controllers;

[Route("files")]
public class FilesController : ApiController
{
    private readonly IMediator _mediator;
    private readonly FileStorageSettings _settings;

    public FilesController(IMediator mediator, IOptions<FileStorageSettings> settings)
    {
        _mediator = mediator;
        _settings = settings.Value;
    }

    [HttpPost]
    [ProducesResponseType(typeof(FileSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length > _settings.MaxFileSize)
        {
            return Problem(FilesErrors.TooBig(_settings.MaxFileSizeInMB));
        }

        using Stream fileStream = file.OpenReadStream();
        var sub = User.GetUserId();

        var command = new UploadFileCommand(sub, fileStream, file.ContentType, file.FileName);

        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            success => Ok(success),
            errors => Problem(errors)
        );
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<FileSchema>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFiles(CancellationToken cancellationToken)
    {
        var sub = User.GetUserId();
        var query = new GetFilesQuery(sub);

        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            success => Ok(success),
            errors => Problem(errors)
        );
    }
}
