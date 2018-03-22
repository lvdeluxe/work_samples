using Hibernum.BruceLee.Ads;
using Hibernum.Data;
using ZeroFormatter;

namespace Hibernum.BruceLee.Data
{
    [ZeroFormattable]
    [DataCategory("Ads")]
    public class AdPlacementData:DataRecordBase
    {
        [Index(KStartIndex + 0)]
        public virtual string PlacementName { get; set; }
        
        [Index(KStartIndex + 1)]
        public virtual string Description { get; set; }

        [Index(KStartIndex + 2)]
        public virtual AdPlacementType PlacementType { get; set; }

        [Index(KStartIndex + 3)]
        public virtual UnityObjectRef<UnityEngine.Sprite> Icon { get; set; }

        [Index(KStartIndex + 4)]
        public virtual float Duration { get; set; }
        
        [Index(KStartIndex + 5)]
        public virtual float CoolDownTime { get; set; }
        
        [Index(KStartIndex + 6)]
        public virtual int MaxPerDay { get; set; }
        
        [Index(KStartIndex + 7)]
        public virtual bool ShowIncentive { get; set; }
        
        [Index(KStartIndex + 8)]
        public virtual bool ShowReward { get; set; }
    }
}
