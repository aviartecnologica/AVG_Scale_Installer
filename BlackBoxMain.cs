
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

namespace AVG_Scale_Installer
{
    public class BlackBoxMain : Fragment
    {
        private Button AddButton;
        private SwipeRefreshLayout Swipe;
        private RecyclerView HouseRecycler;
        private SwipeRefreshLayout EmptySwipe;
        private List<ERoom> HouseList;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.BlackboxMain, container, false);

            ((MainActivity)Activity).SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            ((MainActivity)Activity).SupportActionBar.SetDisplayShowHomeEnabled(true);

            AddButton = view.FindViewById<Button>(Resource.Id.BlackboxMainAddButton);

            Swipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.BlackboxMainSwipe);
            HouseRecycler = view.FindViewById<RecyclerView>(Resource.Id.BlackboxMainRecycler);
            EmptySwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.BlackboxMainEmptySwipe);
            HouseRecycler.SetLayoutManager(new LinearLayoutManager(Context));
            Swipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            EmptySwipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            Swipe.SetColorSchemeResources(Resource.Color.colorAccent);
            EmptySwipe.SetColorSchemeResources(Resource.Color.colorAccent);
            Swipe.Refresh += House_Refresh;
            EmptySwipe.Refresh += House_Refresh;

            //https://advancedrecyclerview.h6ah4i.com/expandable/tutorial/

            AddButton.Click += AddButton_Click;
            Swipe.Refreshing = true;
            EmptySwipe.Refreshing = true;
            House_Refresh(null, null);

            return view;
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

        private void OnHousesLoaded(List<ERoom> data)
        {
            var adapter = new HousesAdapter(data);
            adapter.ItemClick += House_Click;
            HouseRecycler.SetAdapter(adapter);

            if (data.Count == 0)
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

        private void House_Click(object sender, int pos)
        {
            var selected = HouseList[pos];

            BlackBoxSelection bl = new BlackBoxSelection(selected.number);
            Activity.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.MainActivityFrameLayout, bl, null).AddToBackStack(null).Commit();

        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            AddBlackBoxDialog dialog = new AddBlackBoxDialog();
            dialog.Show(Activity.SupportFragmentManager, null);
        }
    }
}
