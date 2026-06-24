#if !WINDOWS
using Camera.MAUI;
#endif

namespace CajaHerramientas.Vistas
{
    public partial class Regla : ContentPage
    {
        private readonly RulerDrawable _drawable;
        private bool _isMeasuring;
        private bool _hasGravity;

        private double _gx, _gy, _gz;
        private double _vx, _vy, _vz;
        private double _px, _py, _pz;
        private DateTime _lastSample;
        private double _totalDistanceCm;

        private const double GravityAlpha = 0.85;
        private const double VelocityDecay = 0.97;
        private const double NoiseThreshold = 0.04;
        private const float PixelsPerCm = 8f;

        public Regla()
        {
            InitializeComponent();
            _drawable = new RulerDrawable();
            RulerOverlay.Drawable = _drawable;

#if !WINDOWS
            var cameraView = new CameraView { AutoStartPreview = true };
            CameraContainer.Children.Add(cameraView);
#else
            StatusLabel.Text = "Cámara no disponible en Windows";
#endif
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Accelerometer.Default.IsSupported)
            {
                Accelerometer.Default.ReadingChanged += OnAccelerometerReading;
                Accelerometer.Default.Start(SensorSpeed.Game);
            }
            else
            {
                StatusLabel.Text = "Acelerómetro no disponible en este dispositivo";
                IniciarButton.IsEnabled = false;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (Accelerometer.Default.IsMonitoring)
            {
                Accelerometer.Default.ReadingChanged -= OnAccelerometerReading;
                Accelerometer.Default.Stop();
            }
        }

        private void OnAccelerometerReading(object? sender, AccelerometerChangedEventArgs e)
        {
            var raw = e.Reading.Acceleration;
            var now = DateTime.UtcNow;

            if (!_hasGravity)
            {
                _gx = raw.X; _gy = raw.Y; _gz = raw.Z;
                _hasGravity = true;
                _lastSample = now;
                return;
            }

            var dt = (now - _lastSample).TotalSeconds;
            if (dt <= 0 || dt > 0.1) { _lastSample = now; return; }
            _lastSample = now;

            _gx = GravityAlpha * _gx + (1 - GravityAlpha) * raw.X;
            _gy = GravityAlpha * _gy + (1 - GravityAlpha) * raw.Y;
            _gz = GravityAlpha * _gz + (1 - GravityAlpha) * raw.Z;

            var lx = raw.X - _gx;
            var ly = raw.Y - _gy;
            var lz = raw.Z - _gz;

            var mag = Math.Sqrt(lx * lx + ly * ly + lz * lz);
            if (mag < NoiseThreshold)
            {
                lx = 0; ly = 0; lz = 0;
            }

            if (_isMeasuring)
            {
                _vx = (_vx + lx * dt) * VelocityDecay;
                _vy = (_vy + ly * dt) * VelocityDecay;
                _vz = (_vz + lz * dt) * VelocityDecay;

                _px += _vx * dt;
                _py += _vy * dt;
                _pz += _vz * dt;

                var totalM = Math.Sqrt(_px * _px + _py * _py + _pz * _pz);
                _totalDistanceCm = totalM * 100;

                if (_totalDistanceCm < 0.5 && mag < NoiseThreshold)
                {
                    _totalDistanceCm = 0;
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DistanciaLabel.Text = $"{_totalDistanceCm:F1} cm";
                    _drawable.DistancePixels = (float)(_totalDistanceCm * PixelsPerCm);
                    RulerOverlay.Invalidate();
                });
            }
        }

        private void OnIniciarClicked(object? sender, EventArgs e)
        {
            _isMeasuring = true;
            _vx = _vy = _vz = 0;
            _px = _py = _pz = 0;
            _totalDistanceCm = 0;
            _hasGravity = false;
            _drawable.DistancePixels = 0;

            DistanciaLabel.Text = "0.0 cm";
            StatusLabel.Text = "Midiendo... Avanza con el dispositivo";
            IniciarButton.IsEnabled = false;
            FinalizarButton.IsEnabled = true;
            RulerOverlay.Invalidate();
        }

        private void OnFinalizarClicked(object? sender, EventArgs e)
        {
            _isMeasuring = false;

            StatusLabel.Text = "Medición completada";
            IniciarButton.IsEnabled = true;
            FinalizarButton.IsEnabled = false;

            DistanciaLabel.Text = $"{_totalDistanceCm:F1} cm";
            _drawable.DistancePixels = (float)(_totalDistanceCm * PixelsPerCm);
            RulerOverlay.Invalidate();
        }
    }

    public class RulerDrawable : IDrawable
    {
        public float DistancePixels { get; set; }
        private const float ScalePixelsPerCm = 8f;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var w = dirtyRect.Width;
            var h = dirtyRect.Height;
            var rulerY = h / 2;
            var leftMargin = 30f;

            if (DistancePixels <= 0)
            {
                canvas.FontColor = Color.FromArgb("#888888");
                canvas.FontSize = 14;
                canvas.DrawString("← Inicia la medición →", w / 2, rulerY, HorizontalAlignment.Center);
                return;
            }

            var lineLength = Math.Min(DistancePixels, w - leftMargin - 20);
            var rightX = leftMargin + lineLength;

            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 3;
            canvas.DrawLine(leftMargin, rulerY, rightX, rulerY);

            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 2;
            canvas.DrawLine(leftMargin, rulerY - 15, leftMargin, rulerY + 15);

            canvas.FillColor = Color.FromArgb("#27AE60");
            var path = new PathF();
            path.MoveTo(rightX, rulerY - 12);
            path.LineTo(rightX + 10, rulerY);
            path.LineTo(rightX, rulerY + 12);
            path.Close();
            canvas.FillPath(path);

            var cmCount = (int)(lineLength / ScalePixelsPerCm);
            for (int i = 1; i <= cmCount; i++)
            {
                var x = leftMargin + i * ScalePixelsPerCm;
                if (x > rightX) break;

                bool isMainTick = i % 5 == 0;
                var tickHeight = isMainTick ? 15f : 10f;

                canvas.StrokeColor = isMainTick ? Colors.White : Color.FromArgb("#AAAAAA");
                canvas.StrokeSize = isMainTick ? 2f : 1f;
                canvas.DrawLine(x, rulerY - tickHeight, x, rulerY + tickHeight);

                if (isMainTick || i % 2 == 0)
                {
                    canvas.FontColor = Colors.White;
                    canvas.FontSize = 10;
                    canvas.DrawString($"{i}", x, rulerY + tickHeight + 12, HorizontalAlignment.Center);
                }
            }

            canvas.FontColor = Color.FromArgb("#27AE60");
            canvas.FontSize = 14;
            canvas.DrawString($"{DistancePixels / ScalePixelsPerCm:F1} cm", rightX + 16, rulerY + 4, HorizontalAlignment.Left);
        }
    }
}
