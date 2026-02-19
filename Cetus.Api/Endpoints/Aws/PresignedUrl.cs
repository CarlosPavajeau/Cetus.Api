using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Infrastructure.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Cetus.Api.Endpoints.Aws;

internal sealed class PresignedUrl : IEndpoint
{
    private sealed record Request(string FileName);
    private sealed record Response(string Url);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("aws/s3/presigned-url", async (
            [FromBody] Request request,
            IOptions<AwsSettings> options,
            ILogger<PresignedUrl> logger
        ) =>
        {
            string region = options.Value.Region;
            string accessKey = options.Value.AccessKey;
            string secretKey = options.Value.SecretKey;
            string bucketName = options.Value.BucketName;

            if (string.IsNullOrWhiteSpace(region) ||
                string.IsNullOrWhiteSpace(bucketName) ||
                string.IsNullOrWhiteSpace(accessKey) ||
                string.IsNullOrWhiteSpace(secretKey)
               )
            {
                logger.LogError("AWS configuration is missing");
                return Results.InternalServerError("Could not generate pre-signed URL");
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

                string url = await s3Client.GetPreSignedURLAsync(requestObject);

                return Results.Ok(new Response(url));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to generate pre-signed URL");
                return Results.InternalServerError("Failed to generate pre-signed URL");
            }
        }).WithTags(Tags.Aws);
    }
}
