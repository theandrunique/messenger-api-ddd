using FluentValidation;
using MessengerAPI.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace MessengerAPI.Application.Files.Commands.UploadFile;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator(IFileStorageSettings settings)
    {
        RuleFor(f => f.FileStream.Length).LessThanOrEqualTo(settings.MaxFileSize);
    }
}
