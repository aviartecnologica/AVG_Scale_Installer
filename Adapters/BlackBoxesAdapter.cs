using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AVG_access_data;

namespace AVG_Scale_Installer.Adapters
{
    public class BlackBoxesAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        public IList<EBlackBoxesBlackBox> items;

        public BlackBoxesAdapter(IList<EBlackBoxesBlackBox> data)
        {
            items = data;
        }

        public override int ItemCount => items.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var item = holder as BlackBoxesViewHolder;

            item.Name.Text = items[position].name;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater
                .From(parent.Context)
                .Inflate(Resource.Layout.ItemPlaceholder, parent, false);
            var vh = new BlackBoxesViewHolder(itemView, OnClick);
            return vh;
        }

        void OnClick(int pos) => ItemClick?.Invoke(this, pos);
    }

    public class BlackBoxesViewHolder : RecyclerView.ViewHolder
    {
        public LinearLayout Parent { get; private set; }
        public TextView Name { get; private set; }

        public BlackBoxesViewHolder(View itemView, Action<int> clickListener) : base(itemView)
        {
            Parent = itemView.FindViewById<LinearLayout>(Resource.Id.ItemPlaceholderParent);
            Name = itemView.FindViewById<TextView>(Resource.Id.ItemPlaceholderText);
            itemView.Click += (sender, e) => clickListener(base.LayoutPosition);
        }
    }
}
