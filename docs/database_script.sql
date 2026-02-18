USE [master]
GO
/****** Object:  Database [DomuWave]    Script Date: 18/02/2026 14:10:40 ******/
CREATE DATABASE [DomuWave]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'DomuWave', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\DomuWave.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'DomuWave_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\DomuWave_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [DomuWave] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [DomuWave].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [DomuWave] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DomuWave] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DomuWave] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DomuWave] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DomuWave] SET ARITHABORT OFF 
GO
ALTER DATABASE [DomuWave] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [DomuWave] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DomuWave] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DomuWave] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DomuWave] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DomuWave] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DomuWave] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DomuWave] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DomuWave] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DomuWave] SET  ENABLE_BROKER 
GO
ALTER DATABASE [DomuWave] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DomuWave] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DomuWave] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DomuWave] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DomuWave] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DomuWave] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [DomuWave] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DomuWave] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [DomuWave] SET  MULTI_USER 
GO
ALTER DATABASE [DomuWave] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DomuWave] SET DB_CHAINING OFF 
GO
ALTER DATABASE [DomuWave] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [DomuWave] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [DomuWave] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [DomuWave] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [DomuWave] SET QUERY_STORE = ON
GO
ALTER DATABASE [DomuWave] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [DomuWave]
GO
/****** Object:  User [DomuWave]    Script Date: 18/02/2026 14:10:40 ******/
CREATE USER [DomuWave] FOR LOGIN [DomuWave] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [cashflow]    Script Date: 18/02/2026 14:10:40 ******/
CREATE USER [cashflow] FOR LOGIN [cashflow] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [DomuWave]
GO
/****** Object:  Schema [hangfire_EAGLEFLY_DomuWave]    Script Date: 18/02/2026 14:10:41 ******/
CREATE SCHEMA [hangfire_EAGLEFLY_DomuWave]
GO
/****** Object:  Table [dbo].[Budget]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Budget](
	[BudgetId] [int] NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CondominiumId] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[ApprovalDate] [datetime2](3) NULL,
	[Status] [nvarchar](50) NOT NULL,
	[TotalIncome] [decimal](15, 2) NOT NULL,
	[TotalExpenses] [decimal](15, 2) NOT NULL,
	[Notes] [nvarchar](1000) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Budget] PRIMARY KEY CLUSTERED 
(
	[BudgetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BudgetItem]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BudgetItem](
	[BudgetItemId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[BudgetId] [int] NOT NULL,
	[AccountId] [int] NOT NULL,
	[Amount] [decimal](15, 2) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_BudgetItem] PRIMARY KEY CLUSTERED 
(
	[BudgetItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ChartOfAccounts]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChartOfAccounts](
	[AccountId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CondominiumId] [int] NOT NULL,
	[Code] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Category] [nvarchar](100) NULL,
	[ParentAccountId] [int] NULL,
	[Level] [int] NOT NULL,
	[Description] [nvarchar](500) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_ChartOfAccounts] PRIMARY KEY CLUSTERED 
(
	[AccountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Communication]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Communication](
	[CommunicationId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CondominiumId] [int] NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[CommunicationType] [nvarchar](50) NOT NULL,
	[Priority] [nvarchar](50) NOT NULL,
	[PublicationDate] [datetime2](3) NOT NULL,
	[ExpirationDate] [datetime2](3) NULL,
	[SendEmail] [bit] NOT NULL,
	[EmailSentAt] [datetime2](3) NULL,
	[IsVisible] [bit] NOT NULL,
	[AttachmentPath] [nvarchar](500) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Communication] PRIMARY KEY CLUSTERED 
(
	[CommunicationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CommunicationRead]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommunicationRead](
	[ReadId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CommunicationId] [int] NOT NULL,
	[UserId] [bigint] NOT NULL,
	[ReadDate] [datetime2](3) NOT NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_CommunicationRead] PRIMARY KEY CLUSTERED 
(
	[ReadId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Condominium]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Condominium](
	[CondominiumId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Code] [nvarchar](50) NULL,
	[TaxCode] [nvarchar](16) NULL,
	[VatNumber] [nvarchar](11) NULL,
	[Email] [nvarchar](255) NULL,
	[Phone] [nvarchar](20) NULL,
	[Pec] [nvarchar](255) NULL,
	[NumberOfUnits] [int] NOT NULL,
	[NumberOfStaircases] [int] NOT NULL,
	[NumberOfFloors] [int] NULL,
	[YearOfConstruction] [int] NULL,
	[TotalMillesimal] [decimal](10, 4) NOT NULL,
	[HasElevator] [bit] NOT NULL,
	[NumberOfElevators] [int] NULL,
	[HasCentralHeating] [bit] NOT NULL,
	[HasConcierge] [bit] NOT NULL,
	[CommonAreasSqm] [decimal](10, 2) NULL,
	[MandateStartDate] [datetime2](3) NULL,
	[MandateEndDate] [datetime2](3) NULL,
	[LastAssemblyDate] [datetime2](3) NULL,
	[InstallmentFrequency] [nvarchar](20) NOT NULL,
	[InstallmentDueDay] [int] NOT NULL,
	[Notes] [nvarchar](2000) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Condominium] PRIMARY KEY CLUSTERED 
(
	[CondominiumId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CondominiumAddress]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CondominiumAddress](
	[AddressId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CondominiumId] [int] NOT NULL,
	[Street] [nvarchar](200) NOT NULL,
	[StreetNumber] [nvarchar](20) NOT NULL,
	[PostalCode] [nvarchar](10) NOT NULL,
	[City] [nvarchar](100) NOT NULL,
	[Province] [nvarchar](2) NOT NULL,
	[Country] [nvarchar](2) NOT NULL,
	[Latitude] [decimal](10, 8) NULL,
	[Longitude] [decimal](11, 8) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_CondominiumAddress] PRIMARY KEY CLUSTERED 
(
	[AddressId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CondominiumCadastralData]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CondominiumCadastralData](
	[CadastralDataId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CondominiumId] [int] NOT NULL,
	[Sheet] [nvarchar](10) NULL,
	[Parcel] [nvarchar](20) NULL,
	[Subordinate] [nvarchar](10) NULL,
	[UrbanSection] [nvarchar](10) NULL,
	[CadastralMunicipality] [nvarchar](100) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_CondominiumCadastralData] PRIMARY KEY CLUSTERED 
(
	[CadastralDataId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CondominiumFee]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CondominiumFee](
	[FeeId] [bigint] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[InstallmentId] [int] NOT NULL,
	[UnitId] [int] NOT NULL,
	[UserId] [bigint] NOT NULL,
	[AmountDue] [decimal](15, 2) NOT NULL,
	[AmountPaid] [decimal](15, 2) NOT NULL,
	[Balance] [decimal](15, 2) NOT NULL,
	[PaymentStatus] [nvarchar](50) NOT NULL,
	[PaymentDate] [datetime2](3) NULL,
	[PaymentMethod] [nvarchar](50) NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_CondominiumFee] PRIMARY KEY CLUSTERED 
(
	[FeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CondominiumInstallment]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CondominiumInstallment](
	[InstallmentId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CondominiumId] [int] NOT NULL,
	[BudgetId] [int] NULL,
	[Year] [int] NOT NULL,
	[InstallmentNumber] [int] NOT NULL,
	[Description] [nvarchar](200) NOT NULL,
	[DueDate] [datetime2](3) NOT NULL,
	[TotalAmount] [decimal](15, 2) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_CondominiumInstallment] PRIMARY KEY CLUSTERED 
(
	[InstallmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Document]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Document](
	[DocumentId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CondominiumId] [int] NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](1000) NULL,
	[Category] [nvarchar](100) NOT NULL,
	[FileName] [nvarchar](255) NOT NULL,
	[FilePath] [nvarchar](500) NOT NULL,
	[FileSize] [bigint] NOT NULL,
	[MimeType] [nvarchar](100) NOT NULL,
	[DocumentDate] [datetime2](3) NULL,
	[IsVisibleToOwners] [bit] NOT NULL,
	[IsArchived] [bit] NOT NULL,
	[Tags] [nvarchar](500) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Document] PRIMARY KEY CLUSTERED 
(
	[DocumentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DocumentAccess]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocumentAccess](
	[AccessId] [bigint] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[DocumentId] [int] NOT NULL,
	[UserId] [bigint] NOT NULL,
	[AccessDate] [datetime2](3) NOT NULL,
	[AccessType] [nvarchar](50) NOT NULL,
	[IpAddress] [nvarchar](50) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_DocumentAccess] PRIMARY KEY CLUSTERED 
(
	[AccessId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Expense]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Expense](
	[ExpenseId] [bigint] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CondominiumId] [int] NOT NULL,
	[AccountId] [int] NOT NULL,
	[SupplierId] [int] NULL,
	[DocumentNumber] [nvarchar](50) NULL,
	[DocumentDate] [datetime2](3) NOT NULL,
	[RegistrationDate] [datetime2](3) NOT NULL,
	[Description] [nvarchar](500) NOT NULL,
	[GrossAmount] [decimal](15, 2) NOT NULL,
	[VatAmount] [decimal](15, 2) NOT NULL,
	[NetAmount] [decimal](15, 2) NOT NULL,
	[ExpenseType] [nvarchar](50) NOT NULL,
	[MillesimalTableId] [int] NOT NULL,
	[PaymentStatus] [nvarchar](50) NOT NULL,
	[PaymentDate] [datetime2](3) NULL,
	[PaymentMethod] [nvarchar](50) NULL,
	[DocumentPath] [nvarchar](500) NULL,
	[Notes] [nvarchar](1000) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Expense] PRIMARY KEY CLUSTERED 
(
	[ExpenseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExpenseAllocation]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExpenseAllocation](
	[ExpenseAllocationId] [bigint] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[ExpenseId] [bigint] NOT NULL,
	[UnitId] [int] NOT NULL,
	[Millesimal] [decimal](10, 4) NOT NULL,
	[AllocatedAmount] [decimal](15, 2) NOT NULL,
	[AllocationPercentage] [decimal](5, 2) NOT NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_ExpenseAllocation] PRIMARY KEY CLUSTERED 
(
	[ExpenseAllocationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Message]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Message](
	[MessageId] [bigint] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CondominiumId] [int] NOT NULL,
	[SenderId] [bigint] NOT NULL,
	[RecipientId] [bigint] NOT NULL,
	[Subject] [nvarchar](200) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[ParentMessageId] [bigint] NULL,
	[IsRead] [bit] NOT NULL,
	[ReadDate] [datetime2](3) NULL,
	[AttachmentPath] [nvarchar](500) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Message] PRIMARY KEY CLUSTERED 
(
	[MessageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MillesimalTable]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MillesimalTable](
	[MillesimalTableId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CondominiumId] [int] NOT NULL,
	[Code] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[TotalMillesimal] [decimal](10, 4) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_MillesimalTable] PRIMARY KEY CLUSTERED 
(
	[MillesimalTableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RealEstateUnit]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RealEstateUnit](
	[UnitId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CondominiumId] [int] NOT NULL,
	[Staircase] [nvarchar](10) NULL,
	[Floor] [int] NOT NULL,
	[InternalNumber] [nvarchar](20) NOT NULL,
	[Subordinate] [nvarchar](10) NULL,
	[Category] [nvarchar](10) NULL,
	[CadastralIncome] [decimal](10, 2) NULL,
	[AreaSqm] [decimal](10, 2) NULL,
	[Rooms] [decimal](4, 1) NULL,
	[UnitType] [nvarchar](50) NOT NULL,
	[OccupancyStatus] [nvarchar](50) NOT NULL,
	[Notes] [nvarchar](1000) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_RealEstateUnit] PRIMARY KEY CLUSTERED 
(
	[UnitId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Receipt]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Receipt](
	[ReceiptId] [bigint] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CondominiumId] [int] NOT NULL,
	[FeeId] [bigint] NULL,
	[UserId] [bigint] NOT NULL,
	[UnitId] [int] NOT NULL,
	[ReceiptDate] [datetime2](3) NOT NULL,
	[RegistrationDate] [datetime2](3) NOT NULL,
	[Amount] [decimal](15, 2) NOT NULL,
	[PaymentMethod] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](500) NOT NULL,
	[BankReference] [nvarchar](100) NULL,
	[ReconciliationStatus] [nvarchar](50) NOT NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Receipt] PRIMARY KEY CLUSTERED 
(
	[ReceiptId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Supplier]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Supplier](
	[SupplierId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CompanyName] [nvarchar](200) NOT NULL,
	[VatNumber] [nvarchar](11) NULL,
	[TaxCode] [nvarchar](16) NULL,
	[Address] [nvarchar](300) NULL,
	[City] [nvarchar](100) NULL,
	[Province] [nvarchar](2) NULL,
	[PostalCode] [nvarchar](10) NULL,
	[Email] [nvarchar](255) NULL,
	[Phone] [nvarchar](20) NULL,
	[Pec] [nvarchar](255) NULL,
	[ContactPerson] [nvarchar](200) NULL,
	[SupplierType] [nvarchar](100) NULL,
	[PaymentTerms] [nvarchar](100) NULL,
	[IbanAccount] [nvarchar](34) NULL,
	[Notes] [nvarchar](1000) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Supplier] PRIMARY KEY CLUSTERED 
(
	[SupplierId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SupplierContract]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SupplierContract](
	[ContractId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CondominiumId] [int] NOT NULL,
	[SupplierId] [int] NOT NULL,
	[ContractNumber] [nvarchar](50) NULL,
	[Subject] [nvarchar](200) NOT NULL,
	[StartDate] [datetime2](3) NOT NULL,
	[EndDate] [datetime2](3) NULL,
	[AnnualAmount] [decimal](15, 2) NULL,
	[Frequency] [nvarchar](50) NULL,
	[AutoRenewal] [bit] NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[DocumentPath] [nvarchar](500) NULL,
	[Notes] [nvarchar](1000) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_SupplierContract] PRIMARY KEY CLUSTERED 
(
	[ContractId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tenant]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tenant](
	[TenantId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Code] [nvarchar](50) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Tenant] PRIMARY KEY CLUSTERED 
(
	[TenantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UnitMillesimal]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UnitMillesimal](
	[UnitMillesimalId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[MillesimalTableId] [int] NOT NULL,
	[UnitId] [int] NOT NULL,
	[Millesimal] [decimal](10, 4) NOT NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_UnitMillesimal] PRIMARY KEY CLUSTERED 
(
	[UnitMillesimalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UnitOwner]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UnitOwner](
	[UnitOwnerId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[UnitId] [int] NOT NULL,
	[UserId] [bigint] NOT NULL,
	[OwnerType] [nvarchar](50) NOT NULL,
	[OwnershipQuota] [decimal](5, 2) NOT NULL,
	[StartDate] [datetime2](3) NOT NULL,
	[EndDate] [datetime2](3) NULL,
	[IsResident] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_UnitOwner] PRIMARY KEY CLUSTERED 
(
	[UnitOwnerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UnitTenant]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UnitTenant](
	[TenantId] [int] IDENTITY(1,1) NOT NULL,
	[OwnerTenantId] [uniqueidentifier] NOT NULL,
	[UnitId] [int] NOT NULL,
	[FirstName] [nvarchar](100) NOT NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[TaxCode] [nvarchar](16) NULL,
	[Email] [nvarchar](255) NULL,
	[Phone] [nvarchar](20) NULL,
	[LeaseStartDate] [datetime2](3) NOT NULL,
	[LeaseEndDate] [datetime2](3) NULL,
	[ContractPath] [nvarchar](500) NULL,
	[ExpensePayer] [nvarchar](50) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedById] [bigint] NOT NULL,
	[CreatedByFullName] [nvarchar](200) NULL,
	[CreationDate] [datetime2](3) NOT NULL,
	[LastUpdatedById] [bigint] NULL,
	[LastUpdatedByFullName] [nvarchar](200) NULL,
	[LastUpdateDate] [datetime2](3) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_UnitTenant] PRIMARY KEY CLUSTERED 
(
	[TenantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [hangfire_EAGLEFLY_DomuWave].[AggregatedCounter]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [hangfire_EAGLEFLY_DomuWave].[AggregatedCounter](
	[Key] [nvarchar](100) NOT NULL,
	[Value] [bigint] NOT NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_CounterAggregated] PRIMARY KEY CLUSTERED 
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [hangfire_EAGLEFLY_DomuWave].[Counter]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [hangfire_EAGLEFLY_DomuWave].[Counter](
	[Key] [nvarchar](100) NOT NULL,
	[Value] [int] NOT NULL,
	[ExpireAt] [datetime] NULL,
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_HangFire_Counter] PRIMARY KEY CLUSTERED 
(
	[Key] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [hangfire_EAGLEFLY_DomuWave].[Hash]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [hangfire_EAGLEFLY_DomuWave].[Hash](
	[Key] [nvarchar](100) NOT NULL,
	[Field] [nvarchar](100) NOT NULL,
	[Value] [nvarchar](max) NULL,
	[ExpireAt] [datetime2](7) NULL,
 CONSTRAINT [PK_HangFire_Hash] PRIMARY KEY CLUSTERED 
(
	[Key] ASC,
	[Field] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = ON, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [hangfire_EAGLEFLY_DomuWave].[Job]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [hangfire_EAGLEFLY_DomuWave].[Job](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[StateId] [bigint] NULL,
	[StateName] [nvarchar](20) NULL,
	[InvocationData] [nvarchar](max) NOT NULL,
	[Arguments] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_Job] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [hangfire_EAGLEFLY_DomuWave].[JobParameter]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [hangfire_EAGLEFLY_DomuWave].[JobParameter](
	[JobId] [bigint] NOT NULL,
	[Name] [nvarchar](40) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_HangFire_JobParameter] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [hangfire_EAGLEFLY_DomuWave].[JobQueue]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [hangfire_EAGLEFLY_DomuWave].[JobQueue](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[JobId] [bigint] NOT NULL,
	[Queue] [nvarchar](50) NOT NULL,
	[FetchedAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_JobQueue] PRIMARY KEY CLUSTERED 
(
	[Queue] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [hangfire_EAGLEFLY_DomuWave].[List]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [hangfire_EAGLEFLY_DomuWave].[List](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](100) NOT NULL,
	[Value] [nvarchar](max) NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_List] PRIMARY KEY CLUSTERED 
(
	[Key] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [hangfire_EAGLEFLY_DomuWave].[Schema]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [hangfire_EAGLEFLY_DomuWave].[Schema](
	[Version] [int] NOT NULL,
 CONSTRAINT [PK_HangFire_Schema] PRIMARY KEY CLUSTERED 
(
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [hangfire_EAGLEFLY_DomuWave].[Server]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [hangfire_EAGLEFLY_DomuWave].[Server](
	[Id] [nvarchar](200) NOT NULL,
	[Data] [nvarchar](max) NULL,
	[LastHeartbeat] [datetime] NOT NULL,
 CONSTRAINT [PK_HangFire_Server] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [hangfire_EAGLEFLY_DomuWave].[Set]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [hangfire_EAGLEFLY_DomuWave].[Set](
	[Key] [nvarchar](100) NOT NULL,
	[Score] [float] NOT NULL,
	[Value] [nvarchar](256) NOT NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_Set] PRIMARY KEY CLUSTERED 
(
	[Key] ASC,
	[Value] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = ON, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [hangfire_EAGLEFLY_DomuWave].[State]    Script Date: 18/02/2026 14:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [hangfire_EAGLEFLY_DomuWave].[State](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[JobId] [bigint] NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
	[Reason] [nvarchar](100) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[Data] [nvarchar](max) NULL,
 CONSTRAINT [PK_HangFire_State] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Index [IX_Budget_CondominiumId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Budget_CondominiumId] ON [dbo].[Budget]
(
	[CondominiumId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Budget_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Budget_TenantId] ON [dbo].[Budget]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Budget_Year]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Budget_Year] ON [dbo].[Budget]
(
	[Year] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Budget_Year_Type]    Script Date: 18/02/2026 14:10:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Budget_Year_Type] ON [dbo].[Budget]
(
	[CondominiumId] ASC,
	[Year] ASC,
	[Type] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_BudgetItem_AccountId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_BudgetItem_AccountId] ON [dbo].[BudgetItem]
(
	[AccountId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_BudgetItem_BudgetId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_BudgetItem_BudgetId] ON [dbo].[BudgetItem]
(
	[BudgetId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_BudgetItem_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_BudgetItem_TenantId] ON [dbo].[BudgetItem]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ChartOfAccounts_Code]    Script Date: 18/02/2026 14:10:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_ChartOfAccounts_Code] ON [dbo].[ChartOfAccounts]
(
	[CondominiumId] ASC,
	[Code] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ChartOfAccounts_CondominiumId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_ChartOfAccounts_CondominiumId] ON [dbo].[ChartOfAccounts]
(
	[CondominiumId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ChartOfAccounts_Parent]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_ChartOfAccounts_Parent] ON [dbo].[ChartOfAccounts]
(
	[ParentAccountId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ChartOfAccounts_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_ChartOfAccounts_TenantId] ON [dbo].[ChartOfAccounts]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Communication_CondominiumId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Communication_CondominiumId] ON [dbo].[Communication]
(
	[CondominiumId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Communication_PublicationDate]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Communication_PublicationDate] ON [dbo].[Communication]
(
	[PublicationDate] DESC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Communication_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Communication_TenantId] ON [dbo].[Communication]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Communication_Type]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Communication_Type] ON [dbo].[Communication]
(
	[CommunicationType] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CommunicationRead_CommunicationId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CommunicationRead_CommunicationId] ON [dbo].[CommunicationRead]
(
	[CommunicationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CommunicationRead_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CommunicationRead_TenantId] ON [dbo].[CommunicationRead]
(
	[TenantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CommunicationRead_Unique]    Script Date: 18/02/2026 14:10:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_CommunicationRead_Unique] ON [dbo].[CommunicationRead]
(
	[CommunicationId] ASC,
	[UserId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CommunicationRead_UserId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CommunicationRead_UserId] ON [dbo].[CommunicationRead]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Condominium_IsDeleted]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Condominium_IsDeleted] ON [dbo].[Condominium]
(
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Condominium_Tenant_Code]    Script Date: 18/02/2026 14:10:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Condominium_Tenant_Code] ON [dbo].[Condominium]
(
	[TenantId] ASC,
	[Code] ASC
)
WHERE ([IsDeleted]=(0) AND [Code] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Condominium_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Condominium_TenantId] ON [dbo].[Condominium]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_CondominiumAddress_City]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CondominiumAddress_City] ON [dbo].[CondominiumAddress]
(
	[City] ASC,
	[Province] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumAddress_CondominiumId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_CondominiumAddress_CondominiumId] ON [dbo].[CondominiumAddress]
(
	[CondominiumId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumAddress_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CondominiumAddress_TenantId] ON [dbo].[CondominiumAddress]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumCadastralData_CondominiumId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_CondominiumCadastralData_CondominiumId] ON [dbo].[CondominiumCadastralData]
(
	[CondominiumId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumCadastralData_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CondominiumCadastralData_TenantId] ON [dbo].[CondominiumCadastralData]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumFee_InstallmentId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CondominiumFee_InstallmentId] ON [dbo].[CondominiumFee]
(
	[InstallmentId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_CondominiumFee_Overdue]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CondominiumFee_Overdue] ON [dbo].[CondominiumFee]
(
	[PaymentStatus] ASC,
	[Balance] ASC
)
WHERE ([IsDeleted]=(0) AND ([PaymentStatus] IN ('ToPay', 'Partial', 'Overdue')))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumFee_OwnerStatement]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CondominiumFee_OwnerStatement] ON [dbo].[CondominiumFee]
(
	[UserId] ASC,
	[InstallmentId] ASC
)
INCLUDE([AmountDue],[AmountPaid],[Balance],[PaymentStatus]) 
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_CondominiumFee_PaymentStatus]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CondominiumFee_PaymentStatus] ON [dbo].[CondominiumFee]
(
	[PaymentStatus] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumFee_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CondominiumFee_TenantId] ON [dbo].[CondominiumFee]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumFee_Unique]    Script Date: 18/02/2026 14:10:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_CondominiumFee_Unique] ON [dbo].[CondominiumFee]
(
	[InstallmentId] ASC,
	[UnitId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumFee_UnitId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CondominiumFee_UnitId] ON [dbo].[CondominiumFee]
(
	[UnitId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumFee_UserId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CondominiumFee_UserId] ON [dbo].[CondominiumFee]
(
	[UserId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumInstallment_CondominiumId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CondominiumInstallment_CondominiumId] ON [dbo].[CondominiumInstallment]
(
	[CondominiumId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumInstallment_DueDate]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CondominiumInstallment_DueDate] ON [dbo].[CondominiumInstallment]
(
	[DueDate] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumInstallment_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CondominiumInstallment_TenantId] ON [dbo].[CondominiumInstallment]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumInstallment_Unique]    Script Date: 18/02/2026 14:10:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_CondominiumInstallment_Unique] ON [dbo].[CondominiumInstallment]
(
	[CondominiumId] ASC,
	[Year] ASC,
	[InstallmentNumber] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CondominiumInstallment_Year]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_CondominiumInstallment_Year] ON [dbo].[CondominiumInstallment]
(
	[Year] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Document_Category]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Document_Category] ON [dbo].[Document]
(
	[Category] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Document_CondominiumId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Document_CondominiumId] ON [dbo].[Document]
(
	[CondominiumId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Document_CreationDate]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Document_CreationDate] ON [dbo].[Document]
(
	[CreationDate] DESC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Document_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Document_TenantId] ON [dbo].[Document]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Document_VisibleToOwners]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Document_VisibleToOwners] ON [dbo].[Document]
(
	[CondominiumId] ASC,
	[IsVisibleToOwners] ASC,
	[CreationDate] DESC
)
WHERE ([IsDeleted]=(0) AND [IsArchived]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DocumentAccess_AccessDate]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_DocumentAccess_AccessDate] ON [dbo].[DocumentAccess]
(
	[AccessDate] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DocumentAccess_DocumentId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_DocumentAccess_DocumentId] ON [dbo].[DocumentAccess]
(
	[DocumentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DocumentAccess_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_DocumentAccess_TenantId] ON [dbo].[DocumentAccess]
(
	[TenantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DocumentAccess_UserId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_DocumentAccess_UserId] ON [dbo].[DocumentAccess]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Expense_AccountId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Expense_AccountId] ON [dbo].[Expense]
(
	[AccountId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Expense_Condominium_Year]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Expense_Condominium_Year] ON [dbo].[Expense]
(
	[CondominiumId] ASC,
	[DocumentDate] ASC
)
INCLUDE([NetAmount],[ExpenseType]) 
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Expense_CondominiumId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Expense_CondominiumId] ON [dbo].[Expense]
(
	[CondominiumId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Expense_DocumentDate]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Expense_DocumentDate] ON [dbo].[Expense]
(
	[DocumentDate] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Expense_PaymentStatus]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Expense_PaymentStatus] ON [dbo].[Expense]
(
	[PaymentStatus] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Expense_SupplierId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Expense_SupplierId] ON [dbo].[Expense]
(
	[SupplierId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Expense_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Expense_TenantId] ON [dbo].[Expense]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ExpenseAllocation_ExpenseId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_ExpenseAllocation_ExpenseId] ON [dbo].[ExpenseAllocation]
(
	[ExpenseId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ExpenseAllocation_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_ExpenseAllocation_TenantId] ON [dbo].[ExpenseAllocation]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ExpenseAllocation_Unique]    Script Date: 18/02/2026 14:10:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_ExpenseAllocation_Unique] ON [dbo].[ExpenseAllocation]
(
	[ExpenseId] ASC,
	[UnitId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ExpenseAllocation_UnitId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_ExpenseAllocation_UnitId] ON [dbo].[ExpenseAllocation]
(
	[UnitId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Message_CondominiumId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Message_CondominiumId] ON [dbo].[Message]
(
	[CondominiumId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Message_CreationDate]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Message_CreationDate] ON [dbo].[Message]
(
	[CreationDate] DESC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Message_Parent]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Message_Parent] ON [dbo].[Message]
(
	[ParentMessageId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Message_RecipientId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Message_RecipientId] ON [dbo].[Message]
(
	[RecipientId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Message_SenderId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Message_SenderId] ON [dbo].[Message]
(
	[SenderId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Message_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Message_TenantId] ON [dbo].[Message]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Message_Unread]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Message_Unread] ON [dbo].[Message]
(
	[RecipientId] ASC,
	[IsRead] ASC
)
WHERE ([IsDeleted]=(0) AND [IsRead]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_MillesimalTable_Code]    Script Date: 18/02/2026 14:10:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_MillesimalTable_Code] ON [dbo].[MillesimalTable]
(
	[CondominiumId] ASC,
	[Code] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MillesimalTable_CondominiumId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_MillesimalTable_CondominiumId] ON [dbo].[MillesimalTable]
(
	[CondominiumId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MillesimalTable_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_MillesimalTable_TenantId] ON [dbo].[MillesimalTable]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RealEstateUnit_CondominiumId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_RealEstateUnit_CondominiumId] ON [dbo].[RealEstateUnit]
(
	[CondominiumId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RealEstateUnit_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_RealEstateUnit_TenantId] ON [dbo].[RealEstateUnit]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_RealEstateUnit_Unique]    Script Date: 18/02/2026 14:10:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_RealEstateUnit_Unique] ON [dbo].[RealEstateUnit]
(
	[CondominiumId] ASC,
	[Staircase] ASC,
	[Floor] ASC,
	[InternalNumber] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_RealEstateUnit_UnitType]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_RealEstateUnit_UnitType] ON [dbo].[RealEstateUnit]
(
	[UnitType] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Receipt_CondominiumId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Receipt_CondominiumId] ON [dbo].[Receipt]
(
	[CondominiumId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Receipt_FeeId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Receipt_FeeId] ON [dbo].[Receipt]
(
	[FeeId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Receipt_ReceiptDate]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Receipt_ReceiptDate] ON [dbo].[Receipt]
(
	[ReceiptDate] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Receipt_ReconciliationStatus]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Receipt_ReconciliationStatus] ON [dbo].[Receipt]
(
	[ReconciliationStatus] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Receipt_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Receipt_TenantId] ON [dbo].[Receipt]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Receipt_UnitId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Receipt_UnitId] ON [dbo].[Receipt]
(
	[UnitId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Receipt_UserId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Receipt_UserId] ON [dbo].[Receipt]
(
	[UserId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Supplier_CompanyName]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Supplier_CompanyName] ON [dbo].[Supplier]
(
	[CompanyName] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Supplier_SupplierType]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Supplier_SupplierType] ON [dbo].[Supplier]
(
	[SupplierType] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Supplier_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Supplier_TenantId] ON [dbo].[Supplier]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_SupplierContract_CondominiumId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_SupplierContract_CondominiumId] ON [dbo].[SupplierContract]
(
	[CondominiumId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_SupplierContract_EndDate]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_SupplierContract_EndDate] ON [dbo].[SupplierContract]
(
	[EndDate] ASC
)
WHERE ([IsDeleted]=(0) AND [Status]='Active')
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_SupplierContract_SupplierId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_SupplierContract_SupplierId] ON [dbo].[SupplierContract]
(
	[SupplierId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_SupplierContract_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_SupplierContract_TenantId] ON [dbo].[SupplierContract]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Tenant_Code]    Script Date: 18/02/2026 14:10:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Tenant_Code] ON [dbo].[Tenant]
(
	[Code] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Tenant_IsDeleted]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_Tenant_IsDeleted] ON [dbo].[Tenant]
(
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UnitMillesimal_MillesimalTableId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_UnitMillesimal_MillesimalTableId] ON [dbo].[UnitMillesimal]
(
	[MillesimalTableId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UnitMillesimal_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_UnitMillesimal_TenantId] ON [dbo].[UnitMillesimal]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UnitMillesimal_Unique]    Script Date: 18/02/2026 14:10:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_UnitMillesimal_Unique] ON [dbo].[UnitMillesimal]
(
	[MillesimalTableId] ASC,
	[UnitId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UnitMillesimal_UnitId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_UnitMillesimal_UnitId] ON [dbo].[UnitMillesimal]
(
	[UnitId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UnitOwner_Active]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_UnitOwner_Active] ON [dbo].[UnitOwner]
(
	[UnitId] ASC,
	[IsActive] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UnitOwner_TenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_UnitOwner_TenantId] ON [dbo].[UnitOwner]
(
	[TenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UnitOwner_UnitId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_UnitOwner_UnitId] ON [dbo].[UnitOwner]
(
	[UnitId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UnitOwner_UserId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_UnitOwner_UserId] ON [dbo].[UnitOwner]
(
	[UserId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UnitTenant_Active]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_UnitTenant_Active] ON [dbo].[UnitTenant]
(
	[UnitId] ASC,
	[IsActive] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UnitTenant_OwnerTenantId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_UnitTenant_OwnerTenantId] ON [dbo].[UnitTenant]
(
	[OwnerTenantId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UnitTenant_UnitId]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_UnitTenant_UnitId] ON [dbo].[UnitTenant]
(
	[UnitId] ASC
)
WHERE ([IsDeleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HangFire_AggregatedCounter_ExpireAt]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_AggregatedCounter_ExpireAt] ON [hangfire_EAGLEFLY_DomuWave].[AggregatedCounter]
(
	[ExpireAt] ASC
)
WHERE ([ExpireAt] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HangFire_Hash_ExpireAt]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_Hash_ExpireAt] ON [hangfire_EAGLEFLY_DomuWave].[Hash]
(
	[ExpireAt] ASC
)
WHERE ([ExpireAt] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HangFire_Job_ExpireAt]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_Job_ExpireAt] ON [hangfire_EAGLEFLY_DomuWave].[Job]
(
	[ExpireAt] ASC
)
INCLUDE([StateName]) 
WHERE ([ExpireAt] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_HangFire_Job_StateName]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_Job_StateName] ON [hangfire_EAGLEFLY_DomuWave].[Job]
(
	[StateName] ASC
)
WHERE ([StateName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HangFire_List_ExpireAt]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_List_ExpireAt] ON [hangfire_EAGLEFLY_DomuWave].[List]
(
	[ExpireAt] ASC
)
WHERE ([ExpireAt] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HangFire_Server_LastHeartbeat]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_Server_LastHeartbeat] ON [hangfire_EAGLEFLY_DomuWave].[Server]
(
	[LastHeartbeat] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HangFire_Set_ExpireAt]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_Set_ExpireAt] ON [hangfire_EAGLEFLY_DomuWave].[Set]
(
	[ExpireAt] ASC
)
WHERE ([ExpireAt] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_HangFire_Set_Score]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_Set_Score] ON [hangfire_EAGLEFLY_DomuWave].[Set]
(
	[Key] ASC,
	[Score] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HangFire_State_CreatedAt]    Script Date: 18/02/2026 14:10:41 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_State_CreatedAt] ON [hangfire_EAGLEFLY_DomuWave].[State]
(
	[CreatedAt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Budget] ADD  CONSTRAINT [DF__Budget__Status__3C89F72A]  DEFAULT ('Draft') FOR [Status]
GO
ALTER TABLE [dbo].[Budget] ADD  CONSTRAINT [DF__Budget__TotalInc__3D7E1B63]  DEFAULT ((0)) FOR [TotalIncome]
GO
ALTER TABLE [dbo].[Budget] ADD  CONSTRAINT [DF__Budget__TotalExp__3E723F9C]  DEFAULT ((0)) FOR [TotalExpenses]
GO
ALTER TABLE [dbo].[Budget] ADD  CONSTRAINT [DF__Budget__Creation__3F6663D5]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Budget] ADD  CONSTRAINT [DF__Budget__IsDelete__405A880E]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[BudgetItem] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[BudgetItem] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[ChartOfAccounts] ADD  DEFAULT ((1)) FOR [Level]
GO
ALTER TABLE [dbo].[ChartOfAccounts] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ChartOfAccounts] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[ChartOfAccounts] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Communication] ADD  DEFAULT ('Normal') FOR [Priority]
GO
ALTER TABLE [dbo].[Communication] ADD  DEFAULT (getdate()) FOR [PublicationDate]
GO
ALTER TABLE [dbo].[Communication] ADD  DEFAULT ((1)) FOR [SendEmail]
GO
ALTER TABLE [dbo].[Communication] ADD  DEFAULT ((1)) FOR [IsVisible]
GO
ALTER TABLE [dbo].[Communication] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Communication] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[CommunicationRead] ADD  DEFAULT (getdate()) FOR [ReadDate]
GO
ALTER TABLE [dbo].[CommunicationRead] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[CommunicationRead] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Condominium] ADD  DEFAULT ((0)) FOR [NumberOfUnits]
GO
ALTER TABLE [dbo].[Condominium] ADD  DEFAULT ((1)) FOR [NumberOfStaircases]
GO
ALTER TABLE [dbo].[Condominium] ADD  DEFAULT ((1000.0000)) FOR [TotalMillesimal]
GO
ALTER TABLE [dbo].[Condominium] ADD  DEFAULT ((0)) FOR [HasElevator]
GO
ALTER TABLE [dbo].[Condominium] ADD  DEFAULT ((0)) FOR [NumberOfElevators]
GO
ALTER TABLE [dbo].[Condominium] ADD  DEFAULT ((0)) FOR [HasCentralHeating]
GO
ALTER TABLE [dbo].[Condominium] ADD  DEFAULT ((0)) FOR [HasConcierge]
GO
ALTER TABLE [dbo].[Condominium] ADD  DEFAULT ('Monthly') FOR [InstallmentFrequency]
GO
ALTER TABLE [dbo].[Condominium] ADD  DEFAULT ((10)) FOR [InstallmentDueDay]
GO
ALTER TABLE [dbo].[Condominium] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Condominium] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Condominium] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[CondominiumAddress] ADD  DEFAULT ('IT') FOR [Country]
GO
ALTER TABLE [dbo].[CondominiumAddress] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[CondominiumAddress] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[CondominiumCadastralData] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[CondominiumCadastralData] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[CondominiumFee] ADD  DEFAULT ((0)) FOR [AmountPaid]
GO
ALTER TABLE [dbo].[CondominiumFee] ADD  DEFAULT ('ToPay') FOR [PaymentStatus]
GO
ALTER TABLE [dbo].[CondominiumFee] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[CondominiumFee] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[CondominiumInstallment] ADD  DEFAULT ('Open') FOR [Status]
GO
ALTER TABLE [dbo].[CondominiumInstallment] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[CondominiumInstallment] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Document] ADD  DEFAULT ((0)) FOR [IsVisibleToOwners]
GO
ALTER TABLE [dbo].[Document] ADD  DEFAULT ((0)) FOR [IsArchived]
GO
ALTER TABLE [dbo].[Document] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Document] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[DocumentAccess] ADD  DEFAULT (getdate()) FOR [AccessDate]
GO
ALTER TABLE [dbo].[DocumentAccess] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[DocumentAccess] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Expense] ADD  DEFAULT (getdate()) FOR [RegistrationDate]
GO
ALTER TABLE [dbo].[Expense] ADD  DEFAULT ((0)) FOR [VatAmount]
GO
ALTER TABLE [dbo].[Expense] ADD  DEFAULT ('ToPay') FOR [PaymentStatus]
GO
ALTER TABLE [dbo].[Expense] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Expense] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[ExpenseAllocation] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[ExpenseAllocation] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Message] ADD  DEFAULT ((0)) FOR [IsRead]
GO
ALTER TABLE [dbo].[Message] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Message] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[MillesimalTable] ADD  DEFAULT ((1000.0000)) FOR [TotalMillesimal]
GO
ALTER TABLE [dbo].[MillesimalTable] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[MillesimalTable] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[MillesimalTable] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[RealEstateUnit] ADD  DEFAULT ('Residential') FOR [UnitType]
GO
ALTER TABLE [dbo].[RealEstateUnit] ADD  DEFAULT ('Occupied') FOR [OccupancyStatus]
GO
ALTER TABLE [dbo].[RealEstateUnit] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[RealEstateUnit] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[RealEstateUnit] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Receipt] ADD  DEFAULT (getdate()) FOR [RegistrationDate]
GO
ALTER TABLE [dbo].[Receipt] ADD  DEFAULT ('ToReconcile') FOR [ReconciliationStatus]
GO
ALTER TABLE [dbo].[Receipt] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Receipt] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Supplier] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Supplier] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Supplier] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[SupplierContract] ADD  DEFAULT ((0)) FOR [AutoRenewal]
GO
ALTER TABLE [dbo].[SupplierContract] ADD  DEFAULT ('Active') FOR [Status]
GO
ALTER TABLE [dbo].[SupplierContract] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[SupplierContract] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Tenant] ADD  DEFAULT (newid()) FOR [TenantId]
GO
ALTER TABLE [dbo].[Tenant] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Tenant] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Tenant] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[UnitMillesimal] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[UnitMillesimal] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[UnitOwner] ADD  DEFAULT ('Owner') FOR [OwnerType]
GO
ALTER TABLE [dbo].[UnitOwner] ADD  DEFAULT ((100.00)) FOR [OwnershipQuota]
GO
ALTER TABLE [dbo].[UnitOwner] ADD  DEFAULT ((1)) FOR [IsResident]
GO
ALTER TABLE [dbo].[UnitOwner] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[UnitOwner] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[UnitOwner] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[UnitTenant] ADD  DEFAULT ('Owner') FOR [ExpensePayer]
GO
ALTER TABLE [dbo].[UnitTenant] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[UnitTenant] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[UnitTenant] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Budget]  WITH CHECK ADD  CONSTRAINT [FK_Budget_Condominium] FOREIGN KEY([CondominiumId])
REFERENCES [dbo].[Condominium] ([CondominiumId])
GO
ALTER TABLE [dbo].[Budget] CHECK CONSTRAINT [FK_Budget_Condominium]
GO
ALTER TABLE [dbo].[Budget]  WITH CHECK ADD  CONSTRAINT [FK_Budget_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[Budget] CHECK CONSTRAINT [FK_Budget_Tenant]
GO
ALTER TABLE [dbo].[BudgetItem]  WITH CHECK ADD  CONSTRAINT [FK_BudgetItem_Budget] FOREIGN KEY([BudgetId])
REFERENCES [dbo].[Budget] ([BudgetId])
GO
ALTER TABLE [dbo].[BudgetItem] CHECK CONSTRAINT [FK_BudgetItem_Budget]
GO
ALTER TABLE [dbo].[BudgetItem]  WITH CHECK ADD  CONSTRAINT [FK_BudgetItem_ChartOfAccounts] FOREIGN KEY([AccountId])
REFERENCES [dbo].[ChartOfAccounts] ([AccountId])
GO
ALTER TABLE [dbo].[BudgetItem] CHECK CONSTRAINT [FK_BudgetItem_ChartOfAccounts]
GO
ALTER TABLE [dbo].[BudgetItem]  WITH CHECK ADD  CONSTRAINT [FK_BudgetItem_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[BudgetItem] CHECK CONSTRAINT [FK_BudgetItem_Tenant]
GO
ALTER TABLE [dbo].[ChartOfAccounts]  WITH CHECK ADD  CONSTRAINT [FK_ChartOfAccounts_Condominium] FOREIGN KEY([CondominiumId])
REFERENCES [dbo].[Condominium] ([CondominiumId])
GO
ALTER TABLE [dbo].[ChartOfAccounts] CHECK CONSTRAINT [FK_ChartOfAccounts_Condominium]
GO
ALTER TABLE [dbo].[ChartOfAccounts]  WITH CHECK ADD  CONSTRAINT [FK_ChartOfAccounts_Parent] FOREIGN KEY([ParentAccountId])
REFERENCES [dbo].[ChartOfAccounts] ([AccountId])
GO
ALTER TABLE [dbo].[ChartOfAccounts] CHECK CONSTRAINT [FK_ChartOfAccounts_Parent]
GO
ALTER TABLE [dbo].[ChartOfAccounts]  WITH CHECK ADD  CONSTRAINT [FK_ChartOfAccounts_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[ChartOfAccounts] CHECK CONSTRAINT [FK_ChartOfAccounts_Tenant]
GO
ALTER TABLE [dbo].[Communication]  WITH CHECK ADD  CONSTRAINT [FK_Communication_Condominium] FOREIGN KEY([CondominiumId])
REFERENCES [dbo].[Condominium] ([CondominiumId])
GO
ALTER TABLE [dbo].[Communication] CHECK CONSTRAINT [FK_Communication_Condominium]
GO
ALTER TABLE [dbo].[Communication]  WITH CHECK ADD  CONSTRAINT [FK_Communication_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[Communication] CHECK CONSTRAINT [FK_Communication_Tenant]
GO
ALTER TABLE [dbo].[CommunicationRead]  WITH CHECK ADD  CONSTRAINT [FK_CommunicationRead_Communication] FOREIGN KEY([CommunicationId])
REFERENCES [dbo].[Communication] ([CommunicationId])
GO
ALTER TABLE [dbo].[CommunicationRead] CHECK CONSTRAINT [FK_CommunicationRead_Communication]
GO
ALTER TABLE [dbo].[CommunicationRead]  WITH CHECK ADD  CONSTRAINT [FK_CommunicationRead_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[CommunicationRead] CHECK CONSTRAINT [FK_CommunicationRead_Tenant]
GO
ALTER TABLE [dbo].[Condominium]  WITH CHECK ADD  CONSTRAINT [FK_Condominium_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[Condominium] CHECK CONSTRAINT [FK_Condominium_Tenant]
GO
ALTER TABLE [dbo].[CondominiumAddress]  WITH CHECK ADD  CONSTRAINT [FK_CondominiumAddress_Condominium] FOREIGN KEY([CondominiumId])
REFERENCES [dbo].[Condominium] ([CondominiumId])
GO
ALTER TABLE [dbo].[CondominiumAddress] CHECK CONSTRAINT [FK_CondominiumAddress_Condominium]
GO
ALTER TABLE [dbo].[CondominiumAddress]  WITH CHECK ADD  CONSTRAINT [FK_CondominiumAddress_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[CondominiumAddress] CHECK CONSTRAINT [FK_CondominiumAddress_Tenant]
GO
ALTER TABLE [dbo].[CondominiumCadastralData]  WITH CHECK ADD  CONSTRAINT [FK_CondominiumCadastralData_Condominium] FOREIGN KEY([CondominiumId])
REFERENCES [dbo].[Condominium] ([CondominiumId])
GO
ALTER TABLE [dbo].[CondominiumCadastralData] CHECK CONSTRAINT [FK_CondominiumCadastralData_Condominium]
GO
ALTER TABLE [dbo].[CondominiumCadastralData]  WITH CHECK ADD  CONSTRAINT [FK_CondominiumCadastralData_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[CondominiumCadastralData] CHECK CONSTRAINT [FK_CondominiumCadastralData_Tenant]
GO
ALTER TABLE [dbo].[CondominiumFee]  WITH CHECK ADD  CONSTRAINT [FK_CondominiumFee_CondominiumInstallment] FOREIGN KEY([InstallmentId])
REFERENCES [dbo].[CondominiumInstallment] ([InstallmentId])
GO
ALTER TABLE [dbo].[CondominiumFee] CHECK CONSTRAINT [FK_CondominiumFee_CondominiumInstallment]
GO
ALTER TABLE [dbo].[CondominiumFee]  WITH CHECK ADD  CONSTRAINT [FK_CondominiumFee_RealEstateUnit] FOREIGN KEY([UnitId])
REFERENCES [dbo].[RealEstateUnit] ([UnitId])
GO
ALTER TABLE [dbo].[CondominiumFee] CHECK CONSTRAINT [FK_CondominiumFee_RealEstateUnit]
GO
ALTER TABLE [dbo].[CondominiumFee]  WITH CHECK ADD  CONSTRAINT [FK_CondominiumFee_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[CondominiumFee] CHECK CONSTRAINT [FK_CondominiumFee_Tenant]
GO
ALTER TABLE [dbo].[CondominiumInstallment]  WITH CHECK ADD  CONSTRAINT [FK_CondominiumInstallment_Budget] FOREIGN KEY([BudgetId])
REFERENCES [dbo].[Budget] ([BudgetId])
GO
ALTER TABLE [dbo].[CondominiumInstallment] CHECK CONSTRAINT [FK_CondominiumInstallment_Budget]
GO
ALTER TABLE [dbo].[CondominiumInstallment]  WITH CHECK ADD  CONSTRAINT [FK_CondominiumInstallment_Condominium] FOREIGN KEY([CondominiumId])
REFERENCES [dbo].[Condominium] ([CondominiumId])
GO
ALTER TABLE [dbo].[CondominiumInstallment] CHECK CONSTRAINT [FK_CondominiumInstallment_Condominium]
GO
ALTER TABLE [dbo].[CondominiumInstallment]  WITH CHECK ADD  CONSTRAINT [FK_CondominiumInstallment_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[CondominiumInstallment] CHECK CONSTRAINT [FK_CondominiumInstallment_Tenant]
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD  CONSTRAINT [FK_Document_Condominium] FOREIGN KEY([CondominiumId])
REFERENCES [dbo].[Condominium] ([CondominiumId])
GO
ALTER TABLE [dbo].[Document] CHECK CONSTRAINT [FK_Document_Condominium]
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD  CONSTRAINT [FK_Document_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[Document] CHECK CONSTRAINT [FK_Document_Tenant]
GO
ALTER TABLE [dbo].[DocumentAccess]  WITH CHECK ADD  CONSTRAINT [FK_DocumentAccess_Document] FOREIGN KEY([DocumentId])
REFERENCES [dbo].[Document] ([DocumentId])
GO
ALTER TABLE [dbo].[DocumentAccess] CHECK CONSTRAINT [FK_DocumentAccess_Document]
GO
ALTER TABLE [dbo].[DocumentAccess]  WITH CHECK ADD  CONSTRAINT [FK_DocumentAccess_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[DocumentAccess] CHECK CONSTRAINT [FK_DocumentAccess_Tenant]
GO
ALTER TABLE [dbo].[Expense]  WITH CHECK ADD  CONSTRAINT [FK_Expense_ChartOfAccounts] FOREIGN KEY([AccountId])
REFERENCES [dbo].[ChartOfAccounts] ([AccountId])
GO
ALTER TABLE [dbo].[Expense] CHECK CONSTRAINT [FK_Expense_ChartOfAccounts]
GO
ALTER TABLE [dbo].[Expense]  WITH CHECK ADD  CONSTRAINT [FK_Expense_Condominium] FOREIGN KEY([CondominiumId])
REFERENCES [dbo].[Condominium] ([CondominiumId])
GO
ALTER TABLE [dbo].[Expense] CHECK CONSTRAINT [FK_Expense_Condominium]
GO
ALTER TABLE [dbo].[Expense]  WITH CHECK ADD  CONSTRAINT [FK_Expense_MillesimalTable] FOREIGN KEY([MillesimalTableId])
REFERENCES [dbo].[MillesimalTable] ([MillesimalTableId])
GO
ALTER TABLE [dbo].[Expense] CHECK CONSTRAINT [FK_Expense_MillesimalTable]
GO
ALTER TABLE [dbo].[Expense]  WITH CHECK ADD  CONSTRAINT [FK_Expense_Supplier] FOREIGN KEY([SupplierId])
REFERENCES [dbo].[Supplier] ([SupplierId])
GO
ALTER TABLE [dbo].[Expense] CHECK CONSTRAINT [FK_Expense_Supplier]
GO
ALTER TABLE [dbo].[Expense]  WITH CHECK ADD  CONSTRAINT [FK_Expense_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[Expense] CHECK CONSTRAINT [FK_Expense_Tenant]
GO
ALTER TABLE [dbo].[ExpenseAllocation]  WITH CHECK ADD  CONSTRAINT [FK_ExpenseAllocation_Expense] FOREIGN KEY([ExpenseId])
REFERENCES [dbo].[Expense] ([ExpenseId])
GO
ALTER TABLE [dbo].[ExpenseAllocation] CHECK CONSTRAINT [FK_ExpenseAllocation_Expense]
GO
ALTER TABLE [dbo].[ExpenseAllocation]  WITH CHECK ADD  CONSTRAINT [FK_ExpenseAllocation_RealEstateUnit] FOREIGN KEY([UnitId])
REFERENCES [dbo].[RealEstateUnit] ([UnitId])
GO
ALTER TABLE [dbo].[ExpenseAllocation] CHECK CONSTRAINT [FK_ExpenseAllocation_RealEstateUnit]
GO
ALTER TABLE [dbo].[ExpenseAllocation]  WITH CHECK ADD  CONSTRAINT [FK_ExpenseAllocation_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[ExpenseAllocation] CHECK CONSTRAINT [FK_ExpenseAllocation_Tenant]
GO
ALTER TABLE [dbo].[Message]  WITH CHECK ADD  CONSTRAINT [FK_Message_Condominium] FOREIGN KEY([CondominiumId])
REFERENCES [dbo].[Condominium] ([CondominiumId])
GO
ALTER TABLE [dbo].[Message] CHECK CONSTRAINT [FK_Message_Condominium]
GO
ALTER TABLE [dbo].[Message]  WITH CHECK ADD  CONSTRAINT [FK_Message_Parent] FOREIGN KEY([ParentMessageId])
REFERENCES [dbo].[Message] ([MessageId])
GO
ALTER TABLE [dbo].[Message] CHECK CONSTRAINT [FK_Message_Parent]
GO
ALTER TABLE [dbo].[Message]  WITH CHECK ADD  CONSTRAINT [FK_Message_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[Message] CHECK CONSTRAINT [FK_Message_Tenant]
GO
ALTER TABLE [dbo].[MillesimalTable]  WITH CHECK ADD  CONSTRAINT [FK_MillesimalTable_Condominium] FOREIGN KEY([CondominiumId])
REFERENCES [dbo].[Condominium] ([CondominiumId])
GO
ALTER TABLE [dbo].[MillesimalTable] CHECK CONSTRAINT [FK_MillesimalTable_Condominium]
GO
ALTER TABLE [dbo].[MillesimalTable]  WITH CHECK ADD  CONSTRAINT [FK_MillesimalTable_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[MillesimalTable] CHECK CONSTRAINT [FK_MillesimalTable_Tenant]
GO
ALTER TABLE [dbo].[RealEstateUnit]  WITH CHECK ADD  CONSTRAINT [FK_RealEstateUnit_Condominium] FOREIGN KEY([CondominiumId])
REFERENCES [dbo].[Condominium] ([CondominiumId])
GO
ALTER TABLE [dbo].[RealEstateUnit] CHECK CONSTRAINT [FK_RealEstateUnit_Condominium]
GO
ALTER TABLE [dbo].[RealEstateUnit]  WITH CHECK ADD  CONSTRAINT [FK_RealEstateUnit_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[RealEstateUnit] CHECK CONSTRAINT [FK_RealEstateUnit_Tenant]
GO
ALTER TABLE [dbo].[Receipt]  WITH CHECK ADD  CONSTRAINT [FK_Receipt_Condominium] FOREIGN KEY([CondominiumId])
REFERENCES [dbo].[Condominium] ([CondominiumId])
GO
ALTER TABLE [dbo].[Receipt] CHECK CONSTRAINT [FK_Receipt_Condominium]
GO
ALTER TABLE [dbo].[Receipt]  WITH CHECK ADD  CONSTRAINT [FK_Receipt_CondominiumFee] FOREIGN KEY([FeeId])
REFERENCES [dbo].[CondominiumFee] ([FeeId])
GO
ALTER TABLE [dbo].[Receipt] CHECK CONSTRAINT [FK_Receipt_CondominiumFee]
GO
ALTER TABLE [dbo].[Receipt]  WITH CHECK ADD  CONSTRAINT [FK_Receipt_RealEstateUnit] FOREIGN KEY([UnitId])
REFERENCES [dbo].[RealEstateUnit] ([UnitId])
GO
ALTER TABLE [dbo].[Receipt] CHECK CONSTRAINT [FK_Receipt_RealEstateUnit]
GO
ALTER TABLE [dbo].[Receipt]  WITH CHECK ADD  CONSTRAINT [FK_Receipt_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[Receipt] CHECK CONSTRAINT [FK_Receipt_Tenant]
GO
ALTER TABLE [dbo].[Supplier]  WITH CHECK ADD  CONSTRAINT [FK_Supplier_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[Supplier] CHECK CONSTRAINT [FK_Supplier_Tenant]
GO
ALTER TABLE [dbo].[SupplierContract]  WITH CHECK ADD  CONSTRAINT [FK_SupplierContract_Condominium] FOREIGN KEY([CondominiumId])
REFERENCES [dbo].[Condominium] ([CondominiumId])
GO
ALTER TABLE [dbo].[SupplierContract] CHECK CONSTRAINT [FK_SupplierContract_Condominium]
GO
ALTER TABLE [dbo].[SupplierContract]  WITH CHECK ADD  CONSTRAINT [FK_SupplierContract_Supplier] FOREIGN KEY([SupplierId])
REFERENCES [dbo].[Supplier] ([SupplierId])
GO
ALTER TABLE [dbo].[SupplierContract] CHECK CONSTRAINT [FK_SupplierContract_Supplier]
GO
ALTER TABLE [dbo].[SupplierContract]  WITH CHECK ADD  CONSTRAINT [FK_SupplierContract_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[SupplierContract] CHECK CONSTRAINT [FK_SupplierContract_Tenant]
GO
ALTER TABLE [dbo].[UnitMillesimal]  WITH CHECK ADD  CONSTRAINT [FK_UnitMillesimal_MillesimalTable] FOREIGN KEY([MillesimalTableId])
REFERENCES [dbo].[MillesimalTable] ([MillesimalTableId])
GO
ALTER TABLE [dbo].[UnitMillesimal] CHECK CONSTRAINT [FK_UnitMillesimal_MillesimalTable]
GO
ALTER TABLE [dbo].[UnitMillesimal]  WITH CHECK ADD  CONSTRAINT [FK_UnitMillesimal_RealEstateUnit] FOREIGN KEY([UnitId])
REFERENCES [dbo].[RealEstateUnit] ([UnitId])
GO
ALTER TABLE [dbo].[UnitMillesimal] CHECK CONSTRAINT [FK_UnitMillesimal_RealEstateUnit]
GO
ALTER TABLE [dbo].[UnitMillesimal]  WITH CHECK ADD  CONSTRAINT [FK_UnitMillesimal_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[UnitMillesimal] CHECK CONSTRAINT [FK_UnitMillesimal_Tenant]
GO
ALTER TABLE [dbo].[UnitOwner]  WITH CHECK ADD  CONSTRAINT [FK_UnitOwner_RealEstateUnit] FOREIGN KEY([UnitId])
REFERENCES [dbo].[RealEstateUnit] ([UnitId])
GO
ALTER TABLE [dbo].[UnitOwner] CHECK CONSTRAINT [FK_UnitOwner_RealEstateUnit]
GO
ALTER TABLE [dbo].[UnitOwner]  WITH CHECK ADD  CONSTRAINT [FK_UnitOwner_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[UnitOwner] CHECK CONSTRAINT [FK_UnitOwner_Tenant]
GO
ALTER TABLE [dbo].[UnitTenant]  WITH CHECK ADD  CONSTRAINT [FK_UnitTenant_RealEstateUnit] FOREIGN KEY([UnitId])
REFERENCES [dbo].[RealEstateUnit] ([UnitId])
GO
ALTER TABLE [dbo].[UnitTenant] CHECK CONSTRAINT [FK_UnitTenant_RealEstateUnit]
GO
ALTER TABLE [dbo].[UnitTenant]  WITH CHECK ADD  CONSTRAINT [FK_UnitTenant_Tenant] FOREIGN KEY([OwnerTenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
GO
ALTER TABLE [dbo].[UnitTenant] CHECK CONSTRAINT [FK_UnitTenant_Tenant]
GO
ALTER TABLE [hangfire_EAGLEFLY_DomuWave].[JobParameter]  WITH CHECK ADD  CONSTRAINT [FK_HangFire_JobParameter_Job] FOREIGN KEY([JobId])
REFERENCES [hangfire_EAGLEFLY_DomuWave].[Job] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [hangfire_EAGLEFLY_DomuWave].[JobParameter] CHECK CONSTRAINT [FK_HangFire_JobParameter_Job]
GO
ALTER TABLE [hangfire_EAGLEFLY_DomuWave].[State]  WITH CHECK ADD  CONSTRAINT [FK_HangFire_State_Job] FOREIGN KEY([JobId])
REFERENCES [hangfire_EAGLEFLY_DomuWave].[Job] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [hangfire_EAGLEFLY_DomuWave].[State] CHECK CONSTRAINT [FK_HangFire_State_Job]
GO
ALTER TABLE [dbo].[Budget]  WITH CHECK ADD  CONSTRAINT [CK_Budget_Status] CHECK  (([Status]='Closed' OR [Status]='Approved' OR [Status]='Draft'))
GO
ALTER TABLE [dbo].[Budget] CHECK CONSTRAINT [CK_Budget_Status]
GO
ALTER TABLE [dbo].[Budget]  WITH CHECK ADD  CONSTRAINT [CK_Budget_Type] CHECK  (([Type]='Actual' OR [Type]='Forecast'))
GO
ALTER TABLE [dbo].[Budget] CHECK CONSTRAINT [CK_Budget_Type]
GO
ALTER TABLE [dbo].[ChartOfAccounts]  WITH CHECK ADD  CONSTRAINT [CK_ChartOfAccounts_Type] CHECK  (([Type]='Expense' OR [Type]='Income'))
GO
ALTER TABLE [dbo].[ChartOfAccounts] CHECK CONSTRAINT [CK_ChartOfAccounts_Type]
GO
ALTER TABLE [dbo].[Communication]  WITH CHECK ADD  CONSTRAINT [CK_Communication_Priority] CHECK  (([Priority]='High' OR [Priority]='Normal' OR [Priority]='Low'))
GO
ALTER TABLE [dbo].[Communication] CHECK CONSTRAINT [CK_Communication_Priority]
GO
ALTER TABLE [dbo].[Communication]  WITH CHECK ADD  CONSTRAINT [CK_Communication_Type] CHECK  (([CommunicationType]='Info' OR [CommunicationType]='Urgent' OR [CommunicationType]='Convocation' OR [CommunicationType]='Notice'))
GO
ALTER TABLE [dbo].[Communication] CHECK CONSTRAINT [CK_Communication_Type]
GO
ALTER TABLE [dbo].[Condominium]  WITH CHECK ADD  CONSTRAINT [CK_Condominium_DueDay] CHECK  (([InstallmentDueDay]>=(1) AND [InstallmentDueDay]<=(31)))
GO
ALTER TABLE [dbo].[Condominium] CHECK CONSTRAINT [CK_Condominium_DueDay]
GO
ALTER TABLE [dbo].[Condominium]  WITH CHECK ADD  CONSTRAINT [CK_Condominium_Frequency] CHECK  (([InstallmentFrequency]='Annual' OR [InstallmentFrequency]='Semiannual' OR [InstallmentFrequency]='Quarterly' OR [InstallmentFrequency]='Monthly'))
GO
ALTER TABLE [dbo].[Condominium] CHECK CONSTRAINT [CK_Condominium_Frequency]
GO
ALTER TABLE [dbo].[CondominiumFee]  WITH CHECK ADD  CONSTRAINT [CK_CondominiumFee_Balance] CHECK  (([Balance]=([AmountDue]-[AmountPaid])))
GO
ALTER TABLE [dbo].[CondominiumFee] CHECK CONSTRAINT [CK_CondominiumFee_Balance]
GO
ALTER TABLE [dbo].[CondominiumFee]  WITH CHECK ADD  CONSTRAINT [CK_CondominiumFee_PaymentStatus] CHECK  (([PaymentStatus]='Overdue' OR [PaymentStatus]='Partial' OR [PaymentStatus]='Paid' OR [PaymentStatus]='ToPay'))
GO
ALTER TABLE [dbo].[CondominiumFee] CHECK CONSTRAINT [CK_CondominiumFee_PaymentStatus]
GO
ALTER TABLE [dbo].[CondominiumInstallment]  WITH CHECK ADD  CONSTRAINT [CK_CondominiumInstallment_Status] CHECK  (([Status]='Cancelled' OR [Status]='Closed' OR [Status]='Open'))
GO
ALTER TABLE [dbo].[CondominiumInstallment] CHECK CONSTRAINT [CK_CondominiumInstallment_Status]
GO
ALTER TABLE [dbo].[DocumentAccess]  WITH CHECK ADD  CONSTRAINT [CK_DocumentAccess_AccessType] CHECK  (([AccessType]='Download' OR [AccessType]='View'))
GO
ALTER TABLE [dbo].[DocumentAccess] CHECK CONSTRAINT [CK_DocumentAccess_AccessType]
GO
ALTER TABLE [dbo].[Expense]  WITH CHECK ADD  CONSTRAINT [CK_Expense_Amounts] CHECK  (([NetAmount]=([GrossAmount]-[VatAmount])))
GO
ALTER TABLE [dbo].[Expense] CHECK CONSTRAINT [CK_Expense_Amounts]
GO
ALTER TABLE [dbo].[Expense]  WITH CHECK ADD  CONSTRAINT [CK_Expense_ExpenseType] CHECK  (([ExpenseType]='Extraordinary' OR [ExpenseType]='Ordinary'))
GO
ALTER TABLE [dbo].[Expense] CHECK CONSTRAINT [CK_Expense_ExpenseType]
GO
ALTER TABLE [dbo].[Expense]  WITH CHECK ADD  CONSTRAINT [CK_Expense_PaymentStatus] CHECK  (([PaymentStatus]='PartiallyPaid' OR [PaymentStatus]='Paid' OR [PaymentStatus]='ToPay'))
GO
ALTER TABLE [dbo].[Expense] CHECK CONSTRAINT [CK_Expense_PaymentStatus]
GO
ALTER TABLE [dbo].[Receipt]  WITH CHECK ADD  CONSTRAINT [CK_Receipt_Amount] CHECK  (([Amount]>(0)))
GO
ALTER TABLE [dbo].[Receipt] CHECK CONSTRAINT [CK_Receipt_Amount]
GO
ALTER TABLE [dbo].[UnitMillesimal]  WITH CHECK ADD  CONSTRAINT [CK_UnitMillesimal_Positive] CHECK  (([Millesimal]>(0)))
GO
ALTER TABLE [dbo].[UnitMillesimal] CHECK CONSTRAINT [CK_UnitMillesimal_Positive]
GO
ALTER TABLE [dbo].[UnitOwner]  WITH CHECK ADD  CONSTRAINT [CK_UnitOwner_Quota] CHECK  (([OwnershipQuota]>(0) AND [OwnershipQuota]<=(100)))
GO
ALTER TABLE [dbo].[UnitOwner] CHECK CONSTRAINT [CK_UnitOwner_Quota]
GO
USE [master]
GO
ALTER DATABASE [DomuWave] SET  READ_WRITE 
GO
