using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;

namespace GymManagementAPI.Services
{
    public class FirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public FirebaseStorageService(IConfiguration config)
        {
            // 1. Load Base64-encoded JSON from user-secrets or Azure App Settings
            var base64Json = config["Firebase:CredentialsJsonBase64"];
            if (string.IsNullOrWhiteSpace(base64Json))
                throw new Exception("Firebase:CredentialsJsonBase64 is missing. Add it to user-secrets or Azure App Settings.");

            // 2. Decode base64 → JSON
            var jsonBytes = Convert.FromBase64String(base64Json);
            using var stream = new MemoryStream(jsonBytes);

            var credential = GoogleCredential.FromStream(stream);
            _storageClient = StorageClient.Create(credential);
            _bucketName = config["Firebase:Bucket"];
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileUrl))
                {
                    Console.WriteLine("⚠️ Skipped empty Firebase URL");
                    return;
                }

                // ✅ Extract the part after "/o/" and before "?alt="
                var startIndex = fileUrl.IndexOf("/o/") + 3;
                if (startIndex < 3)
                {
                    Console.WriteLine($"⚠️ Invalid Firebase URL: {fileUrl}");
                    return;
                }

                int endIndex = fileUrl.IndexOf("?alt=");
                string objectNameEncoded;

                if (endIndex > startIndex)
                    objectNameEncoded = fileUrl.Substring(startIndex, endIndex - startIndex);
                else
                    objectNameEncoded = fileUrl.Substring(startIndex); // fallback, just in case

                // Decode %2F → /
                var objectName = Uri.UnescapeDataString(objectNameEncoded);

                // ✅ Delete file from Firebase Storage bucket
                await _storageClient.DeleteObjectAsync(_bucketName, objectName);
                Console.WriteLine($"✅ Deleted from Firebase: {objectName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error deleting file from Firebase: {ex.Message}");
            }
        }
    }
}
