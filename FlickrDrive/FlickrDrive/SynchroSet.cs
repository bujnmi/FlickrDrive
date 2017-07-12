using System.Linq;

namespace FlickrDrive
{
    public class SynchroSet:BaseNotifyObject
    {
        
        private readonly Alive _alive;
        public SynchroSet(string path, Alive alive)
        {
            Path = path;
            _alive = alive;
        }

        private int _up;
        private int _down;
        private bool _isSynchronizationRequested;
        public string Path { get; set; }

        public string DirectoryPath;
        public int Up
        {
            get { return _up; }
            set
            {
                _up = value;
                OnPropertyChanged();
            }
        }

        public int Down
        {
            get { return _down; }
            set
            {
                _down = value; 
                OnPropertyChanged();
            }
        }

        public string NumberOfPhotosString
        {
            get { return $"({NumberOfPhotos} photos)"; }
        }
        public int NumberOfPhotos
        {
            get
            {
                var count = Up;
                var set = _alive.FlickrData.Sets.FirstOrDefault(s => s.Title == this.Path);
                if (set != null)
                {
                    count += set.NumberOfPhotos;
                }
                return count;
            }
        }

        public bool IsSynchronizationNeeded { get { return Down + Up != 0; } }
        public bool IsSynchronizationRequested
        {
            get { return _isSynchronizationRequested; }
            set
            {
                _isSynchronizationRequested = value;
                if (value)
                {
                    _alive.AddSynchronizeSet(this);
                }
                else
                {
                    _alive.RemoveSynchronizationOfSet(this);
                }
            }
        }
    }
}