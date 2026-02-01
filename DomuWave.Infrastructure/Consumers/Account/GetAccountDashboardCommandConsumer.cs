using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Book;

public class GetAccountDashboardCommandConsumer : InMemoryConsumerBase<GetAccountDashboardCommand, AccountDashboardDto>
{
    private readonly IBookService _bookService;
    private readonly IUserService _userService;
    private readonly IMediator _mediator;
    private readonly ICoreAuthorizationManager _authorizationManager;


    public GetAccountDashboardCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ICoreAuthorizationManager authorizationManager, IBookService bookService, IMediator mediator, IUserService userService) : base(sessionFactoryProvider)
    {
        _authorizationManager = authorizationManager;
        _bookService = bookService;
        _mediator = mediator;
        _userService = userService;
    }

    protected override async Task<AccountDashboardDto> Consume(GetAccountDashboardCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);


        if (currentUser != null)
        {
            Models.Book book = null;
            var bookId = evt.BookId;

            book = await session.Query<Models.Book>().GetQueryable().FilterByOwner(currentUser)
                .Where(k => k.Id == bookId).FirstOrDefaultAsync(cancellationToken);

            if (book == null)
            {

                throw new NotFoundException("Elemento non trovato");
            }

            Models.Account account = await session.Query<Models.Account>().GetQueryable().FilterByBook(book)
                .Where(k => k.Id == evt.AccountId).FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (account == null)
            {
                throw new NotFoundException("Elemento non trovato");
            }

            AccountDashboardDto accountDashboardDto = new AccountDashboardDto();

            var accountReports = await session.Query<AccountReport>().Where(k => k.Account.Id == evt.AccountId)
                .ToListAsync(cancellationToken);


            accountDashboardDto.AccountBalance = accountReports
                .FirstOrDefault(l => l.ReportKey == AccountReport.ReportActualBalance)?.ReportValue;
            accountDashboardDto.AvailableBalance = account.Balance;
            accountDashboardDto.Totals = new List<MovementTotal>();
         
            
            
            accountDashboardDto.Totals.AddRange(GetPeriodData(accountReports, AccountReport.ReportCurrentMonthAllInput, AccountReport.ReportCurrentMonthAllOutput, Period.CurrentMonth));
            accountDashboardDto.Totals.AddRange(GetPeriodData(accountReports, AccountReport.ReportCurrentWeekAllInput, AccountReport.ReportCurrentWeekAllOutput, Period.CurrentWeek));
            accountDashboardDto.Totals.AddRange(GetPeriodData(accountReports, AccountReport.ReportCurrentDayAllInput, AccountReport.ReportCurrentDayAllOutput, Period.CurrentDay));
            accountDashboardDto.Totals.AddRange(GetPeriodData(accountReports, AccountReport.ReportCurrentQuarterAllInput, AccountReport.ReportCurrentQuarterAllOutput, Period.CurrentQuarter));
            accountDashboardDto.Totals.AddRange(GetPeriodData(accountReports, AccountReport.ReportLastMonthAllInput, AccountReport.ReportLastMonthAllOutput, Period.PreviousMonth));
            accountDashboardDto.Totals.AddRange(GetPeriodData(accountReports, AccountReport.ReportLastWeekAllInput, AccountReport.ReportLastWeekAllOutput, Period.PreviousWeek));
            accountDashboardDto.Totals.AddRange(GetPeriodData(accountReports, AccountReport.ReportLastDayAllInput, AccountReport.ReportLastDayAllOutput, Period.PreviousDay));
            accountDashboardDto.Totals.AddRange(GetPeriodData(accountReports, AccountReport.ReportLastQuarterAllInput, AccountReport.ReportLastQuarterAllOutput, Period.PreviousQuarter));
            //accountDashboardDto.Totals.AddRange(GetPeriodData(accountReports, AccountReport.ReportLastWeekAllInput, AccountReport.reportlast, Period.PreviousDay);

            accountDashboardDto.ClosedDate = account.ClosedDate;
            accountDashboardDto.OpenDate = account.OpenDate;
            accountDashboardDto.AccountId = account.Id;
            accountDashboardDto.AccountName = account.Name;
            return accountDashboardDto;


        }
        else
        {
            throw new UserNotAuthorizedException("Utente non autorizzato");
        }

        return null;
    }

    private IList<MovementTotal> GetPeriodData(List<AccountReport> accountReports, string inputAllKey, string outputAllKey, Period period)
    {
        IList<MovementTotal> returnList = new List<MovementTotal>();
        MovementTotal mo;
        PeriodDefinition currentMonth = new PeriodDefinition();

        var currentMonthAllImport = accountReports.FirstOrDefault(j => j.ReportKey == inputAllKey);
        var currentMonthAllOutput = accountReports.FirstOrDefault(j => j.ReportKey == outputAllKey);
        if (currentMonthAllOutput != null && currentMonthAllImport != null)
        {
            currentMonth.Period = period;
            currentMonth.FirstDay = currentMonthAllImport.StartDate.GetValueOrDefault();
            currentMonth.LastDay = currentMonthAllImport.EndDate.GetValueOrDefault();

            MovementTotal m = new MovementTotal()
            {
                MovementType = MovementType.Deposit,
                Period = currentMonth,
                Amount = currentMonthAllImport.ReportValue
            };
            mo = new MovementTotal()
            {
                MovementType = MovementType.Expense,
                Period = currentMonth,
                Amount = currentMonthAllOutput.ReportValue
            };
            returnList.Add(m);
            returnList.Add(mo);
        }

        return returnList;
    }
}