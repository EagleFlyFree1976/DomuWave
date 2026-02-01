using CPQ.Core.DTO;

namespace DomuWave.Services.Models.Dto.Import;

public class ImportDto : BookEntityDto<long>
{
    public  LookupEntityDto<long> TargetAccount { get; set; }

    public  ImportConfigurationDto Configuration { get; set; }

}

public class ImportMinDto : BookEntityDto<long>
{
    public  LookupEntityDto<long> TargetAccount { get; set; }

}