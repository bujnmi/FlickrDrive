using FlickrDrive.Annotations;

namespace FlickrDrive.Tasks
{
    public class SynchronizeTask
    {
        public bool IsDone { get; set; }
        public string AlbumTitle { get; set; }

        public virtual bool ContainsSynchronizationOfPhoto => false;

        public void Synchronize(FlickrAlive alive)
        {
            CurrentAttempt++;
            SynchronizeImplementation(alive);
            IsDone = true;
        }

        public virtual void SynchronizeImplementation(FlickrAlive alive)
        {
            
        }

        public int CurrentAttempt { get; set; }
    }
}