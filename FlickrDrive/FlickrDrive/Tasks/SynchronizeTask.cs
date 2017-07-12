using FlickrDrive.Annotations;

namespace FlickrDrive.Tasks
{
    public class SynchronizeTask
    {
        public bool IsDone { get; set; }
        public string AlbumTitle { get; set; }

        public virtual bool ContainsSynchronizationOfPhoto => false;

        public virtual void Synchronize(Alive alive)
        {
            
        }

        public int CurrentAttempt { get; set; }
    }
}