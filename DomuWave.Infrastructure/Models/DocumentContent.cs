namespace DomuWave.Services.Models
{
    public class DocumentContent : TenantEntity<int>
    {
        public virtual Document Document    { get; set; } = null!;
        public virtual byte[]   FileContent { get; set; } = Array.Empty<byte>();

        public override int GetHashCode() => Id.GetHashCode();
    }
}
