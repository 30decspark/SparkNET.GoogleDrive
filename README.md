## Usage

To use **SparkNET.GoogleDrive** for manage file on Goolge Drive.

### Example Code:

```csharp
using SparkNET.GoogleDrive;

// Create a service
string json = "{...}"; // Service account credentials
using var service = new GoogleDriveService();
string fileId = await service.UploadFileAsync(folderId, file, fileName);
```