using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using FlickrDrive.Properties;
using FlickrDrive.Tasks;
using FlickrNet;

namespace FlickrDrive
{
    public class FlickrAlive : BaseNotifyObject
    {
        public List<Action> CancelActions;
        public FlickrData FlickrData;
        public Flickr FlickrInstance;

        public string Root
        {
            get { return _root; }
            set
            {
                if (_root != null && _root != value)
                {
                    //needs update path before updating meta
                    _root = value;
                    Settings.Default.RootPath = value;
                    Settings.Default.Save();
                    Task.Run(() =>
                    {
                        UpdateMeta();
                    });
                }
                _root = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoggedIn
        {
            get { return !string.IsNullOrEmpty(Settings.Default.oauthToken)&& !string.IsNullOrEmpty(Settings.Default.oauthTokenSecret); }
        }
        public bool IsSynchronizing
        {
            get { return _isSynchronizing; }
            set
            {
                _isSynchronizing = value;
                OnPropertyChanged();
            }
        }

        public string UserName
        {
            get { return Settings.Default.Username; }
        }

        private BasicSecurity _basicSecurity;
        private bool _isSynchronizing;
        private ObservableCollection<SynchroSet> _allSynchroSets;
        private string _root;

        public ObservableCollection<SynchroSet> AllSynchroSets
        {
            get { return _allSynchroSets; }
            set
            {
                _allSynchroSets = value;
                OnPropertyChanged();
            }
        }

        private List<SynchronizeTask> ReOrderTasks = new List<SynchronizeTask>();

        private List<SynchronizeTask> PermissionTasks = new List<SynchronizeTask>();
        private List<SynchronizeTask> PhotoTasks { get; set; }

        public int SynchronizationTasksCount
        {
            get { return PhotoTasks.Count() + PermissionTasks.Count() + ReOrderTasks.Count(); }
        }

        public int SynchronizationTasksDoneCount
        {
            get { return PhotoTasks.Where(t => t.IsDone).Count() + PermissionTasks.Where(t=>t.IsDone).Count() + ReOrderTasks.Where(t => t.IsDone).Count(); }
        }

        public string SynchronizationProgressString
        {
            get { return $"Actions made {SynchronizationTasksDoneCount} from {SynchronizationTasksCount}."; }
        }

        public string TasksString
        {
            get { return $"Actions to be made {SynchronizationTasksCount}."; }
        }

        public void Stop()
        {
            PhotoTasks.Clear();
            foreach (var cancelAction in CancelActions)
            {
                cancelAction();
            }
            UpdateMeta();
            IsSynchronizing = false;

        }
        private object sync = new object();
        public FlickrAlive()
        {
            CancelActions = new List<Action>();
            _basicSecurity = new BasicSecurity();
            FlickrInstance = new Flickr(Constants.KEY, Constants.SECRET);
            FlickrData = new FlickrData();
            AllSynchroSets = new ObservableCollection<SynchroSet>();
            BindingOperations.EnableCollectionSynchronization(AllSynchroSets, sync);
            PhotoTasks = new List<SynchronizeTask>();
        }

        public void Initialize()
        {
            var synchronizePath = Settings.Default.RootPath;
            if (string.IsNullOrEmpty(synchronizePath))
            {
                synchronizePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                                  Constants.DelimiterInWindowsPath + Constants.ProgramName;
            }

            Root = synchronizePath;
            EnsureDirectoryExist(Root);

            if (string.IsNullOrEmpty(Settings.Default.oauthTokenSecret))
            {
                Authorize();
            }
            GetAccessToken();
            
            Task.Run(() =>
            {
                UpdateMeta();
            });
        }

        public async void Authorize()
        {
            var requestToken = FlickrInstance.OAuthGetRequestToken(Constants.LOCAL_HOST_ADDRESS);
            var authorizationUrl = FlickrInstance.OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Delete);
            var p = Process.Start(authorizationUrl);
            await ListenForToken(requestToken);
            OnPropertyChanged(nameof(IsLoggedIn));
        }

        private async Task ListenForToken(OAuthRequestToken requestToken)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(Constants.LOCAL_HOST_ADDRESS);
            listener.Start();
            string oauthToken = "";
            string oauthVerifier = "";
            while (true)
            {
                var context = listener.GetContext();
                oauthToken = context.Request.QueryString.Get("oauth_token");
                oauthVerifier = context.Request.QueryString.Get("oauth_verifier");

                Debug.WriteLine(oauthToken, oauthVerifier);
                if (!string.IsNullOrEmpty(oauthToken) && !string.IsNullOrEmpty(oauthVerifier))
                {
                    //form response
                    var outputStream = context.Response.OutputStream;
                    string responseString = "<html><body><h2>Access granted</h2></body></html>";
                    var bytes = Encoding.UTF8.GetBytes(responseString);
                    context.Response.ContentLength64 = bytes.Length;
                    outputStream.Write(bytes, 0, bytes.Length);
                    outputStream.Close();
                    break;
                }

                
            }

            var access = FlickrInstance.OAuthGetAccessToken(requestToken, oauthVerifier);
            Settings.Default.Username = access.Username;
            SaveLoginToken(access.Token, access.TokenSecret);
            return;
        }

        private void GetAccessToken()
        {
            var oAuthToken = _basicSecurity.DecryptText(Settings.Default.oauthToken,
                Constants.EncryptionKey);
            var oAuthAccessTokenSecret = _basicSecurity.DecryptText(Settings.Default.oauthTokenSecret,
                Constants.EncryptionKey);

            FlickrInstance.OAuthAccessToken = oAuthToken;
            FlickrInstance.OAuthAccessTokenSecret = oAuthAccessTokenSecret;
        }

        private void SaveLoginToken(string token, string tokenSecret)
        {

            var enVerif = _basicSecurity.EncryptText(tokenSecret, Constants.EncryptionKey);
            Settings.Default.oauthTokenSecret = enVerif;

            var enToken = _basicSecurity.EncryptText(token, Constants.EncryptionKey);
            Settings.Default.oauthToken = enToken;

            Settings.Default.Save();
        }

        public void Synchronize()
        {
            Task.Run(() =>
            {
                IsSynchronizing = true;

                if(SynchronizeTasks(PhotoTasks))
                {
                    SynchronizeTasks(PermissionTasks);
                    SynchronizeTasks(ReOrderTasks);
                }                

                IsSynchronizing = false;
                PhotoTasks.Clear();
                PermissionTasks.Clear();
                ReOrderTasks.Clear();

                UpdateMeta();
            });

        }

        private int errorCounter = 0;
        private bool SynchronizeTasks(List<SynchronizeTask> tasks)
        {
            while (tasks.Count(t => !t.IsDone && t.CurrentAttempt < Constants.MaxAttemptCount) > 0)
            {
                foreach (var synchronizationTask in tasks.Where(t => !t.IsDone && t.CurrentAttempt < Constants.MaxAttemptCount))
                {
                    OnPropertyChanged(nameof(SynchronizationTasksDoneCount));
                    OnPropertyChanged(nameof(SynchronizationProgressString));
                    try
                    {
                        synchronizationTask.Synchronize(this);
                    }
                    catch (Exception e)
                    {
                        errorCounter++;
                        Debug.WriteLine($"{errorCounter} {e}");
                    }
                }
            }

            if (tasks.Any(t => !t.IsDone))
            {
                return false;
            }
            return true;
        }

        
        public void UpdateMeta()
        {
            AllSynchroSets.Clear();
            //clear old
            FlickrData.Sets = FlickrInstance.PhotosetsGetList();           
            EnsureDirectoryExist(Root);

            var Directories = Directory.GetDirectories(Root);
            foreach (var directory in Directories)
            {
                var directoryName = Path.GetFileName(directory);

                if (FlickrData.Sets.FirstOrDefault(s => s.Title == directoryName) == null)
                {
                    var setx = new SynchroSet(directoryName, this);
                    setx.Up = Directory.GetFiles(directory).FilterPhotos().Count();
                    AllSynchroSets.Add(setx);
                }
            }

            foreach (var set in FlickrData.Sets)
            {
                var setx = new SynchroSet(set.Title, this);
                var primaryPermission = FlickrInstance.PhotosGetPerms(set.PrimaryPhotoId);
                setx._isPublic = primaryPermission.IsPublic;
                setx._isFamily = primaryPermission.IsFamily;

                var testedDirectory = Root + Constants.DelimiterInWindowsPath + set.Title;
                if (!Directory.Exists(testedDirectory))
                {
                    setx.Down = set.NumberOfPhotos;
                }
                else
                {
                    var photosString = GetAllPhotos(set).Select(p=>p.Title);

                    var files = Directory.GetFiles(testedDirectory).FilterPhotos().Select(Path.GetFileNameWithoutExtension);
                    foreach (var file in files)
                    {
                        if (!photosString.Contains(file))
                        {
                            setx.Up++;
                        }
                    }

                    foreach (var photo in photosString)
                    {
                        if (!files.Contains(photo))
                        {
                            setx.Down++;
                        }
                    }
                }
                AllSynchroSets.Add(setx);
            }

            TasksCountChanged();
        }

        public List<Photo> GetAllPhotos(Photoset set)
        {
            var photos = FlickrInstance.PhotosetsGetPhotos(set.PhotosetId);
            var photosList = photos.ToList();
            for (int i = 2; i <= Math.Ceiling((double) photos.Total/Constants.MaxPerPage); i++)
            {
                photosList.AddRange(FlickrInstance.PhotosetsGetPhotos(set.PhotosetId, PhotoSearchExtras.All,
                    PrivacyFilter.None, i,
                    Constants.MaxPerPage));
            }
            return photosList;
        }

        public void AddSynchronizeSet(SynchroSet synchroSet)
        {
            string directory = Root + Constants.DelimiterInWindowsPath + synchroSet.Title;
            EnsureDirectoryExist(directory);

            var set = FlickrData.Sets.FirstOrDefault(i => i.Title == synchroSet.Title);
            if (set == null)
            {
                //create new album, only if there is at least one photo
                if (Directory.GetFiles(directory).FilterPhotos().Any())
                {
                    SynchronizeNewDirectoryUp(directory);
                }
            }
            else
            {
                SynchronizeExistingDirectory(set, directory);
            }   

            OnPropertyChanged(nameof(SynchronizationTasksCount)); 
            OnPropertyChanged(nameof(TasksString));

        }

        private void SynchronizeExistingDirectory(Photoset set, string directory)
        {
            var localFiles = Directory.GetFiles(directory).FilterPhotos();
            var photos = FlickrInstance.PhotosetsGetPhotos(set.PhotosetId);
            for (int i = 2; i <= Math.Ceiling((double)photos.Total / Constants.MaxPerPage); i++)
            {
                    var ph = FlickrInstance.PhotosetsGetPhotos(set.PhotosetId, PhotoSearchExtras.All,
                        PrivacyFilter.None, i,
                        Constants.MaxPerPage);
                    foreach (var p in ph)
                    {
                        photos.Add(p);
                    }                
            }
            foreach (var file in localFiles)
            {
                if (photos.FirstOrDefault(p => p.Title == Path.GetFileNameWithoutExtension(file)) == null)
                {
                    var task = new UploadTask(file, set.PhotosetId, set.Title);
                    PhotoTasks.Add(task);
                }
            }
            foreach (var photo in photos)
            {
                if (localFiles.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == photo.Title) == null)
                {
                    var task = new DownloadTask(photo.PhotoId, directory, set.Title);
                    PhotoTasks.Add(task);
                }
            }


        }

        private void SynchronizeNewDirectoryUp(string directory)
        {
            var files = Directory.GetFiles(directory).FilterPhotos();
            var albumTitle = Path.GetFileName(directory);
            var createAlbumTask = new CreateAlbumTask(files.First(), albumTitle);
            List<UploadTask> allFilesUp = new List<UploadTask>();
            foreach (var file in files)
            {
                if (file == files.First())
                {
                    continue;
                }
                allFilesUp.Add(new UploadTask(file, albumTitle));
            }
            createAlbumTask.PostAction = new Action(() =>
            {
                foreach (var file in allFilesUp)
                {
                    file.AlbumId = createAlbumTask.AlbumId;
                }
            });

            PhotoTasks.Add(createAlbumTask);
            PhotoTasks.AddRange(allFilesUp);
        }


        private void EnsureDirectoryExist(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void RemoveSynchronizationOfSet(SynchroSet synchroSet)
        {
            PhotoTasks.RemoveAll(t => t.AlbumTitle == synchroSet.Title);            
            TasksCountChanged();
        }

        public void Logout()
        {
            Settings.Default.oauthToken = String.Empty;
            Settings.Default.oauthTokenSecret = String.Empty;
            Settings.Default.Username = String.Empty;
            FlickrInstance.OAuthAccessToken = String.Empty;
            FlickrInstance.OAuthAccessTokenSecret = String.Empty;
            OnPropertyChanged(nameof(IsLoggedIn));
            Settings.Default.Save();
        }

        public void AddReorderPhotosByDataTaken(SynchroSet synchroSet)
        {
            Action preSynchroAction = () =>
            {
                FlickrData.Sets = FlickrInstance.PhotosetsGetList();
            };
            ReOrderTasks.Add(new ActionTask(preSynchroAction, synchroSet.Title));
            ReOrderTasks.Add(new OrderPhotosTask(synchroSet.Title));
            TasksCountChanged();

        }

        public void RemoveReorderPhotosByDataTaken(SynchroSet synchroSet)
        {
            ReOrderTasks.RemoveAll(s => s.AlbumTitle == synchroSet.Title);
            TasksCountChanged();

        }


        public void AddPermToSynchroSet(SynchroSet synchroSet)
        {
            PermissionTasks.RemoveAll(t => t.AlbumTitle == synchroSet.Title);

            Action preSynchroAction = () =>
            {
                FlickrData.Sets = FlickrInstance.PhotosetsGetList();
            };
            PermissionTasks.Add(new ActionTask(preSynchroAction, synchroSet.Title));

            List<Photo> photos = null;
            Action changePermissionAction = new Action(() =>
            {              
                var set = FlickrData.Sets.First(s => s.Title == synchroSet.Title);
                photos = GetAllPhotos(set);
                foreach (var photo in photos)
                {
                    FlickrInstance.PhotosSetPerms(photo.PhotoId, synchroSet.IsPublic, photo.IsFriend, synchroSet.IsFamily, PermissionComment.FriendsAndFamily, PermissionAddMeta.Owner);
                }
            });
            PermissionTasks.Add(new ActionTask(changePermissionAction, synchroSet.Title));
            TasksCountChanged();

        }

        public void ReorderSets()
        {
            if (!ReOrderTasks.Any(x => x is OrderSetsTask))
            {
                ReOrderTasks.Add(new OrderSetsTask());
            }
            TasksCountChanged();
        }

        private void TasksCountChanged()
        {
            OnPropertyChanged(nameof(SynchronizationTasksCount));
            OnPropertyChanged(nameof(TasksString));
        }
    }
}