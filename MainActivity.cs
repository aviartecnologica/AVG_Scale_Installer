
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AVG_Scale_Installer.Tools;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace AVG_Scale_Installer
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class MainActivity : AppCompatActivity
    {
        private ISharedPreferences Prefs;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MainActivity);

            Prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            Toolbar mToolbar = FindViewById<Toolbar>(Resource.Id.MainActivityToolbar);
            SetSupportActionBar(mToolbar);

            Data.Server = Prefs.GetString("server", "");

            Data.CurrentCenter = await RequestAPI.GetCurrentCenter();
            if(Data.CurrentCenter == null)
            {
                Toast.MakeText(this, "ERROR", ToastLength.Short).Show();
                Intent intent = new Intent(this, typeof(InitialActivity));
                Prefs.Edit().PutString("server", "").Apply();
                FinishAffinity();
                StartActivity(intent);
            }

            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.MainActivityFrameLayout, new Home(), "Home").Commit();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.Toolbar, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_logout:
                    Intent intent = new Intent(this, typeof(InitialActivity));
                    Prefs.Edit().PutString("server", "").Apply();
                    FinishAffinity();
                    StartActivity(intent);

                    break;

                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}