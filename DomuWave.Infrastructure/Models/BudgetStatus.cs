namespace DomuWave.Services.Models
{
    public class BudgetStatus
    {
        public virtual int    Id   { get; set; }
        public virtual string Name { get; set; }

        public const int Draft    = 1;
        public const int Approved = 2;
        public const int Closed   = 3;
    }
}
