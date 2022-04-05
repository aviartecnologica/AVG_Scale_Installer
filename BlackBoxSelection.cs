
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
    public class BlackBoxSelection : Fragment
    {
        private SwipeRefreshLayout Swipe;
        private RecyclerView BlackBoxRecycler;
        private SwipeRefreshLayout EmptySwipe;
        private List<EBlackBoxesBlackBox> BlackBoxList;
        private ERoom SelectedRoom;

        public BlackBoxSelection(ERoom room)
        {
            SelectedRoom = room;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.BlackboxSelection, container, false);

            ((MainActivity)Activity).SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            ((MainActivity)Activity).SupportActionBar.SetDisplayShowHomeEnabled(true);

            Swipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.BlackboxSelectionSwipe);
            BlackBoxRecycler = view.FindViewById<RecyclerView>(Resource.Id.BlackboxSelectionRecycler);
            EmptySwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.BlackboxSelectionEmptySwipe);
            BlackBoxRecycler.SetLayoutManager(new LinearLayoutManager(Context));
            Swipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            EmptySwipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            Swipe.SetColorSchemeResources(Resource.Color.colorAccent);
            EmptySwipe.SetColorSchemeResources(Resource.Color.colorAccent);
            Swipe.Refresh += Blackbox_Refresh;
            EmptySwipe.Refresh += Blackbox_Refresh;

            Swipe.Refreshing = true;
            EmptySwipe.Refreshing = true;
            Blackbox_Refresh(null, null);

            return view;
        }

        private async void Blackbox_Refresh(object sender, EventArgs e)
        {
            BlackBoxList = await RequestAPI.GetBlackboxes(Data.CurrentCenter.idCenter, SelectedRoom.number);
            if(BlackBoxList == null)
            {
                BlackBoxList = new List<EBlackBoxesBlackBox>();
            }
            OnBlackboxLoaded(BlackBoxList);
        }

        private void OnBlackboxLoaded(List<EBlackBoxesBlackBox> data)
        {
            var adapter = new BlackBoxesAdapter(data);
            adapter.ItemClick += BlackBox_Click;
            BlackBoxRecycler.SetAdapter(adapter);

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

        private void BlackBox_Click(object sender, int pos)
        {
            var selected = BlackBoxList[pos];

            BlackBox blackBox = new BlackBox(selected, SelectedRoom);
            Activity.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.MainActivityFrameLayout, blackBox, null).AddToBackStack(null).Commit();
        }
    }
}
