using System.Linq;
using FlickrNet;

namespace FlickrDrive
{
    public class SynchroSet:BaseNotifyObject
    {
        public bool IsFamily
        {
            get { return _isFamily; }
            set
            {
                _isFamily = value;
                _alive.AddPermToSynchroSet(this);
            }
        }

        public bool IsPublic
        {
            get { return _isPublic; }
            set
            {
                _isPublic = value;
                _alive.AddPermToSynchroSet(this);

            }
        }

        public bool IsSearchable
        {
            get { return _isSearchable; }
            set
            {
                _isSearchable = value;
                _alive.AddPermToSynchroSet(this);

            }
        }

        public bool IsReorderPhotosRequested
        {
            get { return _isReorderPhotosRequested; }
            set
            {
                _isReorderPhotosRequested = value;
                if (value)
                {
                    _alive.AddReorderPhotosByDataTaken(this);
                }
                else
                {
                    _alive.RemoveReorderPhotosByDataTaken(this);
                }
            }
        }

        public Photo PrimaryPhotoData;
        private readonly FlickrAlive _alive;
        public SynchroSet(string title, FlickrAlive alive)
        {
            Title = title;
            _alive = alive;
        }

        private int _up;
        private int _down;
        private bool _isSynchronizationRequested;
        public string Title { get; set; }

        public string DirectoryPath;
        public bool _isFamily;
        public bool _isPublic;
        public bool _isSearchable;
        private bool _isReorderPhotosRequested;

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
                var set = _alive.FlickrData.Sets.FirstOrDefault(s => s.Title == this.Title);
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
                IsReorderPhotosRequested = value;
            }
        }
    }
}