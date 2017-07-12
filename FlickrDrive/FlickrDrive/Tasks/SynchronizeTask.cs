using FlickrDrive.Annotations;

namespace FlickrDrive.Tasks
{
    public class SynchronizeTask
    {
        public bool IsDone { get; set; }
        public string AlbumTitle { get; set; }

        public virtual bool ContainsSynchronizationOfPhoto => false;

        public void Synchronize(Alive alive)
        {
            CurrentAttempt++;
            SynchronizeImplementation(alive);
            IsDone = true;
        }

        public virtual void SynchronizeImplementation(Alive alive)
        {
            
        }

        public int CurrentAttempt { get; set; }
    }
}