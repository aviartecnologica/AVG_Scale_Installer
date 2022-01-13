using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AVG_access_data;

namespace AVG_Scale_Installer.Adapters
{
    public class LittersAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        public IList<Department> items;

        public LittersAdapter(IList<Department> data)
        {
            items = data;
        }

        public override int ItemCount => items.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var item = holder as LittersViewHolder;

            if (items[position].Selected)
            {
                item.Parent.SetBackgroundResource(Resource.Drawable.selection_item_selected);
            }
            else
            {
                item.Parent.SetBackgroundResource(Resource.Drawable.selection_item);
            }
            item.Name.Text = items[position].Litter.name;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater
                .From(parent.Context)
                .Inflate(Resource.Layout.ItemPlaceholder, parent, false);
            var vh = new LittersViewHolder(itemView, OnClick);
            return vh;
        }

        void OnClick(int pos) => ItemClick?.Invoke(this, pos);
    }

    public class LittersViewHolder : RecyclerView.ViewHolder
    {
        public LinearLayout Parent { get; private set; }
        public TextView Name { get; private set; }

        public LittersViewHolder(View itemView, Action<int> clickListener) : base(itemView)
        {
            Parent = itemView.FindViewById<LinearLayout>(Resource.Id.ItemPlaceholderParent);
            Name = itemView.FindViewById<TextView>(Resource.Id.ItemPlaceholderText);
            itemView.Click += (sender, e) => clickListener(base.LayoutPosition);
        }
    }

    public class Department
    {
        public ELitter Litter { get; set; }
        public bool Selected { get; set; }

        public Department(ELitter l, bool s)
        {
            Litter = l;
            Selected = s;
        }
    }
}
