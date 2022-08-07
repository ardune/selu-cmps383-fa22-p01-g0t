using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Net.Http.Headers;

namespace SP22.P01.Tests.Web.Helpers;

public sealed class WebTestContext : IDisposable
{
    public WebTestContext()
    {
        Server = new WebHostFactory<Program>();
    }

    public WebHostFactory<Program> Server { get; }

    public HttpClient GetStandardWebClient()
    {
        var cookieContainer = new CookieContainer(100);
        return Server.CreateDefaultClient(new RedirectHandler(10), new NonSecureCookieHandler(cookieContainer));
    }

    public void Dispose()
    {
        Server.Dispose();
    }

    public class WebHostFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder x)
        {
            x.ConfigureAppConfiguration(y =>
            {
                y.Add(new MemoryConfigurationSource
                {
                    InitialData = new List<KeyValuePair<string, string>>
                    {
                        new("Logging:LogLevel:Microsoft", "Error"),
                        new("Logging:LogLevel:Microsoft.Hosting.Lifetime", "Error"),
                        new("Logging:LogLevel:Default", "Error")
                    }
                });
            });
            base.ConfigureWebHost(x);
        }
    }

    public class NonSecureCookieHandler : DelegatingHandler
    {
        private readonly CookieContainer cookieContainer;

        public NonSecureCookieHandler(CookieContainer cookieContainer)
        {
            this.cookieContainer = cookieContainer;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var cookieHeader = cookieContainer.GetCookieHeader(request.RequestUri ?? throw new Exception("No request uri"));

            request.Headers.Add(HeaderNames.Cookie, cookieHeader);

                var response = await base.SendAsync(request, cancellationToken);
                if (response.Headers.TryGetValues(HeaderNames.SetCookie, out var values))
                {
                    foreach (var header in values)
                    {
                        // HACK: we cannot test on https so we have to force it to be insecure
                        var keys = header.Split("; ").Where(x => !string.Equals(x, "secure"));
                        var result = string.Join("; ", keys);
                        cookieContainer.SetCookies(response.RequestMessage?.RequestUri ?? throw new Exception("No request uri"), result);
                    }
                }

                return response;
            }
        }
    }
