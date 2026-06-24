namespace CajaHerramientas.Vistas
{
    public partial class Brujula : ContentPage
    {
        private readonly CompassDrawable _drawable;

        public Brujula()
        {
            InitializeComponent();
            _drawable = new CompassDrawable();
            CompassView.Drawable = _drawable;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Compass.Default.IsSupported)
            {
                Compass.Default.ReadingChanged += OnCompassReadingChanged;
                Compass.Default.Start(SensorSpeed.UI);
            }
            else
            {
                HeadingLabel.Text = "Brújula no disponible";
                DirectionLabel.Text = "";
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (Compass.Default.IsMonitoring)
            {
                Compass.Default.ReadingChanged -= OnCompassReadingChanged;
                Compass.Default.Stop();
            }
        }

        private void OnCompassReadingChanged(object? sender, CompassChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var heading = e.Reading.HeadingMagneticNorth;
                _drawable.Heading = heading;
                CompassView.Invalidate();

                HeadingLabel.Text = $"{heading:F1}°";

                DirectionLabel.Text = heading switch
                {
                    >= 348.75 or < 11.25 => "N",
                    >= 11.25 and < 33.75 => "NNE",
                    >= 33.75 and < 56.25 => "NE",
                    >= 56.25 and < 78.75 => "ENE",
                    >= 78.75 and < 101.25 => "E",
                    >= 101.25 and < 123.75 => "ESE",
                    >= 123.75 and < 146.25 => "SE",
                    >= 146.25 and < 168.75 => "SSE",
                    >= 168.75 and < 191.25 => "S",
                    >= 191.25 and < 213.75 => "SSW",
                    >= 213.75 and < 236.25 => "SW",
                    >= 236.25 and < 258.75 => "WSW",
                    >= 258.75 and < 281.25 => "W",
                    >= 281.25 and < 303.75 => "WNW",
                    >= 303.75 and < 326.25 => "NW",
                    >= 326.25 and < 348.75 => "NNW",
                    _ => "N"
                };
            });
        }
    }

    public class CompassDrawable : IDrawable
    {
        public double Heading { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var size = Math.Min(dirtyRect.Width, dirtyRect.Height);
            var cx = dirtyRect.Width / 2;
            var cy = dirtyRect.Height / 2;
            var radius = size / 2 - 10;

            canvas.Antialias = true;

            // Fondo del círculo
            canvas.FillColor = Color.FromArgb("#16213E");
            canvas.FillCircle(cx, cy, radius);

            // Borde exterior
            canvas.StrokeColor = Color.FromArgb("#0F3460");
            canvas.StrokeSize = 3;
            canvas.DrawCircle(cx, cy, radius);

            // Borde interior
            canvas.StrokeColor = Color.FromArgb("#533483");
            canvas.StrokeSize = 1;
            canvas.DrawCircle(cx, cy, radius - 15);

            // Marcas de grados
            for (int i = 0; i < 360; i += 5)
            {
                var angle = i * Math.PI / 180;
                bool isMajor = i % 45 == 0;
                bool isMedium = i % 15 == 0 && !isMajor;

                float innerR = isMajor ? radius - 30 : isMedium ? radius - 20 : radius - 12;
                float outerR = radius - 8;

                var x1 = cx + (float)Math.Cos(angle) * innerR;
                var y1 = cy + (float)Math.Sin(angle) * innerR;
                var x2 = cx + (float)Math.Cos(angle) * outerR;
                var y2 = cy + (float)Math.Sin(angle) * outerR;

                canvas.StrokeColor = isMajor ? Colors.White : Color.FromArgb("#888888");
                canvas.StrokeSize = isMajor ? 2.5f : isMedium ? 1.5f : 1;
                canvas.DrawLine(x1, y1, x2, y2);
            }

            // Letras cardinales (fijas — sin rotar)
            float textOffset = radius - 45;
            canvas.FontSize = 22;
            canvas.FontColor = Colors.White;

            canvas.DrawString("N", cx, cy - textOffset, HorizontalAlignment.Center);
            canvas.FontColor = Color.FromArgb("#888888");
            canvas.DrawString("S", cx, cy + textOffset, HorizontalAlignment.Center);
            canvas.DrawString("E", cx + textOffset, cy, HorizontalAlignment.Center);
            canvas.DrawString("W", cx - textOffset, cy, HorizontalAlignment.Center);

            // Aguja de la brújula (rota según heading)
            var headingRad = (Heading - 90) * Math.PI / 180;

            // Aguja norte (roja)
            float needleLength = radius - 35;
            float nx = cx + (float)Math.Cos(headingRad) * needleLength;
            float ny = cy + (float)Math.Sin(headingRad) * needleLength;

            canvas.StrokeColor = Colors.Red;
            canvas.StrokeSize = 4;
            canvas.DrawLine(cx, cy, nx, ny);

            // Punta de la aguja norte
            canvas.FillColor = Colors.Red;
            var tipSize = 8;
            canvas.FillCircle(nx, ny, tipSize);

            // Aguja sur (gris)
            var southRad = headingRad + Math.PI;
            float sx = cx + (float)Math.Cos(southRad) * (needleLength * 0.7f);
            float sy = cy + (float)Math.Sin(southRad) * (needleLength * 0.7f);

            canvas.StrokeColor = Color.FromArgb("#888888");
            canvas.StrokeSize = 3;
            canvas.DrawLine(cx, cy, sx, sy);

            // Círculo central
            canvas.FillColor = Color.FromArgb("#533483");
            canvas.FillCircle(cx, cy, 8);
        }
    }
}
