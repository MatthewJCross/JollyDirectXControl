using Jolly2DEngineLab.Animation;
using System;
using System.Windows;

namespace Jolly2DEngineLab
{
    public partial class MainWindow : Window
    {
        private EngineAnimation? _engineAnimation;
        private bool _viewportResizePending;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
            AnimationView.SizeChanged += AnimationView_SizeChanged;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_engineAnimation != null)
            {
                return;
            }

            await AnimationView.WaitForReadyAsync();

            if (!IsLoaded)
            {
                return;
            }

            _engineAnimation = new EngineAnimation(AnimationView);

            _engineAnimation.Input.Attach(this);

            _engineAnimation.StrokeMm = (float)StrokeSlider.Value;
            _engineAnimation.BoreMm = (float)BoreSlider.Value;
            _engineAnimation.Rpm = (float)RpmSlider.Value;
            _engineAnimation.Paused = false;

            UpdateEngineData();

            _engineAnimation.Start();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            if (_engineAnimation == null)
            {
                return;
            }

            _engineAnimation.Input.Detach(this);
            _engineAnimation.Stop();
            ((IDisposable)_engineAnimation).Dispose();

            _engineAnimation = null;
        }

        private void AnimationView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // The DirectX control should resize through its existing
            // WindowsFormsHost and panel resize handling.
        }

        private void StrokeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (StrokeValueText == null)
            {
                return;
            }

            StrokeValueText.Text = $"{StrokeSlider.Value:F1} mm";

            if (_engineAnimation != null)
            {
                _engineAnimation.StrokeMm = (float)StrokeSlider.Value;
            }

            UpdateEngineData();
        }

        private void BoreSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (BoreValueText == null)
            {
                return;
            }

            BoreValueText.Text = $"{BoreSlider.Value:F1} mm";

            if (_engineAnimation != null)
            {
                _engineAnimation.BoreMm = (float)BoreSlider.Value;
            }

            UpdateEngineData();
        }

        private void RpmSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (RpmValueText == null)
            {
                return;
            }

            RpmValueText.Text = $"{RpmSlider.Value:N0} RPM";

            if (_engineAnimation != null)
            {
                _engineAnimation.Rpm = (float)RpmSlider.Value;
            }

            UpdateEngineData();
        }

        private void StrokeDecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            StrokeSlider.Value = Math.Max(StrokeSlider.Minimum, StrokeSlider.Value - 1.0);
        }

        private void StrokeIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            StrokeSlider.Value = Math.Min(StrokeSlider.Maximum, StrokeSlider.Value + 1.0);
        }

        private void BoreDecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            BoreSlider.Value = Math.Max(BoreSlider.Minimum, BoreSlider.Value - 1.0);
        }

        private void BoreIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            BoreSlider.Value = Math.Min(BoreSlider.Maximum, BoreSlider.Value + 1.0);
        }

        private void RpmDecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            RpmSlider.Value = Math.Max(RpmSlider.Minimum, RpmSlider.Value - 100.0);
        }

        private void RpmIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            RpmSlider.Value = Math.Min(RpmSlider.Maximum, RpmSlider.Value + 100.0);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_engineAnimation == null)
            {
                return;
            }

            _engineAnimation.Paused = false;

            AnimationStatusText.Text = "Running";
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_engineAnimation == null)
            {
                return;
            }

            _engineAnimation.Paused = true;

            AnimationStatusText.Text = "Stopped";
        }

        private void UpdateEngineData()
        {
            if (DisplacementValueText == null || RatioValueText == null)
            {
                return;
            }

            double stroke = StrokeSlider.Value;
            double bore = BoreSlider.Value;

            double displacementCc = Math.PI / 4.0 * bore * bore * stroke / 1000.0;
            double ratio = stroke / bore;

            DisplacementValueText.Text = $"{displacementCc:N1} cc";
            RatioValueText.Text = $"{ratio:F2} : 1";
        }
    }
}