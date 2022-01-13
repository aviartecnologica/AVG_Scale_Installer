using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AVG_access_data;

namespace AVG_Scale_Installer.Adapters
{
    public class RoomsAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        public IList<Room> items;

        public RoomsAdapter(IList<Room> data)
        {
            items = data;
        }

        public override int ItemCount => items.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var item = holder as RoomsViewHolder;

            if (items[position].Selected)
            {
                item.Parent.SetBackgroundResource(Resource.Drawable.selection_item_selected);
            }
            else
            {
                item.Parent.SetBackgroundResource(Resource.Drawable.selection_item);
            }
            item.Name.Text = items[position].House.name;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater
                .From(parent.Context)
                .Inflate(Resource.Layout.ItemPlaceholder, parent, false);
            var vh = new RoomsViewHolder(itemView, OnClick);
            return vh;
        }

        void OnClick(int pos) => ItemClick?.Invoke(this, pos);
    }

    public class RoomsViewHolder : RecyclerView.ViewHolder
    {
        public LinearLayout Parent { get; private set; }
        public TextView Name { get; private set; }

        public RoomsViewHolder(View itemView, Action<int> clickListener) : base(itemView)
        {
            Parent = itemView.FindViewById<LinearLayout>(Resource.Id.ItemPlaceholderParent);
            Name = itemView.FindViewById<TextView>(Resource.Id.ItemPlaceholderText);
            itemView.Click += (sender, e) => clickListener(base.LayoutPosition);
        }
    }

    public class Room
    {
        public ERoom House { get; set; }
        public bool Selected { get; set; }

        public Room(ERoom r, bool s)
        {
            House = r;
            Selected = s;
        }
    }
}
