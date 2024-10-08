using System.Security.Cryptography;
using AutoMapper;
using ErrorOr;
using MediatR;
using MessengerAPI.Application.Common.Interfaces.Persistance;
using MessengerAPI.Application.Files.Common.Interfaces;
using MessengerAPI.Contracts.Common;
using MessengerAPI.Domain.Common.Entities;

namespace MessengerAPI.Application.Files.Commands.UploadFile;

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, ErrorOr<FileSchema>>
{
    private readonly IFileStorageService _fileStorage;
    private readonly IFileRepository _fileRepository;
    private readonly IMapper _mapper;

    public UploadFileCommandHandler(IFileStorageService fileStorage, IFileRepository fileRepository, IMapper mapper)
    {
        _fileStorage = fileStorage;
        _fileRepository = fileRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Uploading file to S3 and saving it in DB
    /// </summary>
    /// <param name="request"><see cref="UploadFileCommand"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="FileSchema"/></returns>
    public async Task<ErrorOr<FileSchema>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var sha256Bytes = ComputeSha256Hash(request.FileStream);
        var sha265String = Convert.ToHexString(sha256Bytes).ToLower();

        var key = $"{sha265String}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        request.FileStream.Position = 0;

        var url = await _fileStorage.PutAsync(
            request.FileStream,
            key,
            request.FileName,
            request.ContentType,
            cancellationToken);

        var file = FileData.CreateNew(request.Sub, request.ContentType, request.FileName, url, request.FileStream.Length, sha256Bytes);

        await _fileRepository.AddFileAsync(file, cancellationToken);

        return _mapper.Map<FileSchema>(file);
    }

    private byte[] ComputeSha256Hash(Stream stream)
    {
        using (var sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(stream);
        }
    }
}
