using ChatSystem.Hubs;
using ChatSystem.Models;
using ChatSystem.Models.ViewModels;
using ChatSystem.StaticData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChatSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<ChatSystemHub> _chatSystemHubContext;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string _messageAPIAddress;
        public HomeController(IHubContext<ChatSystemHub> chatSystemHubContext, IConfiguration configuration)
        {
            _chatSystemHubContext = chatSystemHubContext;
            _httpClient = new HttpClient();
            _configuration = configuration;
            _messageAPIAddress = _configuration["messageAPIAddress"];
        }
       
        public IActionResult GetCurrentSessionUser()
        {
            string userGuidString = HttpContext.Session.GetString("UserGuid")!;
            return Json(new { userGuidString });
        }
        public IActionResult Index()
        {
            string userGuidString = HttpContext.Session.GetString("UserGuid")!;
            var viewModel = new GetAllUsersViewModel();
            if (userGuidString == null)
            {
                Guid userGuid = Guid.NewGuid();
                HttpContext.Session.SetString("UserGuid", userGuid.ToString());
                ConnectedChatUsers.Users.Add(new Models.User { Id = userGuid.ToString() });

            }
            viewModel.CurrentSession_Id = HttpContext.Session.GetString("UserGuid")!;
            return View(viewModel);
        }
        public async Task<IActionResult> OpenUserChat(string toUserId)
        {

            string userGuidString = HttpContext.Session.GetString("UserGuid")!;
            if (string.IsNullOrWhiteSpace(userGuidString) || toUserId == userGuidString)
            {
                return RedirectToAction("Index");
            }
            var viewModel = new GetUsersChatViewModel() { CurrentUser_Id = userGuidString, ChatWithUser_Id = toUserId };
            //We check the DB for old messages
          
            string apiUrl = string.Format("{0}/api/Message/GetMessagesForUsers?fromUserId={1}&toUserId={2}", _messageAPIAddress, userGuidString, toUserId);

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<MessageViewModel> databaseMessages = JsonSerializer.Deserialize<List<MessageViewModel>>(responseBody);
                if (databaseMessages != null && databaseMessages.Count > 0)
                {
                    viewModel.Messages = databaseMessages;
                    viewModel.Messages.ForEach(e => e.CurrentSessionUser_Id = userGuidString);
                }

            }
            //We check if we have some messages in the memory cache
            var msg = MessageCache.GetUserMessages(userGuidString, toUserId).Select(e => new MessageViewModel
            {
                ToUser_Id = e.ToUser_Id,
                Content = e.Content,
                CurrentSessionUser_Id = userGuidString,
                FromUser_Id = e.FromUser_Id,
                PublishedOn = e.PublishedOn
            }).ToList();

            viewModel.Messages.AddRange(msg);
            viewModel.Messages = viewModel.Messages.OrderBy(e => e.PublishedOn).ToList();
            return View(viewModel);
        }
        [HttpGet]
        public IActionResult GetUsersReport()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetUsersReportData(DateTime fromDate, DateTime toDate)
        {
            try
            {
                string apiUrl = string.Format("{0}/api/Message/GetMessagesReport?fromDate={1}&toDate={2}", _messageAPIAddress, fromDate, toDate);

                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return Ok(responseBody);
                }
                else
                {
                    return StatusCode((int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the API call
                return BadRequest(ex.Message);
            }
        }

    }
}