
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Felipecsl.GifImageViewLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AVG_Scale_Installer
{
    public class DoingWork : DialogFragment
    {
        private GifImageView Gif;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.DoingWork, container, false);

            try
            {
                Gif = view.FindViewById<GifImageView>(Resource.Id.DoingWorkGif);
                Stream input = Resources.OpenRawResource(Resource.Drawable.blinkgif);
                byte[] bytes = ConvertByteArray(input);
                Gif.SetBytes(bytes);
                Gif.StartAnimation();

                Dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
                Dialog.SetCanceledOnTouchOutside(false);
                Dialog.SetCancelable(false);
            }
            catch(Exception ex) { }

            return view;
        }

        private byte[] ConvertByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, read);
                return ms.ToArray();
            }
        }
    }
}