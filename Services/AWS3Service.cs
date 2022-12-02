using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace Residence.Services;

public class AWS3Service
{
    private readonly IAmazonS3 _aws3Client;
    private readonly string _bucketName;
    public AWS3Service(IConfiguration configuration)
    {
        var AWSServiceSettings = "AWSServiceSettings";
        var credentials = new BasicAWSCredentials(configuration[$"{AWSServiceSettings}:AccessKey"], configuration[$"{AWSServiceSettings}:SecretAccessKey"]);
        var region = RegionEndpoint.GetBySystemName(configuration[$"{AWSServiceSettings}:Region"]);
        var config = new AmazonS3Config
        {
            RegionEndpoint = region
        };
        _bucketName = configuration[$"{AWSServiceSettings}:BucketName"];
        _aws3Client = new AmazonS3Client(credentials, config);
    }

    public async Task<byte[]> DownloadFileAsync(string file)
    {
        MemoryStream? ms = null;

        try
        {
            GetObjectRequest getObjectRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = file
            };

            using (var response = await _aws3Client.GetObjectAsync(getObjectRequest))
            {
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    using (ms = new MemoryStream())
                    {
                        await response.ResponseStream.CopyToAsync(ms);
                    }
                }
            }

            if (ms is null || ms.ToArray().Length < 1)
                throw new FileNotFoundException(string.Format("The document '{0}' is not found", file));

            return ms.ToArray();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> UploadFileAsync(IFormFile file, string key)
    {
        try
        {
            using (var newMemoryStream = new MemoryStream())
            {
                await file.CopyToAsync(newMemoryStream);

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = newMemoryStream,
                    Key = key,
                    BucketName = _bucketName,
                    ContentType = file.ContentType
                };

                var fileTransferUtility = new TransferUtility(_aws3Client);

                await fileTransferUtility.UploadAsync(uploadRequest);

                return true;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
}