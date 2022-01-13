
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AVG_Scale_Installer
{
    public class Home : Fragment
    {
        private Button ScalesButton;
        private Button BlackBoxButton;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Home, container, false);

            ((MainActivity)Activity).SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            ((MainActivity)Activity).SupportActionBar.SetDisplayShowHomeEnabled(false);

            ScalesButton = view.FindViewById<Button>(Resource.Id.HomeScalesButton);
            BlackBoxButton = view.FindViewById<Button>(Resource.Id.HomeBlackboxButton);

            ScalesButton.Click += ScalesButton_Click;
            BlackBoxButton.Click += BlackBoxButton_Click;

            return view;
        }

        private void ScalesButton_Click(object sender, EventArgs e)
        {
            ScalesMain scales = new ScalesMain();
            Activity.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.MainActivityFrameLayout, scales, "ScalesMain").AddToBackStack(null).Commit();

        }

        private void BlackBoxButton_Click(object sender, EventArgs e)
        {
            BlackBoxMain blackboxes = new BlackBoxMain();
            Activity.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.MainActivityFrameLayout, blackboxes, "BlackboxMain").AddToBackStack(null).Commit();
        }
    }
}