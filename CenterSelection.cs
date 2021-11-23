
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using AVG_access_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVG_Scale_Installer
{
    public class CenterSelection : Fragment
    {
        private object GetCenters;
        private List<ECenter> CentersList;
        private CentersAdapter Adapter;
        private RecyclerView Recycler;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            GetCenters = Task.Run(() =>
            {
                CentersList = new List<ECenter>()
                {
                    new ECenter(){ coding = "ES1", name = "prueba1"},
                    new ECenter(){coding = "ES2", name = "prueba2"},
                    new ECenter(){ coding = "ES3", name = "prueba3"}
                };
            });
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.CenterSelection, container, false);

            ((MainActivity)Activity).SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            ((MainActivity)Activity).SupportActionBar.SetDisplayShowHomeEnabled(false);
            ((MainActivity)Activity).SupportActionBar.Title = "Seleccion centro";

            Recycler = view.FindViewById<RecyclerView>(Resource.Id.CenterSelectionRecycler);
            Recycler.SetLayoutManager(new LinearLayoutManager(Context));

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        public async override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            //Gif carga on

            //Gif carga off
            Adapter = new CentersAdapter(CentersList);
            Adapter.ItemClick += Center_ItemClick;
            Recycler.SetAdapter(Adapter);
        }

        private void Center_ItemClick(object sender, int pos)
        {
            var center = CentersList[pos];
        }
    }
}