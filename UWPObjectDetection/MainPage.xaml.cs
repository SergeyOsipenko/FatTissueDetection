using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using UWPObjectDetection.Helpers;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using Windows.Media;
using Windows.UI.ViewManagement;
using Windows.UI.Popups;
using Windows.System;

namespace UWPObjectDetection
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(1280, 720);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void RunObjectDetection_Click(object sender, RoutedEventArgs e)
        {
            RunObjectDetection();
        }

        private async void RunObjectDetection()
        {
            var detection = new ObjectDetection(new List<string> { "Fat Tissue" });
            StorageFile modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/model.onnx"));
            await detection.Init(modelFile);

            var images = await Images.PickImages();
            double step = prBar.Maximum / images.Count();
            int count = 0;

            VideoFrame videoFrame = null;
            List<PredictionModel> predictions = null;

            btnRun.IsEnabled = false;
            prBar.Visibility = Visibility.Visible;
            prText.Visibility = Visibility.Visible;

            foreach (StorageFile image in images)
            {
                videoFrame = await Images.GetVideoFrame(image);

                predictions = (await detection.PredictImageAsync(videoFrame))
                    .Where(predict => predict.Probability > Constants.TRESHHOLD)
                    .ToList();

                if(predictions.Count > 0)
                {
                    count++;
                    prBar.Value += step;
                    Images.SaveImage(new Tuple<StorageFile, IList<PredictionModel>>(image, predictions));
                }
            }

            prBar.Visibility = Visibility.Collapsed;
            prText.Visibility = Visibility.Collapsed;
            btnRun.IsEnabled = true;

            var dialog = new MessageDialog($"Всего {count} изображений содержит жировую ткань");
            await dialog.ShowAsync();

            await Launcher.LaunchFolderAsync(KnownFolders.SavedPictures);
        }
    }
}
