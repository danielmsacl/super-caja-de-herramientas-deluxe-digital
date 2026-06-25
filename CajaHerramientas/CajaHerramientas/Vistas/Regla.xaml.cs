namespace CajaHerramientas.Vistas
{
    public partial class Regla : ContentPage
    {
        public Regla()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateRuler();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width > 0) UpdateRuler();
        }

        private void UpdateRuler()
        {
            var info = DeviceDisplay.MainDisplayInfo;
            var dipsPerCm = CalcDipsPerCm();
            var totalCm = info.Width / dipsPerCm;

            ScreenWidthLabel.Text = $"Ancho de pantalla: {totalCm:F1} cm";

            RulerView.Drawable = new RulerDrawable((float)dipsPerCm, (float)info.Width);
            RulerView.Invalidate();
        }

        private static double CalcDipsPerCm()
        {
#if WINDOWS
            return 96.0 / 2.54;
#elif IOS
            return 163.0 / 2.54;
#else
            return 160.0 / 2.54;
#endif
        }
    }

    public class RulerDrawable : IDrawable
    {
        private readonly float _dipsPerCm;
        private readonly float _screenWidthDips;
        private const float RulerY = 80f;
        private const float LeftMargin = 16f;
        private const float RightMargin = 16f;
        private const int MmPerCm = 10;

        public RulerDrawable(float dipsPerCm, float screenWidthDips)
        {
            _dipsPerCm = dipsPerCm;
            _screenWidthDips = screenWidthDips;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var drawWidth = Math.Min(_screenWidthDips, dirtyRect.Width);
            var rightX = drawWidth - RightMargin;
            var rulerWidth = rightX - LeftMargin;
            var totalMm = (int)(rulerWidth / _dipsPerCm * MmPerCm);

            canvas.Antialias = true;

           
            canvas.StrokeColor = Color.FromArgb("#ECF0F1");
            canvas.StrokeSize = 2;
            canvas.DrawLine(LeftMargin, RulerY, rightX, RulerY);

            for (int mm = 0; mm <= totalMm; mm++)
            {
                var x = LeftMargin + mm * _dipsPerCm / MmPerCm;
                if (x > rightX) break;

                bool isCm = mm % MmPerCm == 0;
                bool isHalf = mm % (MmPerCm / 2) == 0;
                int cmNum = mm / MmPerCm;
                bool is5Cm = isCm && cmNum % 5 == 0;

                float tickHeight;
                float fontSize;
                Color color;

                if (is5Cm)
                {
                    tickHeight = 28f;
                    fontSize = 13f;
                    color = Colors.White;
                }
                else if (isCm)
                {
                    tickHeight = 22f;
                    fontSize = 11f;
                    color = Color.FromArgb("#BDC3C7");
                }
                else if (isHalf)
                {
                    tickHeight = 16f;
                    fontSize = 0f;
                    color = Color.FromArgb("#95A5A6");
                }
                else
                {
                    tickHeight = 10f;
                    fontSize = 0f;
                    color = Color.FromArgb("#5D6D7E");
                }

                canvas.StrokeColor = color;
                canvas.StrokeSize = isCm ? 2f : 1f;
                canvas.DrawLine(x, RulerY - tickHeight, x, RulerY + tickHeight);

                if (isCm)
                {
                    canvas.FontColor = color;
                    canvas.FontSize = fontSize;
                    canvas.DrawString($"{cmNum}", x, RulerY + tickHeight + 5,
                        HorizontalAlignment.Center);
                }
            }

           
            var totalCm = rulerWidth / _dipsPerCm;
            canvas.FontColor = Color.FromArgb("#3498DB");
            canvas.FontSize = 12;
            canvas.DrawString($"{totalCm:F1} cm →", rightX - 2, RulerY - 34,
                HorizontalAlignment.Right);
        }
    }
}
