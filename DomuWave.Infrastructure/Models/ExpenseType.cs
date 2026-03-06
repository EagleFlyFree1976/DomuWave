namespace DomuWave.Services.Models
{
    public class ExpenseType
    {
        public virtual int    Id   { get; set; }
        public virtual string Name { get; set; }

        public const int Manutenzione = 1;
        public const int Pulizie      = 2;
        public const int Sicurezza    = 3;
        public const int Utenze       = 4;
        public const int Professionale = 5;
        public const int Altro        = 6;
    }
}
