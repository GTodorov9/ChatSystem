using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.StaticData;
using Microsoft.Extensions.Hosting;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChatSystem.HostedServices;

public class HostedService : IHostedService, IDisposable
{
    private readonly HttpClient _httpClient;
    private Timer _timer;
    private string _messageAPIAddress;
    private readonly IConfiguration _configuration;
    private int _NMinutes;
    public HostedService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _configuration = configuration;
        _messageAPIAddress = _configuration["messageAPIAddress"];
        _NMinutes = int.Parse(_configuration["SendDataEveryNMinutes"].ToString());
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(_NMinutes));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private async void DoWork(object state)
    {
        var messages = MessageCache.Messages;
        // save to db
        string apiUrl = string.Format("{0}/api/Message/SaveMessages", _messageAPIAddress);
        if (messages.Count > 0)
        {
            var bindingModelMessages = messages.Select(e => new
            {
                Content = e.Content,
                TimeStamp = e.PublishedOn,
                FromUser_Id = e.FromUser_Id,
                ToUser_Id = e.ToUser_Id
            }).ToList();
            var bindingModel = new
            {
                Messages = bindingModelMessages,
            };
            string requestBody = JsonSerializer.Serialize(bindingModel);
            StringContent content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                //We stored the data in the DB and we delete the memory cache
                MessageCache.Messages = new();
            }
        }

    }
}