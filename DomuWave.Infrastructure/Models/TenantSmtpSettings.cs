namespace DomuWave.Services.Models
{
    public class TenantSmtpSettings : TenantEntity<int>
    {
        public virtual string Host              { get; set; } = string.Empty;
        public virtual int    Port              { get; set; } = 587;
        public virtual bool   UseSsl            { get; set; } = true;
        public virtual string Username          { get; set; } = string.Empty;
        public virtual string PasswordEncrypted { get; set; } = string.Empty;
        public virtual string FromEmail         { get; set; } = string.Empty;
        public virtual string FromName          { get; set; } = string.Empty;
        public virtual bool   IsEnabled         { get; set; } = true;

        public override int GetHashCode() => Id.GetHashCode();
    }
}
