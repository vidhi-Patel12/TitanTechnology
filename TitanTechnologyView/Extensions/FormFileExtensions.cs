// Helpers/FormFileExtensions.cs
using System.Net.Http.Headers;
using TitanTechnologyView.Extensions;

namespace TitanTechnologyView.Helpers
{
    public static class FormFileExtensions
    {
        /// <summary>
        /// Attaches either a new uploaded file or an existing remote file URL into MultipartFormDataContent.
        /// </summary>
        public static async Task AttachFileAsync(this MultipartFormDataContent content, HttpClient client, string fieldName,
                                                 IFormFile? newFile,  string? existingRelativeUrl, string apiOrigin)
        {
            if (newFile != null)
            {
                var streamContent = new StreamContent(newFile.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(newFile.ContentType);
                content.Add(streamContent, fieldName, newFile.FileName);
                return;
            }

            if (!string.IsNullOrWhiteSpace(existingRelativeUrl))
            {
                // If existing URL is relative, make it absolute
                var absoluteUrl = existingRelativeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? existingRelativeUrl
                    : $"{apiOrigin}{existingRelativeUrl}";

                var fileBytes = await client.GetByteArrayAsync(absoluteUrl);
                var fileName = Path.GetFileName(existingRelativeUrl);

                var byteContent = new ByteArrayContent(fileBytes);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue(FileHelper.GuessMine(fileName));
                content.Add(byteContent, fieldName, fileName);
            }
        }
    }
}
