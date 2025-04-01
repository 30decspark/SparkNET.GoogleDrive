using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Http;

namespace SparkNET.GoogleDrive
{
    public class GoogleDriveService : IDisposable
    {
        private readonly DriveService _service;
        private bool _disposed = false;

        public GoogleDriveService(string json)
        {
            GoogleCredential cred = GoogleCredential.FromJson(json)
                .CreateScoped([DriveService.ScopeConstants.DriveFile]);
            _service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred
            });
        }

        public async Task<string> UploadFileAsync(string? folderId, IFormFile file, string? fileName = null)
        {
            if (string.IsNullOrWhiteSpace(folderId))
            {
                throw new ArgumentException("The \"folderId\" parameter cannot be null or empty.", nameof(folderId));
            }

            var meta = new Google.Apis.Drive.v3.Data.File 
            {
                Name = fileName ?? file.FileName,
                Parents = [folderId]
            };

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var request = _service.Files.Create(meta, ms, file.ContentType);
            request.Fields = "id";

            var response = await request.UploadAsync(CancellationToken.None);
            if (response.Status == UploadStatus.Failed)
            {
                throw new Exception(response.Exception.Message);
            }
            return request.ResponseBody.Id;
        }

        public async Task<string> UploadFileAsync(string? folderId, byte[] file, string? fileName, string? mimeType)
        {
            if (string.IsNullOrWhiteSpace(folderId))
            {
                throw new ArgumentException("The \"folderId\" parameter cannot be null or empty.", nameof(folderId));
            }
            else if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("The \"fileName\" parameter cannot be null or empty.", nameof(fileName));
            }
            else if (string.IsNullOrWhiteSpace(mimeType))
            {
                throw new ArgumentException("The \"mimeType\" parameter cannot be null or empty.", nameof(mimeType));
            }

            var meta = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                Parents = [folderId]
            };

            using var ms = new MemoryStream(file);
            var request = _service.Files.Create(meta, ms, mimeType);
            request.Fields = "id";

            var response = await request.UploadAsync(CancellationToken.None);
            if (response.Status == UploadStatus.Failed)
            {
                throw new Exception(response.Exception.Message);
            }
            return request.ResponseBody.Id;
        }

        public async Task<byte[]> DownloadFileAsync(string? fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId)) return [];

            try
            {
                using var ms = new MemoryStream();
                var request = _service.Files.Get(fileId);
                await request.DownloadAsync(ms);
                return ms.ToArray();
            }
            catch
            {
                return [];
            }
        }

        public async Task<bool> DeleteFileAsync(string? fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId)) return false;

            try
            {
                var request = _service.Files.Delete(fileId);
                await request.ExecuteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _service?.Dispose();
                }
                _disposed = true;
            }
        }

        ~GoogleDriveService()
        {
            Dispose(false);
        }
    }
}
