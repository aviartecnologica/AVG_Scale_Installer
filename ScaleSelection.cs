
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
    public class ScaleSelection : Fragment
    {
        private SwipeRefreshLayout Swipe;
        private RecyclerView ScaleRecycler;
        private SwipeRefreshLayout EmptySwipe;
        private int RoomNumber;
        private int LitterDepartment;
        private int LitterLot;
        private List<EBlackBoxesScale> ScaleList;

        public ScaleSelection(int room, int dep, int lot)
        {
            RoomNumber = room;
            LitterDepartment = dep;
            LitterLot = lot;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.ScaleSelection, container, false);

            ((MainActivity)Activity).SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            ((MainActivity)Activity).SupportActionBar.SetDisplayShowHomeEnabled(true);

            Swipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.ScaleSelectionSwipe);
            ScaleRecycler = view.FindViewById<RecyclerView>(Resource.Id.ScaleSelectionRecycler);
            EmptySwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.ScaleSelectionEmptySwipe);
            ScaleRecycler.SetLayoutManager(new LinearLayoutManager(Context));
            Swipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            EmptySwipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            Swipe.SetColorSchemeResources(Resource.Color.colorAccent);
            EmptySwipe.SetColorSchemeResources(Resource.Color.colorAccent);
            Swipe.Refresh += Scale_Refresh;
            EmptySwipe.Refresh += Scale_Refresh;

            Swipe.Refreshing = true;
            EmptySwipe.Refreshing = true;
            Scale_Refresh(null, null);

            return view;
        }

        private async void Scale_Refresh(object sender, EventArgs e)
        {
            ScaleList = await RequestAPI.GetScales(Data.CurrentCenter.idCenter, RoomNumber, LitterDepartment, LitterLot);
            if(ScaleList == null)
            {
                ScaleList = new List<EBlackBoxesScale>();
            }
            OnScaleLoaded(ScaleList);
        }

        private void OnScaleLoaded(List<EBlackBoxesScale> data)
        {
            var adapter = new ScalesAdapter(data);
            adapter.ItemClick += Scale_Click;
            ScaleRecycler.SetAdapter(adapter);

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

        private void Scale_Click(object sender, int pos)
        {
            var selected = ScaleList[pos];
        }
    }
}
