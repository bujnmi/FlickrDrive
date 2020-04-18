using System;

namespace FlickrDrive.Tasks
{
    public class ActionTask : SynchronizeTask
    {
        private readonly Action _a;

        public ActionTask(Action a, string albumTitle)
        {
            AlbumTitle = albumTitle;
            _a = a;
        }
        public override void SynchronizeImplementation(FlickrAlive alive)
        {
            _a?.Invoke();
        }
    }
}