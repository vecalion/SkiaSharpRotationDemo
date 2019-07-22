using System;
using System.ComponentModel;
using System.Threading.Tasks;
using SkiaSharp;
using Xamarin.Forms;

namespace RotationDemo
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        bool _pageIsActive;
        float _degrees;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _pageIsActive = true;
            AnimationLoop();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _pageIsActive = false;
        }

        async Task AnimationLoop()
        {
            while (_pageIsActive)
            {
                canvasView.InvalidateSurface();
                await Task.Delay(TimeSpan.FromSeconds(1.0 / 30));
            }
        }

        void OnCanvasViewPaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            IncrementDegrees();

            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;

            var x1 = info.Width / 6;
            var x2 = info.Width / 3 + x1;
            var x3 = info.Width / 3 * 2 + x1;
            var y = info.Height / 2;

            int _squareWidth = Math.Min(info.Width, info.Height) / 10;

            canvas.Clear();

            using (SKPaint paint = new SKPaint())
            using (SKPaint paint2 = new SKPaint())
            using (var path1 = new SKPath())
            using (var path2 = new SKPath())
            using (var path3 = new SKPath())
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = SKColors.Black;
                paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 10 }, 2);
                paint.StrokeWidth = 1;

                paint2.Style = SKPaintStyle.Stroke;
                paint2.Color = SKColors.Black;
                paint2.IsAntialias = true;
                paint2.StrokeWidth = 3;

                canvas.DrawLine(0, y, info.Width, y, paint);

                canvas.DrawLine(x1, 0, x1, info.Height, paint);
                canvas.DrawLine(x2, 0, x2, info.Height, paint);
                canvas.DrawLine(x3, 0, x3, info.Height, paint);

                path1.MoveTo(x1, y);
                path1.LineTo(x1 + _squareWidth, y);
                path1.LineTo(x1 + _squareWidth, y - _squareWidth);
                path1.LineTo(x1, y - _squareWidth);
                path1.LineTo(x1, y);

                var cx2 = x2 + _squareWidth * 2 - _squareWidth / 2;
                var cy2 = y - _squareWidth * 2 + _squareWidth / 2;
                path2.MoveTo(cx2, cy2);
                path2.LineTo(cx2 + _squareWidth, cy2);
                path2.LineTo(cx2 + _squareWidth, cy2 - _squareWidth);
                path2.LineTo(cx2, cy2 - _squareWidth);
                path2.LineTo(cx2, cy2);

                var cx3 = x3 - _squareWidth / 2;
                var cy3 = y + _squareWidth / 2;
                path3.MoveTo(cx3, cy3);
                path3.LineTo(cx3 + _squareWidth, cy3);
                path3.LineTo(cx3 + _squareWidth, cy3 - _squareWidth);
                path3.LineTo(cx3, cy3 - _squareWidth);
                path3.LineTo(cx3, cy3);

                DrawRotatedUsingCanvas(canvas, path1, paint2, _degrees, path1[0].X, path1[0].Y);
                DrawRotatedManually(canvas, path2, paint2, _degrees, x2, y);
                DrawRotatedWithMatrices(canvas, path3, paint2, _degrees, x3, y);
            }
        }

        private void IncrementDegrees()
        {
            _degrees += 3.5f;

            if (_degrees >= 360)
            {
                _degrees = 0;
            }
        }

        void DrawRotatedWithMatrices(SKCanvas canvas, SKPath path, SKPaint paint, float degrees, int cx, int cy)
        {
            var result = SKMatrix.MakeIdentity();
            var translate = SKMatrix.MakeTranslation(-cx, -cy);
            var rotate = SKMatrix.MakeRotationDegrees(degrees);
            var translate2 = SKMatrix.MakeTranslation(cx, cy);

            SKMatrix.PostConcat(ref result, translate);
            SKMatrix.PostConcat(ref result, rotate);
            SKMatrix.PostConcat(ref result, translate2);

            path.Transform(result);
            canvas.DrawPath(path, paint);
        }

        void DrawRotatedUsingCanvas(SKCanvas canvas, SKPath path, SKPaint paint, float degrees, float cx, float cy)
        {
            canvas.Save();
            canvas.RotateDegrees(degrees, cx, cy);
            canvas.DrawPath(path, paint);
            canvas.Restore();
        }

        void DrawRotatedManually(SKCanvas canvas, SKPath path, SKPaint paint, float degrees, int cx, int cy)
        {
            float rad = ToRadians(degrees);

            using (SKPath pathRotated = new SKPath())
            {
                pathRotated.MoveTo(Rotate(path[0], cx, cy, rad));
                pathRotated.LineTo(Rotate(path[1], cx, cy, rad));
                pathRotated.LineTo(Rotate(path[2], cx, cy, rad));
                pathRotated.LineTo(Rotate(path[3], cx, cy, rad));
                pathRotated.LineTo(Rotate(path[4], cx, cy, rad));
                canvas.DrawPath(pathRotated, paint);
            }
        }

        SKPoint Rotate(SKPoint point, int cx, int cy, float rad)
        {
            var sin = (float)Math.Sin(rad);
            var cos = (float)Math.Cos(rad);

            var translatedX = point.X - cx;
            var translatedY = point.Y - cy;

            var rotatedX = translatedX * cos - translatedY * sin;
            var rotatedY = translatedX * sin + translatedY * cos;

            return new SKPoint(rotatedX + cx, rotatedY + cy);
        }

        static float ToRadians(float degrees)
        {
            return (float)(Math.PI * degrees / 180.0);
        }
    }
}
