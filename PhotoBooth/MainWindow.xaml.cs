using CameraControl.Devices;
using CameraControl.Devices.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoBooth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Storyboard SBShowCountDown;
        public CameraDeviceManager DeviceManager { get; set; }
        public ICameraDevice CameraDevice { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            DeviceManager = new CameraDeviceManager();
            DeviceManager.CameraSelected += DeviceManager_CameraSelected;
            DeviceManager.CameraConnected += DeviceManager_CameraConnected;
            DeviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
            DeviceManager.CameraDisconnected += DeviceManager_CameraDisconnected;
            DeviceManager.UseExperimentalDrivers = false;
            DeviceManager.DisableNativeDrivers = false;
            SBShowCountDown = (Storyboard)TryFindResource("StoryboardCountDown");
        }


        private void DeviceManager_CameraDisconnected(ICameraDevice cameraDevice)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                btnCamera.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 51, 51, 51));
            }));
            
        }

        private void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
        {
            //var a = 1;
            eventArgs.CameraDevice.TransferFile(eventArgs.Handle, "C:\\Temp\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg");
            eventArgs.CameraDevice.IsBusy = false;
        }

        private void GridMainDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TakePhoto();
        }

        private async void TakePhoto()
        {
            for (int i = 0; i<5;i++)
            {
                StartCountDown(3);
                await Task.Delay(4000);
                Thread thread = new Thread(Capture);
                thread.Start();
            }
            
        }

        private void Capture()
        {
            bool retry;
            do
            {
                retry = false;
                try
                {
                    DeviceManager.SelectedCameraDevice.CapturePhoto();
                }
                catch (DeviceException exception)
                {
                    if (exception.ErrorCode == ErrorCodes.MTP_Device_Busy || exception.ErrorCode == ErrorCodes.ERROR_BUSY)
                    {
                        // this may cause infinite loop
                        Thread.Sleep(100);
                        retry = true;
                    }
                    else
                    {
                        MessageBox.Show("Error occurred :" + exception.Message);
                    }
                }

            } while (retry);
        }


        private async void StartCountDown(int v)
        {
            for (int countdown = v ; countdown > 0; countdown--)
            {
                TxtCountDown.Inlines.Add(countdown.ToString());
                await Task.Delay(1000);
                TxtCountDown.Text = "";
            }
            TxtCountDown.Inlines.Add("Lookup at the Camera");
            await Task.Delay(1500);
            TxtCountDown.Text = "";
        }

        private void DeviceManager_CameraConnected(ICameraDevice cameraDevice)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                btnCamera.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 3, 159, 25));
            }));

           RefreshDisplay();
        }

        private void DeviceManager_CameraSelected(ICameraDevice oldcameraDevice, ICameraDevice newcameraDevice)
        {
            {
                var CameraConnected = DeviceManager.SelectedCameraDevice.GetCapability(CapabilityEnum.LiveView);
                
            };
        }
        private void RefreshDisplay()
        {
            foreach (ICameraDevice cameraDevice in DeviceManager.ConnectedDevices)
            {
               // cmb_cameras.Items.Add(cameraDevice);
                CameraDevice = cameraDevice;
            }
        }

        private async void BtnCapture_Click(object sender, RoutedEventArgs e)
        {
            //GridCountDown.Visibility = Visibility.Visible;
            SBShowCountDown.Begin();
            await Task.Delay(4000);
            DeviceManager.SelectedCameraDevice.CapturePhoto();
            SBShowCountDown.Begin();
            await Task.Delay(4000);
            DeviceManager.SelectedCameraDevice.CapturePhoto();
            SBShowCountDown.Begin();
            await Task.Delay(4000);
            DeviceManager.SelectedCameraDevice.CapturePhoto();
            SBShowCountDown.Begin();
            await Task.Delay(4000);
            DeviceManager.SelectedCameraDevice.CapturePhoto();
            //GridCountDown.Visibility = Visibility.Collapsed;
        }

    }
}
