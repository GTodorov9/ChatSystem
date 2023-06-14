namespace ChatSystem.Models.DataModels
{
    public class Message
    {
        public string Content { get; set; }
        public string FromUser_Id { get; set; }
        public string ToUser_Id { get; set; }
        public DateTime PublishedOn { get; set; }

    }
}
