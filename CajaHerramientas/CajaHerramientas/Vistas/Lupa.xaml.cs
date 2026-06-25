#if !WINDOWS
using Camera.MAUI;
#endif

namespace CajaHerramientas.Vistas
{
    public partial class Lupa : ContentPage
    {
#if !WINDOWS
        private CameraView? _cameraView;
        private bool _camaraIniciada;
        private bool _actualizandoZoom;
        private float _minZoom = 1f;
        private float _maxZoom = 5f;
        private float _zoomInicialPinch = 1f;
#endif

        public Lupa()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

#if !WINDOWS
            await PrepararCamaraAsync();
#else
            StatusLabel.Text = "Camara no disponible en Windows";
            ZoomSlider.IsEnabled = false;
            EnfocarButton.IsEnabled = false;
#endif
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

#if !WINDOWS
            if (_cameraView != null && _camaraIniciada)
            {
                await _cameraView.StopCameraAsync();
                _camaraIniciada = false;
            }
#endif
        }

#if !WINDOWS
        private async Task PrepararCamaraAsync()
        {
            var permiso = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (permiso != PermissionStatus.Granted)
            {
                permiso = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (permiso != PermissionStatus.Granted)
            {
                StatusLabel.Text = "Permiso de camara denegado";
                ZoomSlider.IsEnabled = false;
                EnfocarButton.IsEnabled = false;
                return;
            }

            if (_cameraView == null)
            {
                _cameraView = new CameraView
                {
                    AutoStartPreview = false
                };

                _cameraView.CamerasLoaded += OnCamerasLoaded;
                CameraContainer.Children.Add(_cameraView);
                StatusLabel.Text = "Buscando camara...";
            }
            else if (_cameraView.Camera != null)
            {
                await IniciarCamaraAsync();
            }
        }

        private async void OnCamerasLoaded(object? sender, EventArgs e)
        {
            await ConfigurarCamaraAsync();
        }

        private async Task ConfigurarCamaraAsync()
        {
            if (_cameraView == null)
            {
                return;
            }

            if (_cameraView.NumCamerasDetected <= 0)
            {
                StatusLabel.Text = "No se encontro camara";
                ZoomSlider.IsEnabled = false;
                EnfocarButton.IsEnabled = false;
                return;
            }

            var camaraTrasera = _cameraView.Cameras.FirstOrDefault(c =>
                c.Position.ToString().Contains("Back", StringComparison.OrdinalIgnoreCase) ||
                c.Position.ToString().Contains("Rear", StringComparison.OrdinalIgnoreCase));

            _cameraView.Camera = camaraTrasera ?? _cameraView.Cameras.First();

            _minZoom = _cameraView.MinZoomFactor > 0 ? _cameraView.MinZoomFactor : 1f;
            _maxZoom = _cameraView.MaxZoomFactor > _minZoom ? _cameraView.MaxZoomFactor : _minZoom;
            _maxZoom = Math.Min(_maxZoom, 8f);

            _actualizandoZoom = true;
            ZoomSlider.Minimum = _minZoom;
            ZoomSlider.Maximum = _maxZoom;
            ZoomSlider.Value = _minZoom;
            ZoomSlider.IsEnabled = _maxZoom > _minZoom;
            ZoomLabel.Text = $"{_minZoom:F1}x";
            _actualizandoZoom = false;

            _cameraView.ZoomFactor = _minZoom;
            await IniciarCamaraAsync();
        }

        private async Task IniciarCamaraAsync()
        {
            if (_cameraView == null || _cameraView.Camera == null || _camaraIniciada)
            {
                return;
            }

            var resultado = await _cameraView.StartCameraAsync(new Size(0, 0));

            if (resultado == CameraResult.Success)
            {
                _camaraIniciada = true;
                StatusLabel.Text = "Camara lista";
            }
            else
            {
                StatusLabel.Text = "No se pudo iniciar la camara";
                ZoomSlider.IsEnabled = false;
                EnfocarButton.IsEnabled = false;
            }
        }
#endif

        private void OnZoomChanged(object? sender, ValueChangedEventArgs e)
        {
#if !WINDOWS
            if (_actualizandoZoom || _cameraView == null)
            {
                return;
            }

            CambiarZoom((float)e.NewValue, false);
#endif
        }

        private void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
        {
#if !WINDOWS
            if (_cameraView == null || !_camaraIniciada)
            {
                return;
            }

            if (e.Status == GestureStatus.Started)
            {
                _zoomInicialPinch = _cameraView.ZoomFactor;
            }
            else if (e.Status == GestureStatus.Running)
            {
                CambiarZoom(_zoomInicialPinch * (float)e.Scale, true);
            }
#endif
        }

        private void OnEnfocarClicked(object? sender, EventArgs e)
        {
#if !WINDOWS
            _cameraView?.ForceAutoFocus();
            StatusLabel.Text = "Enfocando...";
#endif
        }

#if !WINDOWS
        private void CambiarZoom(float nuevoZoom, bool actualizarSlider)
        {
            if (_cameraView == null)
            {
                return;
            }

            var zoom = Math.Max(_minZoom, Math.Min(_maxZoom, nuevoZoom));
            _cameraView.ZoomFactor = zoom;
            ZoomLabel.Text = $"{zoom:F1}x";

            if (actualizarSlider)
            {
                _actualizandoZoom = true;
                ZoomSlider.Value = zoom;
                _actualizandoZoom = false;
            }
        }
#endif
    }
}
