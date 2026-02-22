namespace AttandanceSyncApp.Models.DTOs.Auth
{
    public class GoogleAuthDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string GoogleId { get; set; }
        public string IdToken { get; set; }
    }

}
