using Allup.Utilities.Enums;

namespace Allup.Utilities.Extensions
{
    public static class FileValidator
    {

        public static bool ValidateType(this IFormFile file, string type)
        {
            if (file.ContentType.Contains(type))
            {
                return true;
            }
            return false;
        }

        public static bool ValidateSize(this IFormFile file, FileSize fileSize, int size)
        {
            switch (fileSize)
            {
                case FileSize.KB:
                    return file.Length <= size * 1024;

                case FileSize.MB:
                    return file.Length <= size * 1024 * 1024;

                case FileSize.GB:
                    return file.Length <= size * 1024 * 1024 * 1024;

            }
            return false;
        }
        public static string CombinePaths(string[] roots)
        {

            string path = roots[0];

            for (int i = 1; i < roots.Length; i++)
            {
                path = Path.Combine(path, roots[i]);
            }

            return path;
        }

        public static async Task<string> CreateFileAsync(this IFormFile file, params string[] roots)
        {
            string fileName = string.Concat(Guid.NewGuid().ToString(), file.FileName.Substring(file.FileName.LastIndexOf('.')));
            string path = CombinePaths(roots);

            path = Path.Combine(path, fileName);

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return fileName;




        }

        public static void DeleteFile(this string fileName, params string[] roots)
        {
            string path = CombinePaths(roots);


            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

    }
}
