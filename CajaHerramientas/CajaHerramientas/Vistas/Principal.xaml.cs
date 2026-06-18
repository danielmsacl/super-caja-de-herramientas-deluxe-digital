using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace CajaHerramientas.Vistas
{
    public partial class Principal : ContentPage
    {
        private bool _linternaEncendida = false;

        public Principal()
        {
            InitializeComponent();
        }

        // Botón Brújula
        private async void OnBrujulaClicked(object sender, EventArgs e)
        {
            await DisplayAlertAsync("Brújula", "Has seleccionado la Brújula", "OK");
        }

        // Botón Regla 
        private void OnReglaClicked(object sender, EventArgs e)
        {
            ReglaImagen.IsVisible = true;
        }

        // Botón Nivel
        private async void OnNivelClicked(object sender, EventArgs e)
        {
            await DisplayAlertAsync("Nivel", "Has seleccionado el Nivel", "OK");
        }

        // Botón Lupa
        private async void OnLupaClicked(object sender, EventArgs e)
        {
            await DisplayAlertAsync("Lupa", "Has seleccionado la Lupa", "OK");
        }

        // Botón Linterna
        private async void OnLinternaClicked(object sender, EventArgs e)
        {
            try
            {
                if (!_linternaEncendida)
                {
                    await Flashlight.Default.TurnOnAsync();
                    _linternaEncendida = true;
                    LinternaButton.BackgroundColor = Colors.Green;
                    LinternaButton.Text = "🔦 ENCENDIDA";
                }
                else
                {
                    await Flashlight.Default.TurnOffAsync();
                    _linternaEncendida = false;
                    LinternaButton.BackgroundColor = Colors.Orange;
                    LinternaButton.Text = "🔦 Linterna";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
    }
}