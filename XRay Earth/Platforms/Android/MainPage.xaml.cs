using Android.Hardware;
using OpenTK.Mathematics;
using System.Numerics;
using Vector3 = OpenTK.Mathematics.Vector3;
using Quaternion = OpenTK.Mathematics.Quaternion;

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
                Location location = await Geolocation.GetLocationAsync();

                GeomagneticField geoField = new GeomagneticField(
                    (float)location.Latitude,
                    (float)location.Longitude,
                    (float)(location.Altitude ?? 0),
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    );

                Camera.Instance.DeclinationCorrection = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(geoField.Declination));
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