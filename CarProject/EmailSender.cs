using System.Text;
using System.Text.Json;

namespace WebApplication1;

public class EmailSender
{
    private static readonly string ApiKey =
        Environment.GetEnvironmentVariable("BREVO_API_KEY")
        ?? throw new Exception("BREVO_API_KEY environment variable is missing");

    public void SendEmail(string to, string subject, string body)
    {
        Task.Run(async () =>
        {
            try
            {
                using var client = new HttpClient();

                client.DefaultRequestHeaders.Add("api-key", ApiKey);

                var payload = new
                {
                    sender = new
                    {
                        name = "BINK. Publishers",
                        email = "giorgimeshveliani03@gmail.com"
                    },
                    to = new[] { new { email = to } },
                    subject = subject,
                    htmlContent = body
                };

                var json = JsonSerializer.Serialize(payload);

                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(
                    "https://api.brevo.com/v3/smtp/email",
                    content
                );

                var result = await response.Content.ReadAsStringAsync();

                Console.WriteLine(
                    $"EMAIL RESULT: {response.StatusCode} - {result}"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EMAIL ERROR: {ex.Message}");
            }
        });
    }
}