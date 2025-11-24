namespace TitanTechnologyView.Extensions
{
    public static class FileHelpers
    {
        /// <summary>
        /// Returns the file name from a path, optionally removing prefix before last underscore.
        /// </summary>
        public static string GetCleanFileName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;

            var fileName = Path.GetFileName(filePath);

            if (fileName.Contains("_"))
                fileName = fileName.Substring(fileName.LastIndexOf("_") + 1);

            return fileName;
        }
    }
}
