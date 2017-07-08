using System;
using System.IO;
using FlickrNet;

namespace FlickrDrive.Tasks
{
    public class CreateAlbumTask : ISynchronizeTask
    {
        public Action PostAction;
        private readonly string primaryPhotoPath;
        public string AlbumId;
        public CreateAlbumTask(string primaryPhotoPath, string albumTitle)
        {
            this.primaryPhotoPath = primaryPhotoPath;
            AlbumTitle = albumTitle;
        }

        public void Synchronize(Alive alive)
        {
            CurrentAttempt++;
            string photoId;
            using (FileStream fs = new FileStream(primaryPhotoPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var photoName = Path.GetFileNameWithoutExtension(primaryPhotoPath);
                Action a = new Action(() =>
                {
                    fs.Close();
                });
                alive.CancelActions.Add(a);
                photoId = alive.FlickrInstance.UploadPicture(fs, photoName, photoName, null, null, false, false, false, ContentType.None, SafetyLevel.None, HiddenFromSearch.None);
                fs.Close();                
                alive.CancelActions.Remove(a);
            }

            var set = alive.FlickrInstance.PhotosetsCreate(AlbumTitle, photoId);
            AlbumId = set.PhotosetId;
            PostAction?.Invoke();
            IsDone = true;
        }

        public int CurrentAttempt { get; set; }

        public bool IsDone { get; set; }
        public string AlbumTitle { get; }

    }
}