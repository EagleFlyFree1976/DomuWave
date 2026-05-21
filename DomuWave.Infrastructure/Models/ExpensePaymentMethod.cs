namespace DomuWave.Services.Models
{
    public class ExpensePaymentMethod
    {
        public virtual int    Id   { get; set; }
        public virtual string Name { get; set; }

        public const int Contanti                  = 1;
        public const int CartaCreditoDebito        = 2;
        public const int AssegnoBancario           = 3;
        public const int AssegnoCircolare          = 4;
        public const int BonificoBancarioImm       = 5;
        public const int BonificoBancario30        = 6;
        public const int BonificoBancario60        = 7;
        public const int BonificoBancario90        = 8;
        public const int RidSdd                    = 9;
        public const int RiBa                      = 10;
        public const int Mav                       = 11;
        public const int FatturaFineMese           = 12;
        public const int FatturaFineMesePlus30     = 13;
        public const int PayPal                    = 14;
        public const int PagoPA                    = 15;
    }
}
