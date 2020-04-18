using System.Collections.Generic;
using System.IO;

namespace FlickrDrive
{
    public static class Extensions
    {
        public static IEnumerable<string> FilterPhotos(this string[] str)
        {
            foreach (var s in str)
            {
                var extension = Path.GetExtension(s)?.Substring(1).ToLowerInvariant();
                if (extension == "png" || extension == "jpg" || extension == "gif")
                {
                    yield return s;
                }
            }
        }

    }
}