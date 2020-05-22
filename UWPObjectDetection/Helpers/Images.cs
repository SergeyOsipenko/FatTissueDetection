using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI;

namespace UWPObjectDetection.Helpers
{
    public static class Images
    {
        public static async Task<IEnumerable<StorageFile>> GetImagesFromExecutingFolder()
        {
            var folder = await StorageFolder.GetFolderFromPathAsync($"{Environment.CurrentDirectory}\\Assets\\Fat Tissue");

            var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, new List<string>() { ".jpg", ".png", ".bmp" });

            return await folder.CreateFileQueryWithOptions(queryOptions)?.GetFilesAsync();
        }

        public static async Task<IEnumerable<StorageFile>> PickImages()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");

            List<StorageFile> files = (await picker.PickMultipleFilesAsync()).ToList();

            return files;
        }

        public static async Task<VideoFrame> GetVideoFrame(StorageFile file)
        {
            SoftwareBitmap softwareBitmap;
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream 
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file in BGRA8 format
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                return VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
            }
        }

        public static async void SaveImage(Tuple<StorageFile, IList<PredictionModel>> prediction)
        {
            // grab output file
            StorageFolder storageFolder = KnownFolders.SavedPictures;
            var file = await storageFolder.CreateFileAsync($"{prediction.Item1.DisplayName}-result.jpg", CreationCollisionOption.ReplaceExisting);

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, Constants.IMAGE_WIDTH, Constants.IMAGE_HEIGHT, 96);

            // grab your input file from Assets folder
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.White);
                var image = await CanvasBitmap.LoadAsync(device, await prediction.Item1.OpenAsync(FileAccessMode.Read));

                ds.DrawImage(image);

                foreach (var pr in prediction.Item2)
                {
                    var rectangle = new Rect(
                        pr.BoundingBox.Left * Constants.IMAGE_WIDTH,
                        pr.BoundingBox.Top * Constants.IMAGE_HEIGHT,
                        pr.BoundingBox.Width * Constants.IMAGE_WIDTH,
                        pr.BoundingBox.Height * Constants.IMAGE_HEIGHT
                    );
                    ds.DrawRectangle(rectangle, Colors.Red);
                }
            }

            // save results
            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Jpeg, 1f);
            }
        }
    }
}
