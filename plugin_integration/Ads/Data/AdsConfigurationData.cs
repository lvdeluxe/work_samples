using System.Collections.Generic;
using Hibernum.Data;
using ZeroFormatter;

namespace Hibernum.BruceLee.Data
{
    [ZeroFormattable]
    [DataCategory("Ads/Config")]
    public class AdsConfigurationData:DataRecordBase
    {
        [Index(KStartIndex + 0)]
        public virtual IDictionary<DataRecordRef<RewardData>, float> MysteryAdRewardsTable { get; set; }
    }
}