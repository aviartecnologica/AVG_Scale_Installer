using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;

namespace AVG_Scale_Installer
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MainActivity);

            Toolbar mToolbar = FindViewById<Toolbar>(Resource.Id.MainActivityToolbar);
            SetSupportActionBar(mToolbar);

            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.MainActivityFrameLayout, new CenterSelection(), "CenterSelection").Commit();
        }
    }
}