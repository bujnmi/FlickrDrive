using System.Linq;

namespace FlickrDrive.Tasks
{
    public class OrderSetsTask : SynchronizeTask
    {

        public override void SynchronizeImplementation(Alive alive)
        {
            var orderedSets = alive.FlickrData.Sets.OrderByDescending(s => s.Title);
            alive.FlickrInstance.PhotosetsOrderSets(orderedSets.Select(s=>s.PhotosetId).ToArray());
        }
    }
}