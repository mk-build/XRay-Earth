namespace XRay_Earth
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Task.Delay(500).ContinueWith(_ => StartOrientationSensor());

            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status == PermissionStatus.Granted)
            {
                var location = await Geolocation.GetLocationAsync();
                
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (OrientationSensor.Default.IsSupported)
            {
                OrientationSensor.Default.ReadingChanged -= OnOrientationChanged;
                OrientationSensor.Default.Stop();
            }
        }

        private void StartOrientationSensor()
        {
            if (OrientationSensor.Default.IsSupported)
            {
                OrientationSensor.Default.ReadingChanged += OnOrientationChanged;
                OrientationSensor.Default.Start(SensorSpeed.Fastest);
            }
        }

        private void OnOrientationChanged(object? sender, OrientationSensorChangedEventArgs e)
        {
            Camera.Instance.Rotation = e.Reading.Orientation;            
        }
    }
}