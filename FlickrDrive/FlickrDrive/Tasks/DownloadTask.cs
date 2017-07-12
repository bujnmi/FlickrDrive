using System;
using System.Net;

namespace FlickrDrive.Tasks
{
    public class DownloadTask:SynchronizeTask
    {
        private readonly string photoId;
        private readonly string directoryPath;

        public override bool ContainsSynchronizationOfPhoto => true;

        public DownloadTask(string photoId, string directoryPath, string albumTitle)
        {
            this.photoId = photoId;
            this.directoryPath = directoryPath;
            this.AlbumTitle = albumTitle;
        }


        public override void Synchronize(Alive alive)
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

    }
}