namespace CajaHerramientas.Vistas
{
    public partial class Nivel : ContentPage
    {
        private readonly NivelDrawable _drawable;

        public Nivel()
        {
            InitializeComponent();
            _drawable = new NivelDrawable();
            NivelView.Drawable = _drawable;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Accelerometer.Default.IsSupported)
            {
                Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.UI);
            }
            else
            {
                EstadoLabel.Text = "Acelerometro no disponible";
                AnguloLabel.Text = "";
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (Accelerometer.Default.IsMonitoring)
            {
                Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
                Accelerometer.Default.Stop();
            }
        }

        private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            var aceleracion = e.Reading.Acceleration;

            var x = Math.Max(-1, Math.Min(1, aceleracion.X));
            var y = Math.Max(-1, Math.Min(1, aceleracion.Y));
            var z = aceleracion.Z;

            var inclinacion = Math.Atan2(Math.Sqrt(x * x + y * y), Math.Abs(z)) * 180 / Math.PI;
            var estaNivelado = Math.Abs(x) < 0.04 && Math.Abs(y) < 0.04;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                _drawable.BurbujaX = x;
                _drawable.BurbujaY = y;
                _drawable.EstaNivelado = estaNivelado;
                NivelView.Invalidate();

                AnguloLabel.Text = $"Inclinacion: {inclinacion:F1} grados";
                EstadoLabel.Text = estaNivelado ? "Superficie nivelada" : "Superficie inclinada";
                EstadoLabel.TextColor = estaNivelado ? Color.FromArgb("#27AE60") : Color.FromArgb("#E67E22");
            });
        }
    }

    public class NivelDrawable : IDrawable
    {
        public double BurbujaX { get; set; }
        public double BurbujaY { get; set; }
        public bool EstaNivelado { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var size = Math.Min(dirtyRect.Width, dirtyRect.Height);
            var cx = dirtyRect.Width / 2;
            var cy = dirtyRect.Height / 2;
            var radius = size / 2 - 12;
            var maxMove = radius - 42;

            canvas.Antialias = true;

            canvas.FillColor = Color.FromArgb("#1A1A2E");
            canvas.FillCircle(cx, cy, radius);

            canvas.StrokeColor = Color.FromArgb("#3498DB");
            canvas.StrokeSize = 4;
            canvas.DrawCircle(cx, cy, radius);

            canvas.StrokeColor = Color.FromArgb("#3A3A55");
            canvas.StrokeSize = 2;
            canvas.DrawCircle(cx, cy, radius * 0.65f);
            canvas.DrawCircle(cx, cy, radius * 0.35f);

            canvas.StrokeColor = Color.FromArgb("#888888");
            canvas.StrokeSize = 1.5f;
            canvas.DrawLine(cx - radius + 25, cy, cx + radius - 25, cy);
            canvas.DrawLine(cx, cy - radius + 25, cx, cy + radius - 25);

            canvas.FillColor = EstaNivelado ? Color.FromArgb("#27AE60") : Color.FromArgb("#888888");
            canvas.FillCircle(cx, cy, 8);

            var bubbleX = cx + (float)(BurbujaX * maxMove);
            var bubbleY = cy + (float)(BurbujaY * maxMove);
            var distance = Math.Sqrt(Math.Pow(bubbleX - cx, 2) + Math.Pow(bubbleY - cy, 2));

            if (distance > maxMove)
            {
                var scale = maxMove / distance;
                bubbleX = cx + (bubbleX - cx) * (float)scale;
                bubbleY = cy + (bubbleY - cy) * (float)scale;
            }

            canvas.FillColor = EstaNivelado ? Color.FromArgb("#2ECC71") : Color.FromArgb("#F1C40F");
            canvas.FillCircle(bubbleX, bubbleY, 26);

            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 3;
            canvas.DrawCircle(bubbleX, bubbleY, 26);

            canvas.FillColor = Color.FromRgba(255, 255, 255, 120);
            canvas.FillCircle(bubbleX - 8, bubbleY - 8, 7);
        }
    }
}
