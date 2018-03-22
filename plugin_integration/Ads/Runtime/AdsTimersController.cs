using System.Collections;
using Hibernum.BruceLee.Core;
using UnityEngine.Events;
using UnityEngine;
using System.Collections.Generic;
using Hibernum.BruceLee.Data;
using Hibernum.BruceLee.Persistence;

namespace Hibernum.BruceLee.Ads
{
	#region Unity Events
	public class AdTimerStartEvent:UnityEvent<AdPlacementType>{}
	public class AdTimerCompleteEvent:UnityEvent<AdPlacementType>{}
	public class AdTimerUpdateEvent:UnityEvent<AdPlacementType>{}
	#endregion
	
	public class AdsTimersController: BehaviourBase, IController
	{
		[HideInInspector]
		public AdTimerStartEvent OnTimerStart = new AdTimerStartEvent();
		[HideInInspector]
		public AdTimerCompleteEvent OnTimerComplete = new AdTimerCompleteEvent();
		[HideInInspector]
		public AdTimerUpdateEvent OnTimerUpdate = new AdTimerUpdateEvent();

		private Dictionary<AdPlacementType, Timer> _timers = new Dictionary<AdPlacementType, Timer>();
		private Dictionary<AdPlacementType, AdPlacementData> _adPlacements;
		private Dictionary<AdPlacementType, AdInfo.ActiveAdInfo> _cooldownAds = new Dictionary<AdPlacementType, AdInfo.ActiveAdInfo>();
		private AdInfo _adInfo;
		private Coroutine _cooldownCoroutine;
		private IAdsDispatcher _dispatcher;
		
		private readonly WaitForSeconds _cooldownWait = new WaitForSeconds(1f);
		private List<AdPlacementType> _cooldownCompleteItems = new List<AdPlacementType>();
		
#region public Methods

		public void Initialize(IAdsDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
			_dispatcher.OnClickRefuse  += HandleRefuse;
			_dispatcher.OnReward  += HandleReward;
		}

		private void HandleRefuse(AdPlacementType type)
		{
			if (IsTimerRunning(type))
			{
				ResetTimer(type);
			}
		}

		private void HandleReward(AdPlacementType type)
		{
			if (IsTimerRunning(type))
			{
				ResetTimer(type);
			}
		}
		
		public void SetTimers(IEnumerable<AdPlacementData> adPlacements, AdInfo adInfo)
		{
			_timers.Clear();
			_adPlacements = new Dictionary<AdPlacementType, AdPlacementData>();
			foreach (var adPlacementData in adPlacements)
			{	
				_adPlacements.Add(adPlacementData.PlacementType, adPlacementData);
			}

			_adInfo = adInfo;
			foreach (var adPlacementData in _adPlacements.Values)
			{
				if (adPlacementData.Duration != 0)
				{
					var activePlacement = _adInfo.GetActiveAd(adPlacementData.PlacementType);
					Timer t = null;
					var now = TimeUtil.UtcTimeInSecondsSinceEpoch();
					if (activePlacement != null && activePlacement.AvailabilityTime != 0)
					{
						var elapsed = (float) (now - activePlacement.AvailabilityTime);
						
						{
							var time = 0f;
							bool cooldown = false;
					
							while (time < elapsed)
							{
								time += cooldown ? adPlacementData.CoolDownTime : adPlacementData.Duration;
								cooldown = !cooldown;
							}
							var remaining = time - elapsed;
							
							cooldown = !cooldown;
							
							if (cooldown)
							{
								SetCooldown(adPlacementData.PlacementType);
							}
							else
							{
								t = new Timer(remaining);
							}
						}
					}
					else
					{
						t = new Timer(adPlacementData.Duration);
						_adInfo.AddActiveAd(new AdInfo.ActiveAdInfo()
						{
							PlacementType = adPlacementData.PlacementType,
							AvailabilityTime = now,
							CooldownTime = now + adPlacementData.Duration
						});
					}

					if (t != null)
					{
						t.Reset();
						_timers.Add(adPlacementData.PlacementType, t);
						OnTimerStart.Invoke(adPlacementData.PlacementType);
					}
				}
			}
		}

		public void StopTimer(AdPlacementType placementType)
		{
			if (_timers.ContainsKey(placementType))
			{
				_timers[placementType].Pause();
			}
		}

		public void ResetTimer(AdPlacementType placementType)
		{
			if (_timers.ContainsKey(placementType))
			{
				RaiseTimerComplete(placementType);
			}
		}

		public bool IsTimerRunning(AdPlacementType placementType)
		{
			return _timers.ContainsKey(placementType) && 
				!_timers[placementType].IsDone;
		}

		public float GetRemainingTime(AdPlacementType placementType)
		{
			var adPlacementData = _adPlacements[placementType];
			foreach (var kvp in _timers)
			{
				if (kvp.Key == placementType)
				{
					var remaining = kvp.Value.Length - kvp.Value.Elapsed;
					
					if(remaining <= 0f)
					{
						remaining = 0f;
						RaiseTimerComplete(adPlacementData.PlacementType);
					}
					return remaining;
				}
			}
			return -1f;
		}
		
#endregion
		
#region Private Methods
		
		private void RaiseTimerComplete(AdPlacementType placementType)
		{
			var now = TimeUtil.UtcTimeInSecondsSinceEpoch();
			var activeAd = _adInfo.GetActiveAd(placementType);
			activeAd.CooldownTime = now;
			activeAd.AvailabilityTime = now + _adPlacements[placementType].CoolDownTime;
			_adInfo.ReplaceActiveAd(activeAd);
			OnTimerComplete.Invoke(placementType);
			_timers.Remove(placementType);
			SetCooldown(placementType);
		}

		private void SetCooldown(AdPlacementType placementType)
		{
			var activeAdInfo = _adInfo.GetActiveAd(placementType);
			_cooldownAds.Add(placementType, activeAdInfo);
			if (_cooldownCoroutine == null)
			{
				_cooldownCoroutine = StartCoroutine(WaitForCoolDown());
			}
		}
		

		private IEnumerator WaitForCoolDown()
		{
			while (_cooldownAds.Count != 0)
			{
				var now = TimeUtil.UtcTimeInSecondsSinceEpoch();
				_cooldownCompleteItems.Clear();
				foreach (var kvp in _cooldownAds)
				{
					var adPlacementData = _adPlacements[kvp.Value.PlacementType];
					
					var elapsed = now - kvp.Value.AvailabilityTime;
					var time = 0f;
					bool cooldown = true;
					
					while (time < elapsed)
					{
						time += cooldown ? adPlacementData.CoolDownTime : adPlacementData.Duration;
						cooldown = !cooldown;
					}
					
					if (!cooldown)
					{
						var adInfo = kvp.Value;
						adInfo.AvailabilityTime = now;
						adInfo.CooldownTime = now + adPlacementData.Duration;
						_adInfo.ReplaceActiveAd(adInfo);
						
						_cooldownCompleteItems.Add(kvp.Value.PlacementType);
						var t = new Timer();
						t.Length = adPlacementData.Duration;
						t.Reset();
						_timers.Add(kvp.Value.PlacementType, t);
						OnTimerStart.Invoke(kvp.Value.PlacementType);
					}
				}
				
				foreach (var adPlacementType in _cooldownCompleteItems)
				{
					_cooldownAds.Remove(adPlacementType);
				}

				yield return _cooldownWait;
			}
			_cooldownCoroutine = null;
		}
		#endregion
	}
}

