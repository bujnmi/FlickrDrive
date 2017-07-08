using System;
using System.Net;

namespace FlickrDrive.Tasks
{
    public class DownloadTask:ISynchronizeTask
    {
        private readonly string photoId;
        private readonly string directoryPath;

        public DownloadTask(string photoId, string directoryPath, string albumTitle)
        {
            this.photoId = photoId;
            this.directoryPath = directoryPath;
            this.AlbumTitle = albumTitle;
        }

        public string AlbumTitle { get; }

        public void Synchronize(Alive alive)
        {
            CurrentAttempt++;
            var info = alive.FlickrInstance.PhotosGetInfo(photoId);
            
            using (var wc = new WebClient())
            {
                Action a = new Action(() =>
                {
                    wc.CancelAsync();
                });
                alive.CancelActions.Add(a);
                wc.DownloadFile(info.OriginalUrl, directoryPath+Constants.DelimiterInWindowsPath+info.Title+Constants.DelimiterExtension+info.OriginalFormat);
                alive.CancelActions.Remove(a);
            }
            IsDone = true;
        }

        public int CurrentAttempt { get; set; }
        public bool IsDone { get; set; }

    }
}