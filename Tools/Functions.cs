using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AVG_Scale_Installer.Tools
{
    public static class Functions
    {
        public static bool LoadingOpened = false;

        public static void HideKeyboard(FragmentActivity activity)
        {
            try
            {
                InputMethodManager inm = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
                inm.HideSoftInputFromWindow(activity.CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
            }
            catch(Exception ex)
            {

            }
        }

        public static void Loading(bool open)
        {
            try
            {
                if(!open && LoadingOpened)
                {
                    LoadingOpened = false;
                    ((Android.Support.V4.App.DialogFragment)Data.CurrentFM.FindFragmentByTag("waiting")).Dismiss();
                }
                if(open && !LoadingOpened)
                {
                    DoingWork connection = new DoingWork();
                    connection.Cancelable = false;
                    connection.Show(Data.CurrentFM, "waiting");
                    LoadingOpened = true;
                }
            }
            catch(Exception ex)
            {
                
            }
        }
    }
}