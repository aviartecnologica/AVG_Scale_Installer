
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
    public class LitterSelection : Fragment
    {
        private SwipeRefreshLayout Swipe;
        private RecyclerView LitterRecycler;
        private SwipeRefreshLayout EmptySwipe;
        private int RoomNumber;
        private List<ELitter> LitterList;

        public LitterSelection(int room)
        {
            RoomNumber = room;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.LitterSelection, container, false);

            ((MainActivity)Activity).SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            ((MainActivity)Activity).SupportActionBar.SetDisplayShowHomeEnabled(true);

            Swipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.LitterSelectionSwipe);
            LitterRecycler = view.FindViewById<RecyclerView>(Resource.Id.LitterSelectionRecycler);
            EmptySwipe = view.FindViewById<SwipeRefreshLayout>(Resource.Id.LitterSelectionEmptySwipe);
            LitterRecycler.SetLayoutManager(new LinearLayoutManager(Context));
            Swipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            EmptySwipe.SetProgressBackgroundColorSchemeResource(Resource.Color.lighter);
            Swipe.SetColorSchemeResources(Resource.Color.colorAccent);
            EmptySwipe.SetColorSchemeResources(Resource.Color.colorAccent);
            Swipe.Refresh += Litter_Refresh;
            EmptySwipe.Refresh += Litter_Refresh;

            Swipe.Refreshing = true;
            EmptySwipe.Refreshing = true;
            Litter_Refresh(null, null);

            return view;
        }

        private async void Litter_Refresh(object sender, EventArgs e)
        {
            LitterList = await RequestAPI.GetLitters(Data.CurrentCenter.idCenter, RoomNumber);
            if(LitterList == null)
            {
                LitterList = new List<ELitter>();
            }
            OnLitterLoaded(LitterList);
        }

        private void OnLitterLoaded(List<ELitter> data)
        {
            var adapter = new CurrentLitterAdapter(data);
            adapter.ItemClick += Litter_Click;
            LitterRecycler.SetAdapter(adapter);

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

        private void Litter_Click(object sender, int pos)
        {
            var selected = LitterList[pos];

            ScaleSelection scal = new ScaleSelection(RoomNumber, selected.department, selected.lot.idLot);
            Activity.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.MainActivityFrameLayout, scal, null).AddToBackStack(null).Commit();
        }
    }
}
