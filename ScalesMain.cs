
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AVG_Scale_Installer
{
    public class ScalesMain : Fragment
    {
        private Button AddButton;
        private RecyclerView HouseRecycler;
        private TextView NoDataText;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.ScalesMain, container, false);

            ((MainActivity)Activity).SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            ((MainActivity)Activity).SupportActionBar.SetDisplayShowHomeEnabled(true);

            AddButton = view.FindViewById<Button>(Resource.Id.ScalesMainAddButton);
            HouseRecycler = view.FindViewById<RecyclerView>(Resource.Id.ScalesMainHouseRecycler);
            NoDataText = view.FindViewById<TextView>(Resource.Id.ScalesMainNoDataTextView);

            AddButton.Click += AddButton_Click;

            return view;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            AddScaleDialog dialog = new AddScaleDialog();
            Activity.SupportFragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content, dialog, "AddScaleDialog").AddToBackStack(null).Commit();
        }
    }
}