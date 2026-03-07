namespace DomuWave.Services.Clients.Request
{
    public class CreateAuthUserRequest
    {
        public string Email      { get; set; } = string.Empty;
        public string Password   { get; set; } = string.Empty;
        public string Name       { get; set; } = string.Empty;
        public string SurName    { get; set; } = string.Empty;
        public string RoleCode   { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = "DomuWeb";
    }
}
