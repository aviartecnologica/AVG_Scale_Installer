
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AVG_access_data;
using AVG_Scale_Installer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVG_Scale_Installer
{
    public class LogIn : Fragment
    {
        private LinearLayout Parent;
        private ImageView LogoCenter;
        private EditText UserInput;
        private EditText PasswordInput;
        private Button EnterButton;
        private TextView InvalidCredentials;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.LogIn, container, false);

            Parent = view.FindViewById<LinearLayout>(Resource.Id.LogInParent);

            LogoCenter = view.FindViewById<ImageView>(Resource.Id.LogInLogoCenter);

            UserInput = view.FindViewById<EditText>(Resource.Id.LogInUserInput);
            PasswordInput = view.FindViewById<EditText>(Resource.Id.LogInPasswordInput);

            EnterButton = view.FindViewById<Button>(Resource.Id.LogInEnterButton);
            InvalidCredentials = view.FindViewById<TextView>(Resource.Id.LogInInvalidCredentialsText);

            view.FindViewById<LinearLayout>(Resource.Id.LogInLayout).Touch += delegate
            {
                Functions.HideKeyboard(Activity);
                UserInput.ClearFocus();
                PasswordInput.ClearFocus();
            };

            EnterButton.Click += TryLogin;

            return view;
        }

        public async override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            
        }

        private async void TryLogin(object sender, EventArgs e)
        {
            bool completed = false;

            Functions.HideKeyboard(Activity);
            UserInput.ClearFocus();
            PasswordInput.ClearFocus();

            EnterButton.Enabled = false;
            InvalidCredentials.Visibility = ViewStates.Gone;


            //UserInput.Text = "aviartec@gmail.com";
            //PasswordInput.Text = "temporal123";

            try
            {
                LoginResponse loginResponse = await RequestAPI.DoAPIAuthentication(UserInput.Text, PasswordInput.Text);

                if (loginResponse.success)
                {
                    EPerson person = await RequestAPI.GetPerson((int)loginResponse.id, loginResponse.token);

                    if(person != null)
                    {
                        completed = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(Context, Resource.String.login_error, ToastLength.Short).Show();
            }

            if (completed)
            {

                Parent.Visibility = ViewStates.Invisible;
                LogoCenter.Visibility = ViewStates.Visible;

                await Task.Delay(2000);

                Activity.StartActivity(new Intent(Activity, typeof(MainActivity)));

            }
            else
            {
                InvalidCredentials.Visibility = ViewStates.Visible;
                UserInput.Text = "";
                PasswordInput.Text = "";
                EnterButton.Enabled = true;
            }


        }
    }
}