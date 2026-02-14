using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Cetus.Api.Requests.Aws;
using Infrastructure.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Cetus.Api.Controllers;

[Authorize]
[ApiController]
[EnableRateLimiting("fixed")]
[Route("api/[controller]")]
public class AwsController(IOptions<AwsSettings> options, ILogger<AwsController> logger)
    : ControllerBase
{
    private const string FailedToGeneratePreSignedUrl = "Failed to generate pre-signed URL";

    private readonly AwsSettings _settings = options.Value;

    [HttpPost("s3/presigned-url")]
    public async Task<IActionResult> GetPreSignedUrl([FromBody] CreateSignedUrlRequest request)
    {
        string region = _settings.Region;
        string bucketName = _settings.BucketName;

        if (string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(bucketName))
        {
            logger.LogError("AWS configuration is missing");
            return BadRequest("AWS configuration is missing");
        }

        string accessKey = _settings.AccessKey;
        string secretKey = _settings.SecretKey;

        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
        {
            logger.LogError("AWS credentials are missing");
            return BadRequest("AWS credentials are missing");
        }

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        using var s3Client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(region));

        try
        {
            var requestObject = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = request.FileName,
                Verb = HttpVerb.PUT,
                Expires = DateTime.Now.AddMinutes(5)
            };

            string? url = await s3Client.GetPreSignedURLAsync(requestObject);

            if (!string.IsNullOrWhiteSpace(url))
            {
                return Ok(new { Url = url });
            }

            logger.LogError(FailedToGeneratePreSignedUrl);
            return BadRequest(FailedToGeneratePreSignedUrl);
        }
        catch (Exception e)
        {
            logger.LogError(e, FailedToGeneratePreSignedUrl);
            return BadRequest(FailedToGeneratePreSignedUrl);
        }
    }
}
