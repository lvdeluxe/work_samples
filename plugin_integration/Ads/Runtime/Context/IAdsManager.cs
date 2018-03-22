using Hibernum.BruceLee.Context;

namespace Hibernum.BruceLee.Ads
{
    public interface IAdsManager:IContextBindable
    {
        bool IsVideoAvailable(AdPlacementType type);
    }
}