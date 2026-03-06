namespace DomuWave.Services.Models
{
    public class ExpensePaymentStatus
    {
        public virtual int    Id   { get; set; }
        public virtual string Name { get; set; }

        public const int DaPagare = 1;
        public const int Pagata   = 2;
        public const int Scaduta  = 3;
        public const int PagataParzialmente  = 4;
    }
}
