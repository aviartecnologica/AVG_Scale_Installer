using Android.Net.Wifi;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AVG_Scale_Installer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AVG_Scale_Installer.Adapters
{
    class WifisAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        public IList<Network> items;

        public WifisAdapter(IList<Network> data)
        {
            items = data;
        }

        public override long GetItemId(int position)
        {
            return base.GetItemId(position);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater
                .From(parent.Context)
                .Inflate(Resource.Layout.WifiPlaceholder, parent, false);
            var vh = new WifisViewHolder(itemView, OnClick);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = viewHolder as WifisViewHolder;

            if (items[position].Selected)
            {
                item.Parent.SetBackgroundResource(Resource.Drawable.selection_item_selected);
            }
            else
            {
                item.Parent.SetBackgroundResource(Resource.Drawable.selection_item);
            }
            item.Name.Text = items[position].ScanResult.Ssid;
        }

        public override int ItemCount => items.Count;

        void OnClick(int pos) => ItemClick?.Invoke(this, pos);
    }

    public class WifisViewHolder : RecyclerView.ViewHolder
    {
        public LinearLayout Parent { get; private set; }
        public TextView Name { get; private set; }

        public WifisViewHolder(View itemView, Action<int> clickListener) : base(itemView)
        {
            Parent = itemView.FindViewById<LinearLayout>(Resource.Id.WifiPlaceholderParent);
            Name = itemView.FindViewById<TextView>(Resource.Id.WifiPlaceholderName);
            itemView.Click += (sender, e) => clickListener(base.LayoutPosition);
        }

    }

    public class Network
    {
        public ScanResult ScanResult { get; set; }
        public bool Selected { get; set; }

        public Network(ScanResult sr, bool selected)
        {
            ScanResult = sr;
            Selected = selected;
        }
    }
}