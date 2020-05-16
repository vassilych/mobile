using System;
using System.Collections.Generic;
using CoreGraphics;
//using Firebase.Analytics;
//using Google.MobileAds;
using SplitAndMerge;
using UIKit;

namespace scripting.iOS
{
  public class AdMob : iOSVariable
  {
    static string m_appId;
    static string m_interstitialId;
    static string m_bannerId;

    /*static Interstitial m_adInterstitial;

    BannerView m_adView;
    bool viewOnScreen = false;

    public AdMob()
    { }
    public AdMob(string widgetName, UIViewController controller, CGRect rect, string arg) :
      base(UIType.CUSTOM, widgetName)
    {
      ViewX = AddBanner(controller, rect, arg);
      ViewX.Tag = ++m_currentTag;
    }

    public override iOSVariable GetWidget(string widgetType, string widgetName, string initArg, CGRect rect)
    {
      switch (widgetType) {
        case "AdMobBanner":
          return new AdMob(widgetName, AppDelegate.GetCurrentController(), rect, initArg);
      }
      return null;
    }
    public static void Init(string appId, string interstitialId = null,
                                          string bannerId = null)
    {
      m_appId = appId;
      // From Firebase.Analytics for Google AdMob:
      Firebase.Core.App.Configure();
      MobileAds.Configure(m_appId);

      if (!string.IsNullOrWhiteSpace(interstitialId)) {
        m_interstitialId = interstitialId;
        CreateAndRequestInterstitial();
      }
      if (!string.IsNullOrWhiteSpace(bannerId)) {
        m_bannerId = bannerId;
      }
    }

    public static void ShowInterstitialAd(UIViewController controller)
    {
      if (m_adInterstitial.IsReady) {
        m_adInterstitial.PresentFromRootViewController(controller);
      } else {
        Console.WriteLine("AdMob is not ready!!");
        CreateAndRequestInterstitial();
      }
    }

    static void CreateAndRequestInterstitial()
    {
      m_adInterstitial = new Interstitial(m_interstitialId);
      m_adInterstitial.ScreenDismissed += (sender, e) => {
        // Interstitial is a one time use object. That means once an interstitial is shown, HasBeenUsed 
        // returns true and the interstitial can't be used to load another ad. 
        // To request another interstitial, you'll need to create a new Interstitial object.
        m_adInterstitial.Dispose();
        CreateAndRequestInterstitial();
      };

      var request = Request.GetDefaultRequest();
      // Requests test ads on devices you specify. Your test device ID is printed to the console when
      // an ad request is made. GADBannerView automatically returns test ads when running on a
      // simulator. After you get your device ID, add it here
      request.TestDevices = new[] { Request.SimulatorId.ToString() };

      m_adInterstitial.LoadRequest(request);
    }

    public static void GetAdSize(string arg, ref int width, ref int height)
    {
      AdSize adSize = GetAdSize(arg);
      width = (int)adSize.Size.Width;
      height = (int)adSize.Size.Height;

      if (width == 0 || height == 0) {
        var screenSize = UtilsiOS.GetScreenSize();
        width = (int)screenSize.Width;
        if (screenSize.Height <= 400) {
          height = 16;
        } else if (screenSize.Height <= 720) {
          height = 25;
        } else {
          height = 45;
        }
      }
    }

    public static AdSize GetAdSize(string arg)
    {
      switch (arg) {
        case "SmartBanner":
        case "SmartBannerPortrait": return AdSizeCons.SmartBannerPortrait;
        case "SmartBannerLandscape": return AdSizeCons.SmartBannerLandscape;
        case "MediumRectangle": return AdSizeCons.MediumRectangle;
        case "Banner": return AdSizeCons.Banner;
        case "LargeBanner": return AdSizeCons.LargeBanner;
        case "FullBanner": return AdSizeCons.FullBanner;
        case "Leaderboard": return AdSizeCons.Leaderboard;
      }
      return AdSizeCons.Banner;
    }

    public BannerView AddBanner(UIViewController controller, CGRect rect, string arg)
    {
      AdSize adSize = GetAdSize(arg);
      m_adView = new BannerView(size: adSize,
                                origin: new CGPoint(rect.X, rect.Y)) {
        AdUnitID = m_bannerId,
        RootViewController = controller
      };

      // Wire AdReceived event to know when the Ad is ready to be displayed
      m_adView.AdReceived += (object sender, EventArgs e) => {
        if (!viewOnScreen) {
          controller.View.AddSubview(m_adView);
          viewOnScreen = true;
        }
      };

      var request = Request.GetDefaultRequest();
      // Requests test ads on devices you specify. Your test device ID is printed to the console when
      // an ad request is made. GADBannerView automatically returns test ads when running on a
      // simulator. After you get your device ID, add it here
      request.TestDevices = new[] { Request.SimulatorId.ToString() };

      // Request an ad
      m_adView.LoadRequest(request);
      return m_adView;
    }*/
  }
  public class InitAds : ParserFunction
  {
    static bool m_initDone;
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script, Constants.START_ARG, Constants.END_ARG,
                                         (outList) => { isList = outList; });
      Utils.CheckArgs(args.Count, 2, m_name);

      string appId = args[0].AsString();
      string interstId = Utils.GetSafeString(args, 1);
      string bannerId = Utils.GetSafeString(args, 1);

      if (!m_initDone) {
        //AdMob.Init(appId, interstId, bannerId);
        m_initDone = true;
      }

      return Variable.EmptyInstance;
    }
  }
  public class ShowInterstitial : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      //AdMob.ShowInterstitialAd(AppDelegate.GetCurrentController());
      script.MoveForwardIf(Constants.END_ARG);

      return Variable.EmptyInstance;
    }
  }
}
