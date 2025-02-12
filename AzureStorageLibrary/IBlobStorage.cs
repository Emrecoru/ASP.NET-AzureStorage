using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageLibrary
{
    public enum ContainerNameEnum
    {
        pictures,
        pdf,
        logs,
        watermarkpictures
    }

    public interface IBlobStorage
    {
        public string BlobUrl { get; }

        Task UploadAsync(Stream fileStream, string fileName, ContainerNameEnum containerNameEnum);

        Task<Stream> DownloadAsync(string fileName, ContainerNameEnum containerNameEnum);

        Task DeleteAsync(string fileName, ContainerNameEnum containerNameEnum);

        Task SetLogAsync(string text, string fileName);

        Task<List<string>> GetLogAsync(string fileName);

        List<string> GetNames(ContainerNameEnum containerNameEnum);
    }
}
