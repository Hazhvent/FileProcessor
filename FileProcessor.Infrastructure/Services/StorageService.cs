using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Exceptions;
using FileProcessor.Core.Interfaces.Services;
using FileProcessor.Core.Utils;
using SMBLibrary;
using SMBLibrary.Client;

namespace FileProcessor.Infrastructure.Services;

public sealed class StorageService : IStorageService
{
    public string ReadNetworkTextFile(ISMBFileStore fileStore, string smbFilePath)
    {
        if (string.IsNullOrWhiteSpace(smbFilePath)) return string.Empty;

        NTStatus status = fileStore.CreateFile(
            out object fileHandle,
            out _,
            smbFilePath,
            AccessMask.GENERIC_READ,
            SMBLibrary.FileAttributes.Normal,
            ShareAccess.Read,
            CreateDisposition.FILE_OPEN,
            CreateOptions.FILE_NON_DIRECTORY_FILE,
            null);

        if (status != NTStatus.STATUS_SUCCESS)
        {
            throw status switch
            {
                NTStatus.STATUS_SHARING_VIOLATION => new Exceptions.InternalServerErrorException(ErrorConstants.INTERNAL_SMB_FILE_LOCKED),
                NTStatus.STATUS_ACCESS_DENIED => new Exceptions.InternalServerErrorException(ErrorConstants.INTERNAL_SMB_AUTH_READ_DENIED),
                NTStatus.STATUS_OBJECT_NAME_NOT_FOUND => new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_PATH_NOT_EXISTS, smbFilePath)),
                _ => new Exceptions.InternalServerErrorException(string.Format(ErrorConstants.INTERNAL_SMB_GENERIC_ERROR, status))
            };
        }

        try
        {
            long fileSize = GetNetworkFileSize(fileStore, smbFilePath) ?? 0;
            using MemoryStream ms = new();

            long bytesLeidos = 0;
            while (bytesLeidos < fileSize)
            {
                int bytesToRead = Helper.CalculateNextChunkSize(fileSize, bytesLeidos, SettingsConstants.SMB_READ_CHUNK_SIZE);
                NTStatus readStatus = fileStore.ReadFile(out byte[] buffer, fileHandle, bytesLeidos, bytesToRead);

                if (readStatus != NTStatus.STATUS_SUCCESS && readStatus != NTStatus.STATUS_END_OF_FILE)
                {
                    throw new Exceptions.InternalServerErrorException(string.Format(ErrorConstants.INTERNAL_SMB_GENERIC_ERROR, readStatus));
                }

                if (buffer != null && buffer.Length > 0)
                {
                    ms.Write(buffer, 0, buffer.Length);
                    bytesLeidos += buffer.Length;
                }
                else
                {
                    break;
                }
            }

            ms.Position = 0;
            using StreamReader reader = new(ms, System.Text.Encoding.UTF8);
            return reader.ReadToEnd();
        }
        finally
        {
            fileStore.CloseFile(fileHandle);
        }
    }

    // 🌟 NUESTRO ÚNICO WRITE A LA RED (Maneja la segmentación automáticamente)
    public void WriteNetworkFile(ISMBFileStore fileStore, string filePath, byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length == 0) return;
        if (string.IsNullOrWhiteSpace(filePath)) return;

        NTStatus status = fileStore.CreateFile(
            out object fileHandle,
            out _,
            filePath,
            AccessMask.GENERIC_WRITE,
            SMBLibrary.FileAttributes.Normal,
            ShareAccess.None,
            CreateDisposition.FILE_SUPERSEDE,
            CreateOptions.FILE_NON_DIRECTORY_FILE,
            null);

        if (status != NTStatus.STATUS_SUCCESS)
        {
            throw status switch
            {
                NTStatus.STATUS_SHARING_VIOLATION => new Exceptions.InternalServerErrorException(ErrorConstants.INTERNAL_SMB_FILE_LOCKED),
                NTStatus.STATUS_ACCESS_DENIED => new Exceptions.InternalServerErrorException(ErrorConstants.INTERNAL_SMB_AUTH_WRITE_DENIED),
                _ => new Exceptions.InternalServerErrorException(string.Format(ErrorConstants.INTERNAL_SMB_GENERIC_ERROR, status))
            };
        }

        try
        {
            int chunkSize = SettingsConstants.SMB_READ_CHUNK_SIZE; // Usualmente 65536 (64KB)
            long offset = 0;

            while (offset < fileBytes.Length)
            {
                int bytesToWrite = (int)Math.Min(chunkSize, fileBytes.Length - offset);
                byte[] chunkBuffer = new byte[bytesToWrite];
                Buffer.BlockCopy(fileBytes, (int)offset, chunkBuffer, 0, bytesToWrite);

                NTStatus writeStatus = fileStore.WriteFile(out _, fileHandle, offset, chunkBuffer);
                if (writeStatus != NTStatus.STATUS_SUCCESS)
                {
                    throw new Exceptions.InternalServerErrorException(string.Format(ErrorConstants.INTERNAL_SMB_GENERIC_ERROR, writeStatus));
                }
                offset += bytesToWrite;
            }
        }
        finally
        {
            fileStore.CloseFile(fileHandle);
        }
    }

    public bool CheckNetworkDirectoryExists(ISMBFileStore fileStore, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return false;

        NTStatus status = fileStore.CreateFile(
            out object directoryHandle,
            out _,
            relativePath,
            AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE,
            SMBLibrary.FileAttributes.Directory,
            ShareAccess.Read | ShareAccess.Write | ShareAccess.Delete,
            CreateDisposition.FILE_OPEN,
            CreateOptions.FILE_DIRECTORY_FILE,
            null);

        if (status == NTStatus.STATUS_SUCCESS)
        {
            fileStore.CloseFile(directoryHandle);
            return true;
        }

        if (status == NTStatus.STATUS_OBJECT_NAME_NOT_FOUND || status == NTStatus.STATUS_OBJECT_PATH_NOT_FOUND)
        {
            return false;
        }

        throw status switch
        {
            NTStatus.STATUS_ACCESS_DENIED => new Exceptions.InternalServerErrorException(ErrorConstants.INTERNAL_SMB_AUTH_READ_DENIED),
            NTStatus.STATUS_SHARING_VIOLATION => new Exceptions.InternalServerErrorException(ErrorConstants.INTERNAL_SMB_FILE_LOCKED),
            _ => new Exceptions.InternalServerErrorException(string.Format(ErrorConstants.INTERNAL_SMB_GENERIC_ERROR, status))
        };
    }

    public long? GetNetworkFileSize(ISMBFileStore fileStore, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return null;

        NTStatus status = fileStore.CreateFile(
            out object fileHandle,
            out _,
            filePath,
            AccessMask.GENERIC_READ,
            SMBLibrary.FileAttributes.Normal,
            ShareAccess.Read | ShareAccess.Write,
            CreateDisposition.FILE_OPEN,
            CreateOptions.FILE_NON_DIRECTORY_FILE,
            null);

        if (status == NTStatus.STATUS_OBJECT_NAME_NOT_FOUND || status == NTStatus.STATUS_OBJECT_PATH_NOT_FOUND)
        {
            return null;
        }

        if (status != NTStatus.STATUS_SUCCESS)
        {
            throw status switch
            {
                NTStatus.STATUS_ACCESS_DENIED => new Exceptions.InternalServerErrorException(ErrorConstants.INTERNAL_SMB_AUTH_READ_DENIED),
                NTStatus.STATUS_SHARING_VIOLATION => new Exceptions.InternalServerErrorException(ErrorConstants.INTERNAL_SMB_FILE_LOCKED),
                _ => new Exceptions.InternalServerErrorException(string.Format(ErrorConstants.INTERNAL_SMB_GENERIC_ERROR, status))
            };
        }

        try
        {
            NTStatus infoStatus = fileStore.GetFileInformation(out FileInformation fileInfo, fileHandle, FileInformationClass.FileStandardInformation);

            if (infoStatus != NTStatus.STATUS_SUCCESS)
            {
                throw new Exceptions.InternalServerErrorException(string.Format(ErrorConstants.INTERNAL_SMB_GENERIC_ERROR, infoStatus));
            }

            if (fileInfo is FileStandardInformation standardInfo)
            {
                return standardInfo.EndOfFile;
            }

            return null;
        }
        finally
        {
            fileStore.CloseFile(fileHandle);
        }
    }

    public void EnsureNetworkDirectoriesExist(ISMBFileStore fileStore, IEnumerable<Parametros> routeConfigurations)
    {
        if (routeConfigurations == null || !routeConfigurations.Any()) return;

        foreach (var routeConfig in routeConfigurations)
        {
            string relativePath = routeConfig.Parametro;

            if (!string.IsNullOrWhiteSpace(relativePath))
            {
                relativePath = relativePath.TrimStart(SettingsConstants.PATH_SEPARATOR_BACKSLASH, SettingsConstants.PATH_SEPARATOR_SLASH);

                char[] separadores = { SettingsConstants.PATH_SEPARATOR_BACKSLASH, SettingsConstants.PATH_SEPARATOR_SLASH };
                string[] segments = relativePath.Split(separadores, StringSplitOptions.RemoveEmptyEntries);

                string currentPath = string.Empty;

                foreach (string segment in segments)
                {
                    currentPath = string.IsNullOrEmpty(currentPath) ? segment : Path.Combine(currentPath, segment);

                    if (!CheckNetworkDirectoryExists(fileStore, currentPath))
                    {
                        NTStatus status = fileStore.CreateFile(
                            out object directoryHandle,
                            out _,
                            currentPath,
                            AccessMask.GENERIC_READ | AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE,
                            SMBLibrary.FileAttributes.Directory,
                            ShareAccess.Read | ShareAccess.Write | ShareAccess.Delete,
                            CreateDisposition.FILE_OPEN_IF,
                            CreateOptions.FILE_DIRECTORY_FILE,
                            null);

                        if (status == NTStatus.STATUS_SUCCESS)
                        {
                            fileStore.CloseFile(directoryHandle);
                        }
                        else
                        {
                            throw status switch
                            {
                                NTStatus.STATUS_SHARING_VIOLATION => new Exceptions.InternalServerErrorException(ErrorConstants.INTERNAL_SMB_FILE_LOCKED),
                                NTStatus.STATUS_ACCESS_DENIED => new Exceptions.InternalServerErrorException(ErrorConstants.INTERNAL_SMB_AUTH_WRITE_DENIED),
                                NTStatus.STATUS_OBJECT_NAME_NOT_FOUND => new Exceptions.BadRequestException(string.Format(ErrorConstants.REQUEST_PATH_NOT_EXISTS, currentPath)),
                                _ => new Exceptions.InternalServerErrorException(string.Format(ErrorConstants.INTERNAL_SMB_GENERIC_ERROR, status))
                            };
                        }
                    }
                }
            }
        }
    }

    public void DeleteNetworkFile(ISMBFileStore fileStore, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;

        NTStatus status = fileStore.CreateFile(
            out object fileHandle,
            out _,
            filePath,
            AccessMask.DELETE,
            SMBLibrary.FileAttributes.Normal,
            ShareAccess.Read | ShareAccess.Write | ShareAccess.Delete,
            CreateDisposition.FILE_OPEN,
            CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_DELETE_ON_CLOSE,
            null);

        if (status == NTStatus.STATUS_OBJECT_NAME_NOT_FOUND || status == NTStatus.STATUS_OBJECT_PATH_NOT_FOUND)
        {
            return;
        }

        if (status != NTStatus.STATUS_SUCCESS)
        {
            throw status switch
            {
                NTStatus.STATUS_SHARING_VIOLATION => new Exceptions.InternalServerErrorException(ErrorConstants.INTERNAL_SMB_FILE_LOCKED),
                NTStatus.STATUS_ACCESS_DENIED => new Exceptions.InternalServerErrorException(ErrorConstants.INTERNAL_SMB_AUTH_WRITE_DENIED),
                _ => new Exceptions.InternalServerErrorException(string.Format(ErrorConstants.INTERNAL_SMB_GENERIC_ERROR, status))
            };
        }

        fileStore.CloseFile(fileHandle);
    }
}