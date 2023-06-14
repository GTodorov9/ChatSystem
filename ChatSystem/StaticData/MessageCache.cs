using ChatSystem.Models.DataModels;

namespace ChatSystem.StaticData
{
    public static class MessageCache
    {
        public static List<Message> Messages { get; set; } = new();

        public static List<Message> GetUserMessages(string user_Id, string toUser_Id)
        {
            return Messages.Where(e => (e.FromUser_Id == user_Id && e.ToUser_Id == toUser_Id)
                                    || (e.FromUser_Id == toUser_Id && e.ToUser_Id == user_Id)).ToList();
        }
    }
}
