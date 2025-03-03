using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Cetus.Api.Requests.Aws;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AwsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AwsController> _logger;

    public AwsController(IConfiguration configuration, ILogger<AwsController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("s3/presigned-url")]
    public async Task<IActionResult> GetPreSignedUrl([FromBody] CreateSignedUrlRequest request)
    {
        var region = _configuration["AWS:Region"];
        var bucketName = _configuration["AWS:BucketName"];

        if (string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(bucketName))
        {
            _logger.LogError("AWS configuration is missing");
            return BadRequest("AWS configuration is missing");
        }

        var accessKey = _configuration["AWS:AccessKey"];
        var secretKey = _configuration["AWS:SecretKey"];

        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
        {
            _logger.LogError("AWS credentials are missing");
            return BadRequest("AWS credentials are missing");
        }

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var s3Client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(region));

        try
        {
            var requestObject = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = request.FileName,
                Verb = HttpVerb.PUT,
                Expires = DateTime.Now.AddMinutes(5)
            };

            var url = await s3Client.GetPreSignedURLAsync(requestObject);

            if (!string.IsNullOrWhiteSpace(url))
            {
                return Ok(new {Url = url});
            }

            _logger.LogError("Failed to generate pre-signed URL");
            return BadRequest("Failed to generate pre-signed URL");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to generate pre-signed URL");
            return BadRequest("Failed to generate pre-signed URL");
        }
    }
}
