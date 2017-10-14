using System;
using Android.Content;
using Android.Gms.Ads;
using Android.Views;

namespace scripting.Droid
{
    public class AdMob
    {
        static InterstitialAd m_interstitialAd;
        //AdRequest.Builder adRequestBuilder = new AdRequest.Builder();

        static string m_appId;
        static string m_interstitialId;
        static string m_bannerId;
        static string m_testDevice     = "39C1A5B92F1F1F3D99A66FDF86EE30AF";

        class AdListener : Android.Gms.Ads.AdListener
        {
            public override void OnAdClosed()
            {
                RequestNewInterstitial();
            }
        }
        public static void Init(Context context, string appId,
                                string interstitialId = null,
                                string bannerId = null)
        {
            m_appId = appId;
            m_interstitialId = interstitialId;
            m_bannerId = bannerId;

            MobileAds.Initialize(context, m_appId);

            //adRequestBuilder.AddTestDevice(AdRequest.DeviceIdEmulator);
            //adRequestBuilder.AddTestDevice(m_testDevice);

            if (!string.IsNullOrWhiteSpace(interstitialId)) {
                m_interstitialAd = new InterstitialAd(context);
                m_interstitialAd.AdUnitId = interstitialId;

                m_interstitialAd.AdListener = new AdListener();
                RequestNewInterstitial();
            }
        }

        public static AdSize GetAdSize(string arg)
        {
            switch (arg) {
                case "SmartBanner":
                case "SmartBannerLandscape":
                case "SmartBannerPortrait":  return AdSize.SmartBanner;
                case "MediumRectangle":      return AdSize.MediumRectangle;
                case "Banner":               return AdSize.Banner;
                case "LargeBanner":          return AdSize.LargeBanner;
                case "FullBanner":           return AdSize.FullBanner;
                case "Leaderboard":          return AdSize.Leaderboard;
            }
            return AdSize.Banner;
        }
        public static View AddBanner(Context context, string arg)
        {
            AdView adView   = new AdView(context);
            adView.AdUnitId = m_bannerId;
            adView.AdSize   = GetAdSize(arg);

            var adRequest = new AdRequest.Builder().
                                AddTestDevice(AdRequest.DeviceIdEmulator).
                                AddTestDevice(m_testDevice).
                                Build();

            adView.LoadAd(adRequest);
            return adView;
        }

        static void RequestNewInterstitial()
        {
            var adRequest = new AdRequest.Builder().
                                AddTestDevice(AdRequest.DeviceIdEmulator).
                                AddTestDevice(m_testDevice).
                                Build();

            m_interstitialAd.LoadAd(adRequest);
        }
        public static void ShowInterstitialAd()
        {
            if (m_interstitialAd.IsLoaded) {
                m_interstitialAd.Show();
            } else {
                RequestNewInterstitial();
            }
        }
    }
}
