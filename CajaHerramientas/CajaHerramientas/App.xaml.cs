using CajaHerramientas.Vistas;

namespace CajaHerramientas
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new Login());
        }
    }
}