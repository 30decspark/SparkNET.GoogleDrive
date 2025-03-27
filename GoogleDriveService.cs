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

        public async Task<string> UploadFileAsync(string folderId, IFormFile file, string? fileName = null)
        {
            if (string.IsNullOrEmpty(folderId))
            {
                throw new Exception("Invalid Google Drive Folder!");
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

        public async Task<byte[]> DownloadFileAsync(string? fileId)
        {
            if (string.IsNullOrEmpty(fileId)) return [];

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
            if (string.IsNullOrEmpty(fileId)) return false;

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
