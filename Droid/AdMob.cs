using System;
using System.Collections.Generic;
using Android.Content;
using Android.Gms.Ads;
using Android.Views;
using SplitAndMerge;

namespace scripting.Droid
{
  public class AdMob : DroidVariable
  {
    static InterstitialAd m_interstitialAd;
    //AdRequest.Builder adRequestBuilder = new AdRequest.Builder();

    static string m_appId;
    static string m_interstitialId;
    static string m_bannerId;
    static string m_testDevice = "39C1A5B92F1F1F3D99A66FDF86EE30AF";

    class AdListener : Android.Gms.Ads.AdListener
    {
      public override void OnAdClosed()
      {
        RequestNewInterstitial();
      }
    }
    public AdMob()
    { }
    public AdMob(string widgetName, Context context, string arg) :
      base(UIType.CUSTOM, widgetName)
    {
      ViewX = AddBanner(context, arg);
      ViewX.Tag = ++m_currentTag;
      ViewX.Id  = m_currentTag;
    }
    public override DroidVariable GetWidget(string widgetType, string widgetName, string initArg,
                                            int width, int height)
    {
      switch (widgetType) {
        case "AdMobBanner":
          return new AdMob(widgetName, MainActivity.TheView, initArg);
      }
      return null;
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
        case "SmartBannerPortrait": return AdSize.SmartBanner;
        case "MediumRectangle": return AdSize.MediumRectangle;
        case "Banner": return AdSize.Banner;
        case "LargeBanner": return AdSize.LargeBanner;
        case "FullBanner": return AdSize.FullBanner;
        case "Leaderboard": return AdSize.Leaderboard;
      }
      return AdSize.Banner;
    }
    public static View AddBanner(Context context, string arg)
    {
      AdView adView = new AdView(context);
      adView.AdUnitId = m_bannerId;
      adView.AdSize = GetAdSize(arg);

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
  public class InitAds : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2, m_name);

      string appId = args[0].AsString();
      string interstId = Utils.GetSafeString(args, 1);
      string bannerId  = Utils.GetSafeString(args, 2);

      AdMob.Init(MainActivity.TheView, appId, interstId, bannerId);

      return Variable.EmptyInstance;
    }
  }
  public class ShowInterstitial : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      AdMob.ShowInterstitialAd();
      script.MoveForwardIf(Constants.END_ARG);
      return Variable.EmptyInstance;
    }
  }
}
