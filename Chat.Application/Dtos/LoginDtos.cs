namespace Chat.Application.Dtos
{
    public class LoginRequest
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
