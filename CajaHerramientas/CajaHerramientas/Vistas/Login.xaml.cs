using Microsoft.Maui.Controls;
using CajaHerramientas.Vistas;

namespace CajaHerramientas.Vistas
{
    public partial class Login : ContentPage
    {
        public Login()
        {
            InitializeComponent();
        }

        private void OnEntrarClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NombreEntry.Text))
            {
                BienvenidaLabel.Text = $"¡Bienvenido, {NombreEntry.Text}!";
                BienvenidaLabel.IsVisible = true;
                Navigation.PushAsync(new Principal());
            }

        }

    }
}