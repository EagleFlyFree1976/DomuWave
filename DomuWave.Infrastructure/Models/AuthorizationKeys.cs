namespace DomuWave.Services.Models;

public class AuthorizationKeys
{
    public const string Authorizations = "AUTH";
    public const string System = "IsSystem";
    public const string Users = "users";
    public const string Tenants = "TNNT";
    public const string Accounts = "Accounts";
    public const string Admin = "Admin";

    // Condominium management
    public const string Condominiums = "Condominium";
    public const string RealEstateUnits = "RealEstateUnit";
    public const string UnitOwners = "UnitOwner";
    public const string FiscalYear = "FSCY";
    public const string Budget = "BDG";
    public const string Expenses = "Expense";
    public const string Installments = "Installment";
    public const string Communications = "Communication";
    public const string Documents = "Document";

    /// <summary>
    /// Permesso per la gestione delle comunicazioni trai condomini e l'amministratore, come ad esempio i messaggi inviati tramite la bacheca condominiale.
    /// </summary>
    public const string BoardPost = "BoardPost";
    public const string Assembly = "Assembly";

    // Nuove chiavi
    public const string Access = "Access";
    public const string UnitTenants         = "UnitTenant";
    public const string Suppliers           = "Supplier";
    public const string SupplierContracts   = "SupplierContract";
    public const string MillesimalTables    = "MillesimalTable";
    public const string UnitMillesimals     = "UnitMillesimal";
    public const string ChartOfAccounts     = "ChartOfAccounts";
    public const string ChartOfAccountsTemplate = "ChartOfAccountsTemplate";
    public const string ChartOfAccountsCategory = "ChartOfAccountsCategory";
    public const string BudgetItems         = "BudgetItem";
    public const string BillingGroups       = "BillingGroup";
    public const string Buildings           = "Building";
    public const string Staircases          = "Staircase";
    public const string Maintenance         = "Maintenance";
    public const string ExtraordinaryWorks  = "ExtraordinaryWork";
    public const string Consumption         = "Consumption";
    public const string AccountBalances     = "AccountBalance";
    public const string Dashboard           = "Dashboard";
    public const string NotificationTemplates = "NotificationTemplate";
    public const string TenantSmtp          = "TenantSmtp";
    public const string CommunicationNotifications = "CommunicationNotification";
    public const string NotificationAttachments = "NotificationAttachment";
    public const string FileAttachments     = "FileAttachment";
    public const string DynamicFiles        = "DynamicFile";
    public const string Faults              = "Fault";
    public const string PrivateThreads      = "PrivateThread";
    public const string UserTenants         = "UserTenant";
}


public class Modules
{
    public const string DomuWaveModule = "DomuWeb";
    public const string AuthModule = "AUTH";
}
    
public class Keys
{
    
    public const string DRAFT_Label = "Draft";
    public const string REC_Label = "Riconciliata";
    public const string UNREC_Label = "Non riconciliata";
    public const string VOID_Label = "Nulla";
    public const string WATCH_Label = "Da monitorare";


    public const string DRAFT = "DRAFT";
    public const string REC = "REC";
    public const string UNREC = "UNREC";
    public const string VOID = "VOID";
    public const string WATCH = "WATCH";

    
        
    
        
    
}