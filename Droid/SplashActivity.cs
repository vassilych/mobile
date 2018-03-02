using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace scripting.Droid
{
  [Activity(Theme = "@style/MyTheme.Splash",
            MainLauncher = true,
            Icon = "@mipmap/icon",
            ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
            //ScreenOrientation = ScreenOrientation.Portrait,
            NoHistory = true)]
  public class SplashActivity : Activity //AppCompatActivity
  {
    protected override void OnCreate(Bundle savedInstanceState)
    {
      base.OnCreate(savedInstanceState);

      //StartActivity(typeof(MainActivity));
    }
    // Launches the startup task
    protected override void OnResume()
    {
      base.OnResume();
      StartActivity(new Intent(Application.Context, typeof(MainActivity)));
    }

    // Prevent the back button from canceling the startup process
    public override void OnBackPressed() { }
}
}
