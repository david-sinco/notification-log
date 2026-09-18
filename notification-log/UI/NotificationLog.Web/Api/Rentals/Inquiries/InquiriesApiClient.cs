using System.Net.Http.Json;

namespace NotificationLog.Web.Api.Rentals.Inquiries;

public sealed class InquiriesApiClient(HttpClient http)
{
    public Task<PagedResult<InquirySummaryDto>> ListAsync(
        Guid? listingId, Guid? seekerId, bool? isClosed, int page, int pageSize, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("listingId", listingId?.ToString()),
            ("seekerId", seekerId?.ToString()),
            ("isClosed", isClosed?.ToString()),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        return http.GetJsonAsync<PagedResult<InquirySummaryDto>>($"/api/inquiries{query}", ct);
    }

    public Task<InquiryDto> GetByIdAsync(Guid id, CancellationToken ct)
        => http.GetJsonAsync<InquiryDto>($"/api/inquiries/{id}", ct);

    public async Task<Guid> SendMessageAsync(SendInquiryMessageRequest request, CancellationToken ct)
    {
        var response = await http.SendJsonAsync(HttpMethod.Post, "/api/inquiries", request, ct);
        var body = await response.Content.ReadFromJsonAsync<SentInquiryMessageResponse>(ApiJson.Options, ct);
        return body!.InquiryId;
    }

    public Task ReplyAsync(Guid id, Guid actorId, string message, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/inquiries/{id}/replies", new ReplyToInquiryRequest(actorId, message), ct);
}
