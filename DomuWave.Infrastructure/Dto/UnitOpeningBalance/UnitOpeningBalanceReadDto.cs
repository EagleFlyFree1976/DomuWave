using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.UnitOpeningBalance;

public class UnitOpeningBalanceReadDto : TraceEntityDTO<int>
{
    public int     UnitId         { get; set; }
    public string  UnitName       { get; set; }
    public int     FiscalYearId   { get; set; }
    public string  FiscalYearCode { get; set; }

    /// <summary>Saldo di apertura. Editabile manualmente solo sul primo esercizio.</summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>Totale movimenti calcolato alla chiusura (quote addebitate - pagamenti).</summary>
    public decimal TotalMovements { get; set; }

    /// <summary>Saldo di chiusura = OpeningBalance + TotalMovements. Calcolato alla chiusura.</summary>
    public decimal ClosingBalance { get; set; }

    public string  Notes          { get; set; }

    /// <summary>True se il saldo di apertura è modificabile manualmente (solo primo esercizio senza precedente chiuso).</summary>
    public bool    IsEditable     { get; set; }

    /// <summary>True se l'esercizio è stato chiuso (ClosingBalance e TotalMovements sono definitivi).</summary>
    public bool    IsClosed       { get; set; }
}
