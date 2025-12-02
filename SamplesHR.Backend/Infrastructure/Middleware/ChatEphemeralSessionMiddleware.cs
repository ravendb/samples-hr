namespace SamplesHR.Backend.Infrastructure.Middleware;

public class ChatEphemeralSessionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext http)
    {
        if (!http.Request.Cookies.TryGetValue(Constants.Cookies.SessionId, out var sessionId))
        {
            sessionId = Guid.NewGuid().ToString();

            http.Response.Cookies.Append(Constants.Cookies.SessionId, sessionId, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false // todo: HTTPS -> true
            });
        }

        http.Items["SessionId"] = sessionId;
        http.Items["UserAgent"] = http.Request.Headers.UserAgent.ToString();
        http.Items["ClientIP"] = http.Connection.RemoteIpAddress?.ToString();

        http.Items["AcceptLanguage"] = http.Request.Headers.AcceptLanguage.ToString();
        http.Items["Referer"] = http.Request.Headers.Referer.ToString();

        http.Items["UtmSource"] = http.Request.Query["utm_source"].ToString();
        http.Items["UtmMedium"] = http.Request.Query["utm_medium"].ToString();
        http.Items["UtmCampaign"] = http.Request.Query["utm_campaign"].ToString();
        http.Items["UtmTerm"] = http.Request.Query["utm_term"].ToString();
        http.Items["UtmContent"] = http.Request.Query["utm_content"].ToString();

        await next(http);
    }
}