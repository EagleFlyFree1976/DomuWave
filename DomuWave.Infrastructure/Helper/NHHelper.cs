using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Import;

namespace DomuWave.Services.Helper
{
    public static class ImportStatusMap
    {
        public static string GetCode(ImportStatus status) =>
            status switch
            {
                ImportStatus.Duplicate=> "DUP",
                ImportStatus.Error=> "ERR",
                ImportStatus.Excluded=> "EXL",
                ImportStatus.Pending=> "PND",
                ImportStatus.ToBeImported=> "TBI",
                ImportStatus.Processed=> "OK",
                
                _ => throw new ArgumentOutOfRangeException()
            };

        public static ImportStatus GetEnum(string code) =>
            code switch
            {

                "DUP" => ImportStatus.Duplicate,
                "ERR" => ImportStatus.Error,
                "EXL" => ImportStatus.Excluded,
                "PND" => ImportStatus.Pending  ,
                "TBI"=> ImportStatus.ToBeImported,
                "OK"=> ImportStatus.Processed,
                _ => throw new ArgumentOutOfRangeException()
            };
    }
        public static class TransactionTypeMap
        {
            public static string GetCode(TransactionType type) =>
                type switch
                {
                    TransactionType.Entrata => "E",
                    TransactionType.Uscita => "U",
                    TransactionType.Trasferimento => "T",
                    _ => throw new ArgumentOutOfRangeException()
                };

            public static TransactionType GetEnum(string code) =>
                code switch
                {
                    "E" => TransactionType.Entrata,
                    "U" => TransactionType.Uscita,
                    "T" => TransactionType.Trasferimento,
                    _ => throw new ArgumentOutOfRangeException()
                };
            
            public static TransactionType? TryGetEnum(string code) =>
                code?.ToLower() switch
                {
                    "e" => TransactionType.Entrata,
                    "u" => TransactionType.Uscita,
                    "t" => TransactionType.Trasferimento,
                    _ => null
                };
        }
        
        public static class FlowDirectionMap
        {
            public static string GetCode(FlowDirection flowDirection) =>
                flowDirection switch
                {
                    FlowDirection.In => "I",
                    FlowDirection.Out => "O",
                    _ => throw new ArgumentOutOfRangeException()
                };

            public static FlowDirection GetEnum(string code) =>
                code switch
                {
                    "I" => FlowDirection.In,
                    "O" => FlowDirection.Out,
                    
                    _ => throw new ArgumentOutOfRangeException()
                };
            
            public static FlowDirection? TryGetEnum(string code) =>
                code?.ToLower() switch
                {
                    "i" => FlowDirection.In,
                    "o" => FlowDirection.Out,
                    
                    _ => null
                };
        }

   
}
