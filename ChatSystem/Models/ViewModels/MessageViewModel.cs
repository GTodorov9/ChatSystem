using System.Text.Json.Serialization;

namespace ChatSystem.Models.ViewModels
{
    public class MessageViewModel
    {
        public string Id { get; set; }
        [JsonPropertyName("fromUser_Id")]
        public string FromUser_Id { get; set; }
        [JsonPropertyName("toUser_Id")]
        public string ToUser_Id { get; set; }
        [JsonPropertyName("publishedOn")]
        public DateTime PublishedOn { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
        public string CurrentSessionUser_Id { get; set; }
        public string IsCurrentUser
        {
            get
            {
                return FromUser_Id == CurrentSessionUser_Id ? "me" : "other";
            }
        }
    }
}
