using Hibernum.BruceLee.Context;

namespace Hibernum.BruceLee.Ads
{
	public interface IAdsDispatcher : IDispatcher
	{
		ContextEventHandler<AdPlacementType> OnRequest { get; set; }
		ContextEventHandler<AdPlacementType> OnClickAccept { get; set; }
		ContextEventHandler<AdPlacementType> OnClickRefuse { get; set; }
		ContextEventHandler<AdPlacementType> OnClickError { get; set; }
		ContextEventHandler<AdPlacementType> OnReward { get; set; }
		ContextEventHandler<string> OnRewardVideoComplete { get; set; }
		ContextEventHandler<bool> OnAvailabilityChanged { get; set; }
		ContextEventHandler OnServiceError { get; set; }
	}
}