using System.Collections.Generic;
using System.Linq;
using FlickrNet;

namespace FlickrDrive.Tasks
{
    public class OrderPhotosTask : SynchronizeTask
    {

        public OrderPhotosTask(string albumTitle)
        {
            AlbumTitle = albumTitle;
        }
        public override void SynchronizeImplementation(Alive alive)
        {
            var set = alive.FlickrData.Sets.First(s => s.Title == AlbumTitle);
            var photos = alive.GetAllPhotos(set);
            List<PhotoInfo> photoInfos = new List<PhotoInfo>(photos.Count);
            foreach (var photo in photos)
            {
                photoInfos.Add(alive.FlickrInstance.PhotosGetInfo(photo.PhotoId));
            }
            var orderedPhotos = photoInfos.OrderBy(p => p.DateTaken);
            alive.FlickrInstance.PhotosetsReorderPhotos(set.PhotosetId, orderedPhotos.Select(p => p.PhotoId).ToArray());
        }
    }
}