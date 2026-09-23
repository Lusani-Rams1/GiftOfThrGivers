using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net;

namespace GiftOfTheGivers.Functions
{
    public class GenerateTaxCertificate
    {
        private readonly ILogger<GenerateTaxCertificate> _logger;

        public GenerateTaxCertificate(ILogger<GenerateTaxCertificate> logger)
        {
            _logger = logger;
        }

        [Function("GenerateTaxCertificate")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {

            _logger.LogInformation("Processing donation for tax certificate generation.");
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            var donation = JsonSerializer.Deserialize<DonationRequest>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (donation == null || string.IsNullOrWhiteSpace(donation.DonorName))
            {
                return new BadRequestObjectResult("Please provide donorName and amount.");
            }

            var certificateNumber = $"GOTG-TAX-{DateTime.UtcNow:yyyyMMddHHmmss}";

            _logger.LogInformation($"Generated certificate {certificateNumber} for {donation.DonorName}");

            return new OkObjectResult(new
                {
                    CertificateNumber = certificateNumber,
                    DonorName = donation.DonorName,
                    Amount = donation.Amount,
                    DateIssued = DateTime.UtcNow,
                    Message = "Tax certificate generated successfully."
            });
        }
    }

    public class DonationRequest
    {
        public required string DonorName { get; set; }
        public decimal Amount { get; set; }
    }
}
