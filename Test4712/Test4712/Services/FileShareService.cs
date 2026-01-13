using Azure.Storage.Files.Shares;
using Azure;

namespace Test4712.Services
{
    public class FileShareService
    {
        private readonly string _connectionString;
        private readonly string _shareName;

        public FileShareService(string connectionString, string shareName)
        {
            _connectionString = connectionString;
            _shareName = shareName;
        }

        public async Task<string> UploadFileAsync(string fileName, Stream fileStream)
        {

            var shareClient = new ShareClient(_connectionString, _shareName);
            var directoryClient = shareClient.GetRootDirectoryClient();


            var fileClient = directoryClient.GetFileClient(fileName);
            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadRangeAsync(new HttpRange(0, fileStream.Length), fileStream);


            return fileClient.Uri.ToString();
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var fileClient = new ShareFileClient(_connectionString, _shareName, fileName);
            try
            {
                var response = await fileClient.DownloadAsync();
                return response.Value.Content;
            }
            catch (RequestFailedException)
            {

                return null;
            }
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var fileClient = new ShareFileClient(_connectionString, _shareName, fileName);

            try
            {
                await fileClient.DeleteIfExistsAsync();
            }
            catch (RequestFailedException ex)
            {

                Console.WriteLine($"Request failed: {ex.Message}");
            }
        }
    }
}
