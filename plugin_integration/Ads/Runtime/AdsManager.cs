using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hibernum.BruceLee.Audio;
using Hibernum.BruceLee.Context;
using Hibernum.BruceLee.Data;
using Hibernum.BruceLee.GameTime;
using Hibernum.BruceLee.Jobs;
using Hibernum.BruceLee.Persistence;
using Hibernum.BruceLee.Profile;
using Hibernum.BruceLee.Tools;
using Hibernum.BruceLee.UI;
using Hibernum.Unity.BuildTools;
using UnityEngine;

namespace Hibernum.BruceLee.Ads
{
    public enum AdPlacementType
    {
        None,
        Revive,
        MysteryItem,
        DailyFlask,
        Meditation,
        DailyReward,
        LevelUp,
        Shop,
        BrawlEnd,
        WarStart,
        OfflineLoot
    }

    [RequireComponent(typeof(AdsTimersController))]
    public class AdsManager : ManagerBehaviourBase, IAdsManager
    {
        [SerializeField, HideInInspector, Inject]
        private AdsTimersController _timersController;
        private IAdsDispatcher _adsDispatcher;
        
        private IEnumerable<AdPlacementData> _adPlacements;
        
        private IAudioManager _audioManager;
        private IProfileDispatcher _profileDispatcher;
        private AdInfo _adInfo;
        private ITimeDispatcher _timeDispatcher;

        public AdInfo AdInfo
        {
            get { return _adInfo; }
        }

        #region IManager implementation

        public override IEnumerator<JobInstruction> Initialize(IContext context)
        {
            IronSource.Agent.shouldTrackNetworkState (true);
            IronSource.Agent.init(BuildInfo.IronSourceAppKey, IronSourceAdUnits.REWARDED_VIDEO);
            _timeDispatcher = context.GetDispatcher<ITimeDispatcher>();
            _adsDispatcher = context.GetDispatcher<IAdsDispatcher>();
            _profileDispatcher = context.GetDispatcher<IProfileDispatcher>();
            _profileDispatcher.OnLoad += HandleProfileLoad;
            _profileDispatcher.OnProfileReloaded += HandleProfileLoad;
            _timersController.Initialize(_adsDispatcher);
            UIManager.Instance.RegisterController(_timersController);
            _audioManager = context.GetBinding<IAudioManager>();
            AddListeners();
            yield break;
        }

        public override void Run(IContext context)
        {
            TrySetTimers();
        }

        private void HandleProfileLoad(ProfileInfo info)
        {
            _adInfo = info.AdInfo;
            TrySetTimers();
        }

        private void TrySetTimers()
        {
            if(_adPlacements != null && _adInfo != null)
                _timersController.SetTimers(_adPlacements, _adInfo);
        }

        public override IEnumerator<JobInstruction> Load(IContext context)
        {
            _adPlacements = context.GetBinding<IDataManager>().Database.GetRecords<AdPlacementData>();
            yield break;
        }

        #endregion

        public bool IsVideoAvailable(AdPlacementType type)
        {
            bool avail;
            var placement = GetPlacementByType(type);
            
            switch (type)
            {
                default:
                    avail = CommonConditions(placement.PlacementName);
                    break;
                case AdPlacementType.MysteryItem:
                    avail = CommonConditions(placement.PlacementName) && _timersController.IsTimerRunning(AdPlacementType.MysteryItem);
                    break;
            }
            return avail;
        }

        private bool CommonConditions(string placementName)
        {
#if UNITY_EDITOR
            return true;
#else
            return IronSource.Agent.isRewardedVideoAvailable() &&  !IronSource.Agent.isRewardedVideoPlacementCapped(placementName);
#endif
        }

        private AdPlacementData GetPlacementByType(AdPlacementType type)
        {
            return _adPlacements.FirstOrDefault(x=>x.PlacementType == type);
        }

        private void HandleAccept(AdPlacementType type)
        {
            _timeDispatcher.OnGameplayPause.Invoke(true);
            _timersController.StopTimer(type);
            var placement = GetPlacementByType(type);
            _audioManager.Mute(true);
#if UNITY_EDITOR
            StartCoroutine(WaitBeforeEditorCallback(placement.PlacementName));
#else
            if (placement != null && CommonConditions(placement.PlacementName))
            {
                IronSource.Agent.showRewardedVideo(placement.PlacementName);
            }
            else
            {
                var error = new IronSourceError(-1, "Internal error");
                HandleRewardedVideoAdShowFailed(error);
            }
#endif
        }

        private IEnumerator WaitBeforeEditorCallback(string placementName)
        {
            yield return new WaitForSeconds(0.1f);
            HandleRewardedVideoAdRewarded(new IronSourcePlacement(placementName, string.Empty, 1));
        }

        private void AddListeners()
        {
            _adsDispatcher.OnClickAccept  += HandleAccept;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += HandleRewardedVideoAvailabilityChanged;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += HandleRewardedVideoAdRewarded;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent += HandleRewardedVideoAdShowFailed;
        }

        private void RemoveListeners()
        {
            _adsDispatcher.OnClickAccept  -= HandleAccept;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent -= HandleRewardedVideoAvailabilityChanged;
            IronSourceEvents.onRewardedVideoAdRewardedEvent -= HandleRewardedVideoAdRewarded;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent -= HandleRewardedVideoAdShowFailed;
        }

        private void HandleRewardedVideoAvailabilityChanged(bool available)
        {
            _adsDispatcher.OnAvailabilityChanged.Invoke(available);
        }

        private void HandleRewardedVideoAdRewarded(IronSourcePlacement placement)
        {
            _audioManager.Mute(false);
            _adsDispatcher.OnRewardVideoComplete.Invoke(placement.getPlacementName());
            _timeDispatcher.OnGameplayPause.Invoke(false);
        }

        private void HandleRewardedVideoAdShowFailed(IronSourceError error)
        {
            _audioManager.Mute(false);
            _adsDispatcher.OnServiceError.Invoke();
            _timeDispatcher.OnGameplayPause.Invoke(false);
        }

        #region MonoBehaviour Methods

        private void OnApplicationPause(bool isPaused)
        {
            IronSource.Agent.onApplicationPause(isPaused);
        }

        #endregion
    }
}