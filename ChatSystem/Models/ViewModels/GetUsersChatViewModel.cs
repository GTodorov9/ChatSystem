namespace ChatSystem.Models.ViewModels
{
    public class GetUsersChatViewModel
    {
        public string CurrentUser_Id { get; set; }
        public string ChatWithUser_Id { get; set; }
        public List<MessageViewModel> Messages { get; set; } = new();
    }
}
