using Android.Hardware;
using OpenTK.Mathematics;
using System.Numerics;
using Vector3 = OpenTK.Mathematics.Vector3;
using Quaternion = OpenTK.Mathematics.Quaternion;

namespace XRay_Earth

{
    public partial class MainPage : ContentPage
    {
        Quaternion DeclinationCorrection = Quaternion.Identity;

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

                float baseRotation = MathHelper.DegreesToRadians(-90f);
                float latRad = (float)MathHelper.DegreesToRadians(location.Latitude);
                float lonRad = (float)MathHelper.DegreesToRadians(-location.Longitude);
                Vector3 locationVec3 = new Vector3(latRad + baseRotation, 0f, lonRad);

                Camera.GetCamera(Camera.Type.Main).BaseRotation = Quaternion.FromEulerAngles(locationVec3);

                GeomagneticField geoField = new GeomagneticField(
                    (float)location.Latitude,
                    (float)location.Longitude,
                    (float)(location.Altitude ?? 0),
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    );

                DeclinationCorrection = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(geoField.Declination));
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
                OrientationSensor.Default.Start(SensorSpeed.Game);
            }
        }

        private void OnOrientationChanged(object? sender, OrientationSensorChangedEventArgs e)
        {
            Camera.GetCamera(Camera.Type.Main).Rotation = ((Quaternion)e.Reading.Orientation).Inverted() * DeclinationCorrection;
        }
    }
}