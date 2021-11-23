using Android;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AVG_access_data;
using System;
using System.Collections.Generic;

namespace AVG_Scale_Installer
{
    class CentersAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        public List<ECenter> items;

        public CentersAdapter(List<ECenter> data)
        {
            items = data;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).
                Inflate(Resource.Layout.CenterPlaceholder, parent, false);
            var vh = new CentersViewHolder(itemView, OnClick);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = viewHolder as CentersViewHolder;

            item.Name.Text = items[position].coding + " - " + items[position].name;
        }

        public override int ItemCount => items.Count;

        void OnClick(int pos) => ItemClick?.Invoke(this, pos);

    }

    public class CentersViewHolder : RecyclerView.ViewHolder
    {
        public LinearLayout Layout { get; private set; }
        public TextView Name { get; private set; }

        public CentersViewHolder(View itemView, Action<int> clickListener) : base(itemView)
        {
            Layout = itemView.FindViewById<LinearLayout>(Resource.Id.CenterPlaceholderLayout);
            Name = itemView.FindViewById<TextView>(Resource.Id.CenterPlaceholderName);

            itemView.Click += (sender, e) => clickListener(base.LayoutPosition);
        }
    }
}