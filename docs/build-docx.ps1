# Genera un .docx (Office Open XML) direttamente, senza Word, dal riepilogo Markdown.
$ErrorActionPreference = 'Stop'

$mdPath   = Join-Path $PSScriptRoot 'Riepilogo_Modifiche_2026-06.md'
$docxPath = Join-Path $PSScriptRoot 'Riepilogo_Modifiche_2026-06.docx'
$lines    = Get-Content -LiteralPath $mdPath -Encoding UTF8

function XmlEsc([string]$s) {
    $s = $s -replace '&','&amp;'
    $s = $s -replace '<','&lt;'
    $s = $s -replace '>','&gt;'
    return $s
}

# Costruisce i <w:r> (run) di un paragrafo gestendo il grassetto **...**
function Build-Runs([string]$text) {
    $sb = New-Object System.Text.StringBuilder
    $parts = [regex]::Split($text, '(\*\*[^*]+\*\*)')
    foreach ($p in $parts) {
        if ($p.Length -eq 0) { continue }
        $bold = $false
        $t = $p
        if ($p -match '^\*\*(.+)\*\*$') { $bold = $true; $t = $Matches[1] }
        $rpr = if ($bold) { '<w:rPr><w:b/></w:rPr>' } else { '' }
        [void]$sb.Append('<w:r>' + $rpr + '<w:t xml:space="preserve">' + (XmlEsc $t) + '</w:t></w:r>')
    }
    return $sb.ToString()
}

function Para([string]$styleId, [string]$text, [bool]$bullet=$false) {
    $ppr = ''
    if ($styleId) { $ppr += '<w:pStyle w:val="' + $styleId + '"/>' }
    if ($bullet)  { $ppr += '<w:numPr><w:ilvl w:val="0"/><w:numId w:val="1"/></w:numPr>' }
    $pprXml = if ($ppr) { '<w:pPr>' + $ppr + '</w:pPr>' } else { '' }
    return '<w:p>' + $pprXml + (Build-Runs $text) + '</w:p>'
}

$body = New-Object System.Text.StringBuilder
foreach ($raw in $lines) {
    $line = $raw.TrimEnd()
    if     ($line -match '^---\s*$')      { continue }
    elseif ($line -match '^#\s+(.*)')     { [void]$body.Append((Para 'Heading1' $Matches[1])) }
    elseif ($line -match '^##\s+(.*)')    { [void]$body.Append((Para 'Heading2' $Matches[1])) }
    elseif ($line -match '^###\s+(.*)')   { [void]$body.Append((Para 'Heading3' $Matches[1])) }
    elseif ($line -match '^\s*-\s+(.*)')  { [void]$body.Append((Para '' $Matches[1] $true)) }
    elseif ($line.Trim().Length -eq 0)    { [void]$body.Append('<w:p/>') }
    else                                  { [void]$body.Append((Para '' $line)) }
}

$documentXml = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
<w:body>
'@ + $body.ToString() + @'
<w:sectPr><w:pgSz w:w="11906" w:h="16838"/><w:pgMar w:top="1134" w:right="1134" w:bottom="1134" w:left="1134" w:header="720" w:footer="720" w:gutter="0"/></w:sectPr>
</w:body></w:document>
'@

$stylesXml = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:styles xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
<w:docDefaults><w:rPrDefault><w:rPr><w:rFonts w:ascii="Calibri" w:hAnsi="Calibri"/><w:sz w:val="22"/></w:rPr></w:rPrDefault></w:docDefaults>
<w:style w:type="paragraph" w:default="1" w:styleId="Normal"><w:name w:val="Normal"/></w:style>
<w:style w:type="paragraph" w:styleId="Heading1"><w:name w:val="heading 1"/><w:basedOn w:val="Normal"/><w:pPr><w:spacing w:before="240" w:after="120"/></w:pPr><w:rPr><w:b/><w:color w:val="1F4E79"/><w:sz w:val="36"/></w:rPr></w:style>
<w:style w:type="paragraph" w:styleId="Heading2"><w:name w:val="heading 2"/><w:basedOn w:val="Normal"/><w:pPr><w:spacing w:before="240" w:after="80"/></w:pPr><w:rPr><w:b/><w:color w:val="2E74B5"/><w:sz w:val="28"/></w:rPr></w:style>
<w:style w:type="paragraph" w:styleId="Heading3"><w:name w:val="heading 3"/><w:basedOn w:val="Normal"/><w:pPr><w:spacing w:before="120" w:after="60"/></w:pPr><w:rPr><w:b/><w:color w:val="2E74B5"/><w:sz w:val="24"/></w:rPr></w:style>
</w:styles>
'@

$numberingXml = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:numbering xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
<w:abstractNum w:abstractNumId="0"><w:lvl w:ilvl="0"><w:start w:val="1"/><w:numFmt w:val="bullet"/><w:lvlText w:val="&#8226;"/><w:lvlJc w:val="left"/><w:pPr><w:ind w:left="720" w:hanging="360"/></w:pPr><w:rPr><w:rFonts w:ascii="Symbol" w:hAnsi="Symbol"/></w:rPr></w:lvl></w:abstractNum>
<w:num w:numId="1"><w:abstractNumId w:val="0"/></w:num>
</w:numbering>
'@

$contentTypes = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
<Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
<Default Extension="xml" ContentType="application/xml"/>
<Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
<Override PartName="/word/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml"/>
<Override PartName="/word/numbering.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.numbering+xml"/>
</Types>
'@

$rootRels = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
<Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
</Relationships>
'@

$docRels = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
<Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
<Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/numbering" Target="numbering.xml"/>
</Relationships>
'@

# Costruzione del pacchetto ZIP (.docx)
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

if (Test-Path -LiteralPath $docxPath) { Remove-Item -LiteralPath $docxPath -Force }

$enc = New-Object System.Text.UTF8Encoding($false)   # UTF-8 senza BOM
$fs  = [System.IO.File]::Open($docxPath, [System.IO.FileMode]::CreateNew)
$zip = New-Object System.IO.Compression.ZipArchive($fs, [System.IO.Compression.ZipArchiveMode]::Create)

function Add-Entry($archive, [string]$name, [string]$content, $encoder) {
    $entry  = $archive.CreateEntry($name, [System.IO.Compression.CompressionLevel]::Optimal)
    $stream = $entry.Open()
    $bytes  = $encoder.GetBytes($content)
    $stream.Write($bytes, 0, $bytes.Length)
    $stream.Dispose()
}

Add-Entry $zip '[Content_Types].xml'  $contentTypes  $enc
Add-Entry $zip '_rels/.rels'          $rootRels      $enc
Add-Entry $zip 'word/document.xml'    $documentXml   $enc
Add-Entry $zip 'word/styles.xml'      $stylesXml     $enc
Add-Entry $zip 'word/numbering.xml'   $numberingXml  $enc
Add-Entry $zip 'word/_rels/document.xml.rels' $docRels $enc

$zip.Dispose()
$fs.Dispose()

Write-Output ("Creato: {0} ({1} bytes)" -f $docxPath, (Get-Item $docxPath).Length)
