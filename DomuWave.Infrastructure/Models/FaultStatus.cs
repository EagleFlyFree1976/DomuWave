namespace DomuWave.Services.Models
{
    public class FaultStatus
    {
        public virtual int    Id   { get; set; }
        public virtual string Name { get; set; } = string.Empty;

        public override int GetHashCode() => Id.GetHashCode();
    }
}
