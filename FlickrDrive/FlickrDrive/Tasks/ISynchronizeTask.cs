namespace FlickrDrive.Tasks
{
    public interface ISynchronizeTask
    {
        bool IsDone { get; }
        string AlbumTitle { get; }
        void Synchronize(Alive alive);
    }
}