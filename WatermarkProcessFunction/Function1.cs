using System;
using AzureStorageLibrary.Models;
using AzureStorageLibrary.Services;
using AzureStorageLibrary;
using System.Drawing;
using System.IO;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;
using System.Threading.Tasks;
using System.Drawing.Imaging;

namespace WatermarkProcessFunction
{
    public class Function1
    {
        [FunctionName("Function1")]
        public async Task Run([QueueTrigger("watermarkqueue", Connection = "http:::127.0.0.1:10001:devstoreaccount1")]PictureWatermarkQueue myQueueItem, ILogger log)
        {
            ConnectionString.AzureConnectionString = "***";

            IBlobStorage blobStorage = new BlobStorage();
            INoSqlStorage<UserPicture> noSqlStorage = new TableStorage<UserPicture>();

            foreach (var item in myQueueItem.Pictures)
            {
                using var stream = await blobStorage.DownloadAsync(item, ContainerNameEnum.pictures);

                using var memoryStream = AddWaterMark(myQueueItem.WatermarkText, stream);

                await blobStorage.UploadAsync(memoryStream, item, ContainerNameEnum.watermarkpictures);

                log.LogInformation($"{item} resmine watermark eklenmiþtir.");
            }

            var userpicture = await noSqlStorage.Get(myQueueItem.UserId, myQueueItem.City);

            if (userpicture.WatermarkRawPaths != null)
            {
                myQueueItem.Pictures.AddRange(userpicture.WatermarkPaths);
            }

            userpicture.WatermarkPaths = myQueueItem.Pictures;

            await noSqlStorage.Add(userpicture);


            HttpClient httpClient = new HttpClient();

            var response = await httpClient.GetAsync("https://localhost:7068/api/Notification/CompleteWatermarkProcess/" + myQueueItem.ConnectionId);

            log.LogInformation($"Client({myQueueItem.ConnectionId}) bilgilendirilmiþtir");
        }

        public static MemoryStream AddWaterMark(string watermarkText, Stream PictureStream)
        {
            MemoryStream ms = new MemoryStream();

            using (System.Drawing.Image image = Bitmap.FromStream(PictureStream))
            {
                using (Bitmap tempBitmap = new Bitmap(image.Width, image.Height))
                {
                    using (Graphics gph = Graphics.FromImage(tempBitmap))
                    {
                        gph.DrawImage(image, 0, 0);

                        var font = new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold);

                        var color = Color.FromArgb(255, 0, 0);

                        var brush = new SolidBrush(color);

                        var point = new Point(20, image.Height - 50);

                        gph.DrawString(watermarkText, font, brush, point);

                        tempBitmap.Save(ms, ImageFormat.Png);
                    }
                }
            }

            ms.Position = 0;

            return ms;
        }
    }
}
