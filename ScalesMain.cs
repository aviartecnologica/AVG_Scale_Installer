
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AVG_access_data;
using AVG_Scale_Installer.Adapters;
using AVG_Scale_Installer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AVG_Scale_Installer
{
    public class ScalesMain : Fragment
    {
        private Button AddButton;
        private SwipeRefreshLayout Swipe;
        private RecyclerView HouseRecycler;
        private SwipeRefreshLayout EmptySwipe;
        private List<ERoom> HouseList;

        public override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //House_Refresh(null, null);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.ScalesMain, container, false);

            ((MainActivity)Activity).SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            ((MainActivity)Activity).SupportActionBar.SetDisplayShowHomeEnabled(true);

            AddButton = view.FindViewById<Button>(Resource.Id.ScalesMainAddButton);

            Swipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.ScalesMainSwipe);
            HouseRecycler = view.FindViewById<RecyclerView>(Resource.Id.ScalesMainHouseRecycler);
            EmptySwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.ScalesMainEmptySwipe);
            HouseRecycler.SetLayoutManager(new LinearLayoutManager(Context));
            Swipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            EmptySwipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            Swipe.SetColorSchemeResources(Resource.Color.colorAccent);
            EmptySwipe.SetColorSchemeResources(Resource.Color.colorAccent);
            Swipe.Refresh += House_Refresh;
            EmptySwipe.Refresh += House_Refresh;

            AddButton.Click += AddButton_Click;
            Swipe.Refreshing = true;
            EmptySwipe.Refreshing = true;
            House_Refresh(null, null);

            return view;
        }

        private void OnHousesLoaded(List<ERoom> data)
        {
            var adapter = new HousesAdapter(data);
            adapter.ItemClick += House_Click;
            HouseRecycler.SetAdapter(adapter);

            if(data.Count == 0)
            {
                EmptySwipe.Visibility = ViewStates.Visible;
                Swipe.Visibility = ViewStates.Gone;
                EmptySwipe.Refreshing = false;
            }
            else
            {
                EmptySwipe.Visibility = ViewStates.Gone;
                Swipe.Visibility = ViewStates.Visible;
                Swipe.Refreshing = false;
            }
        }

        private async void House_Refresh(object sender, EventArgs e)
        {
            HouseList = await RequestAPI.GetRooms(Data.CurrentCenter.idCenter);
            if(HouseList == null)
            {
                HouseList = new List<ERoom>();
            }
            OnHousesLoaded(HouseList);
        }

        private void House_Click(object sender, int pos)
        {
            var selected = HouseList[pos];

            LitterSelection lit = new LitterSelection(selected);
            Activity.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.MainActivityFrameLayout, lit, null).AddToBackStack(null).Commit();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            AddScaleDialog dialog = new AddScaleDialog();
            dialog.Show(Activity.SupportFragmentManager, null);
            //Activity.SupportFragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content, dialog, "AddScaleDialog").AddToBackStack(null).Commit();
        }
    }
}