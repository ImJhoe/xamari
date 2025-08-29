using System;
using Xamarin.Forms;

namespace CitasMedicasApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadUserInfo();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            if (Application.Current.Properties.ContainsKey("UserName"))
            {
                string userName = Application.Current.Properties["UserName"].ToString();
                string userType = Application.Current.Properties.ContainsKey("UserType") ?
                    Application.Current.Properties["UserType"].ToString() : "Usuario";

                UserInfoLabel.Text = $"Bienvenido {userName} ({userType})";
            }
        }

        private async void OnRegistroMedicoTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("registromedico");
        }

        private async void OnConsultarMedicoTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("consultarmedico");
        }

        private async void OnCrearCitaTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("crearcita");
        }

        private async void OnVerCitasTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("vercitas");
        }
    }
}