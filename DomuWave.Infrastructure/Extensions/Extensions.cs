using DomuWave.Services.Models;

namespace DomuWave.Services.Extensions;

public static class Extensions
{
    public static FlowDirection ToFlowDirection(this TransactionType transactionType, FlowDirection defaultDirection)
    {
        return transactionType switch
        {
            TransactionType.Uscita => FlowDirection.Out,
            TransactionType.Entrata => FlowDirection.In,
            _ =>defaultDirection
        };
    }

    /// <summary>
    /// Restituisce la data all'inizio della giornata (00:00:00.000).
    /// </summary>
    public static DateTime StartOfDay(this DateTime date)
    {
        return date.Date;
    }   

    /// <summary>
    /// Restituisce la data al termine della giornata (23:59:59.999).
    /// </summary>
    public static DateTime EndOfDay(this DateTime date)
    {
        return date.Date.AddDays(1).AddTicks(-1);
    }

    public static DateTime FirstDayOfWeek(this DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }
    public static DateTime LastDayOfWeek(this DateTime date)
    {
        return date.FirstDayOfWeek().AddDays(6).EndOfDay();
        
    }

    public static DateTime FirstDayOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }
    public static DateTime LastDayOfMonth(this DateTime date)
    {
        return date.FirstDayOfMonth().AddMonths(1).AddDays(-1).EndOfDay();
    }
    public static DateTime LastDayOfYear(this DateTime date)
    {
        return new DateTime(date.Year, 12, 31).EndOfDay();
    }

    public static DateTime FirstDayOfYear(this DateTime date)
    {
        return new DateTime(date.Year, 1, 1);
    }

    /// <summary>
    ///  ritorna il primo giorno del trimestre della data specificata
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static DateTime FirstDayOfQuarter(this DateTime date)
    {
        int quarter = GetQuarter(date);
        DateTime firstDay = date.FirstDayOfYear();

        firstDay = firstDay.AddMonths((quarter - 1) * 3);
        return firstDay;
    }

    /// <summary>
    /// ritorna l'ultimo giorno del trimestre della data specificata
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static DateTime LastDayOfQuarter(this DateTime date)
    {
        int quarter = GetQuarter(date);

        DateTime firstDay = date.FirstDayOfQuarter();
        DateTime lastDay = firstDay.AddMonths(3).AddDays(-1).EndOfDay();
        return lastDay;
    }
    /// <summary>
    /// ritorna il trimestre (1-4) della data specificata
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static int GetQuarter(DateTime date)
    {
        return (date.Month - 1) / 3 + 1;
    }
}