namespace DomuWave.Services.Guide;

/// <summary>Un articolo della guida, con corpo Markdown completo.</summary>
public class GuideArticle
{
    public string Slug    { get; set; } = string.Empty;
    public string Title   { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public int    Order   { get; set; }
    public string Body    { get; set; } = string.Empty;
}

/// <summary>Metadati di un articolo (senza corpo) per indice/navigazione.</summary>
public class GuideArticleMeta
{
    public string Slug    { get; set; } = string.Empty;
    public string Title   { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public int    Order   { get; set; }
}

/// <summary>Una sezione della guida con i metadati degli articoli che contiene.</summary>
public class GuideSection
{
    public string Title { get; set; } = string.Empty;
    public int    Order { get; set; }
    public List<GuideArticleMeta> Articles { get; set; } = new();
}

/// <summary>Indice della guida: sezioni ordinate + elenco piatto di metadati.</summary>
public class GuideIndex
{
    public List<GuideSection>      Sections { get; set; } = new();
    public List<GuideArticleMeta>  Articles { get; set; } = new();
}
