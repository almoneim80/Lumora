﻿using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Serilog;
using Lumora.Configuration;
using Lumora.DTOs;
using Lumora.Helpers;

namespace Lumora.Services;

public class IpDetailsService
{
    protected static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions();

    private readonly IOptions<GeolocationApiConfig> options;

    public IpDetailsService(IOptions<GeolocationApiConfig> options)
    {
        this.options = options;
        JsonHelper.Configure(SerializeOptions, JsonNamingConvention.SnakeCase);
    }

    public async Task<IpDetailsDto?> GetIpDetails(string ip)
    {
        IpDetailsDto? ipDetailsDto;

        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var queryParams = new Dictionary<string, string>
        {
            ["apiKey"] = options.Value.AuthKey,
            ["ip"] = ip,
        };

        var response = await client.GetAsync(QueryHelpers.AddQueryString(options.Value.Url, queryParams!));

        if (response.IsSuccessStatusCode)
        {
            ipDetailsDto = JsonSerializer.Deserialize<IpDetailsDto>(response.Content.ReadAsStringAsync().Result, SerializeOptions);

            Log.Information("Success of resolving {0}", ipDetailsDto!.Ip!);
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            throw new IpDetailsException(responseContent);
        }

        return ipDetailsDto;
    }
}
