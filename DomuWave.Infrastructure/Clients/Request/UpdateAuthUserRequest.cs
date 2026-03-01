namespace DomuWave.Services.Clients.Request
{
    public class UpdateAuthUserRequest
    {
        public string? Name     { get; set; }
        public string? SurName  { get; set; }
        public string? Email    { get; set; }
        public bool    IsActive { get; set; }
        public string? RoleCode { get; set; }
    }
}
