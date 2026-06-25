import * as XLSX from 'xlsx'
import jsPDF from 'jspdf'
import autoTable from 'jspdf-autotable'

// ── helpers ──────────────────────────────────────────────────────────────────
const fmt = (v) =>
  new Intl.NumberFormat('it-IT', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(v ?? 0)

const fmtDate = (d) => (d ? new Date(d).toLocaleDateString('it-IT') : '—')

function triggerDownload(blob, filename) {
  const url = URL.createObjectURL(blob)
  const a   = Object.assign(document.createElement('a'), { href: url, download: filename })
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

function writeExcel(wb, filename) {
  const buf  = XLSX.write(wb, { type: 'array', bookType: 'xlsx' })
  const blob = new Blob([buf], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
  triggerDownload(blob, `${filename}.xlsx`)
}

// Intestazione comune del PDF.
function pdfHeader(doc, title, subtitle) {
  doc.setFontSize(14)
  doc.setFont('helvetica', 'bold')
  doc.text(title, 14, 15)
  doc.setFontSize(9)
  doc.setFont('helvetica', 'normal')
  doc.setTextColor(120)
  if (subtitle) doc.text(subtitle, 14, 21)
  doc.text(`Generato il ${new Date().toLocaleDateString('it-IT')}`, 14, subtitle ? 26 : 21)
  doc.setTextColor(0)
  return subtitle ? 33 : 28
}

// Tabella a due colonne (Voce | Importo) con sezioni e totali.
function twoColTable(doc, startY, leftTitle, rows) {
  autoTable(doc, {
    startY,
    head: [[
      { content: leftTitle, styles: { fontStyle: 'bold' } },
      { content: 'Importo (€)', styles: { halign: 'right', fontStyle: 'bold' } },
    ]],
    body: rows.map(r => [
      { content: r.label, styles: r.bold ? { fontStyle: 'bold' } : {} },
      { content: fmt(r.value), styles: { halign: 'right', ...(r.bold ? { fontStyle: 'bold' } : {}) } },
    ]),
    styles:     { fontSize: 9, cellPadding: 2 },
    headStyles: { fillColor: [80, 80, 160], textColor: 255 },
    margin:     { left: 14, right: 14 },
  })
  return doc.lastAutoTable.finalY
}

const subtitleOf = (d) =>
  `${d.condominiumName ?? ''} — ${d.fiscalYearCode ?? ''}`.trim()

// ═══════════════════════════════════════════════════════════════════════════
// CONTO ECONOMICO
// ═══════════════════════════════════════════════════════════════════════════
function entrateRows(d) {
  const rows = [{ label: 'Versamenti dei condòmini', value: d.versamentiCondomini }]
  for (const r of d.entrateRows ?? []) rows.push({ label: `${r.accountCode} ${r.accountName}`, value: r.amount })
  rows.push({ label: 'Totale entrate', value: d.totaleEntrate, bold: true })
  return rows
}
function usciteRows(d) {
  const rows = (d.usciteRows ?? []).map(r => ({ label: `${r.accountCode} ${r.accountName}`, value: r.amount }))
  if (d.saldoEsercizioPrecedente) rows.push({ label: 'Saldo esercizio precedente', value: d.saldoEsercizioPrecedente })
  rows.push({ label: 'Totale uscite', value: d.totaleUscite, bold: true })
  return rows
}

export function exportContoEconomicoPdf(d, filename = 'conto-economico') {
  const doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' })
  let y = pdfHeader(doc, 'Conto economico', subtitleOf(d))
  doc.setFontSize(9); doc.setTextColor(120)
  doc.text(`Periodo: ${fmtDate(d.startDate)} – ${fmtDate(d.endDate)}`, 14, y)
  doc.setTextColor(0); y += 6

  y = twoColTable(doc, y, 'Entrate', entrateRows(d)) + 6
  y = twoColTable(doc, y, 'Uscite', usciteRows(d)) + 6

  doc.setFont('helvetica', 'bold'); doc.setFontSize(11)
  doc.text(`${d.saldoTipo}: € ${fmt(d.saldoFinale)}`, 14, y + 2)
  doc.save(`${filename}.pdf`)
}

export function exportContoEconomicoExcel(d, filename = 'conto-economico') {
  const wb = XLSX.utils.book_new()
  const aoa = [
    ['CONTO ECONOMICO', subtitleOf(d)],
    ['Periodo', `${fmtDate(d.startDate)} – ${fmtDate(d.endDate)}`],
    [],
    ['ENTRATE', 'Importo (€)'],
    ...entrateRows(d).map(r => [r.label, r.value]),
    [],
    ['USCITE', 'Importo (€)'],
    ...usciteRows(d).map(r => [r.label, r.value]),
    [],
    [d.saldoTipo, d.saldoFinale],
  ]
  const ws = XLSX.utils.aoa_to_sheet(aoa)
  ws['!cols'] = [{ wch: 45 }, { wch: 16 }]
  XLSX.utils.book_append_sheet(wb, ws, 'Conto economico')
  writeExcel(wb, filename)
}

// ═══════════════════════════════════════════════════════════════════════════
// FLUSSI DI CASSA
// ═══════════════════════════════════════════════════════════════════════════
function incassiRows(d) {
  return [
    { label: 'Avanzo di cassa iniziale', value: d.avanzoInizialeCassa },
    { label: 'Versamenti dei condòmini', value: d.versamentiCondomini },
    { label: 'Totale incassi', value: d.totaleIncassi, bold: true },
  ]
}
function pagamentiRows(d) {
  const rows = [
    { label: "Spese sostenute dell'esercizio", value: d.pagamentiEsercizioCorrente },
    { label: 'Spese di esercizi precedenti', value: d.pagamentiEserciziPrecedenti },
  ]
  if (d.usciteIndividuali) rows.push({ label: 'Uscite individuali', value: d.usciteIndividuali })
  rows.push({ label: 'Totale pagamenti', value: d.totalePagamenti, bold: true })
  rows.push({ label: 'Avanzo di cassa finale', value: d.avanzoFinaleCassa, bold: true })
  return rows
}

export function exportFlussiCassaPdf(d, filename = 'flussi-cassa') {
  const doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' })
  let y = pdfHeader(doc, 'Flussi di cassa', subtitleOf(d))
  doc.setFontSize(9); doc.setTextColor(120)
  doc.text(`Periodo: ${fmtDate(d.startDate)} – ${fmtDate(d.endDate)}`, 14, y)
  doc.setTextColor(0); y += 6

  y = twoColTable(doc, y, 'Incassi', incassiRows(d)) + 6
  twoColTable(doc, y, 'Pagamenti', pagamentiRows(d))
  doc.save(`${filename}.pdf`)
}

export function exportFlussiCassaExcel(d, filename = 'flussi-cassa') {
  const wb = XLSX.utils.book_new()
  const aoa = [
    ['FLUSSI DI CASSA', subtitleOf(d)],
    ['Periodo', `${fmtDate(d.startDate)} – ${fmtDate(d.endDate)}`],
    [],
    ['INCASSI', 'Importo (€)'],
    ...incassiRows(d).map(r => [r.label, r.value]),
    [],
    ['PAGAMENTI', 'Importo (€)'],
    ...pagamentiRows(d).map(r => [r.label, r.value]),
  ]
  const ws = XLSX.utils.aoa_to_sheet(aoa)
  ws['!cols'] = [{ wch: 45 }, { wch: 16 }]
  XLSX.utils.book_append_sheet(wb, ws, 'Flussi di cassa')
  writeExcel(wb, filename)
}

// ═══════════════════════════════════════════════════════════════════════════
// SITUAZIONE PATRIMONIALE
// ═══════════════════════════════════════════════════════════════════════════
function attivitaRows(d) {
  const rows = [{ label: 'Crediti verso condòmini', value: d.creditiVersoCondomini }]
  for (const r of d.disponibilita ?? []) rows.push({ label: `${r.accountCode} ${r.accountName}`, value: r.amount })
  rows.push({ label: 'Totale attività', value: d.totaleAttivita, bold: true })
  return rows
}
function passivitaRows(d) {
  const rows = [
    { label: 'Debiti verso condòmini', value: d.debitiVersoCondomini },
    { label: 'Debiti verso terzi', value: d.debitiVersoTerzi },
  ]
  for (const r of d.fondi ?? []) rows.push({ label: `${r.accountCode} ${r.accountName}`, value: r.amount })
  rows.push({ label: 'Totale passività', value: d.totalePassivita, bold: true })
  return rows
}

export function exportSituazionePatrimonialePdf(d, filename = 'situazione-patrimoniale') {
  const doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' })
  let y = pdfHeader(doc, 'Situazione patrimoniale', subtitleOf(d))
  doc.setFontSize(9); doc.setTextColor(120)
  doc.text(`Al ${fmtDate(d.referenceDate)}`, 14, y)
  doc.setTextColor(0); y += 6

  y = twoColTable(doc, y, 'Attività', attivitaRows(d)) + 6
  twoColTable(doc, y, 'Passività', passivitaRows(d))
  doc.save(`${filename}.pdf`)
}

export function exportSituazionePatrimonialeExcel(d, filename = 'situazione-patrimoniale') {
  const wb = XLSX.utils.book_new()
  const aoa = [
    ['SITUAZIONE PATRIMONIALE', subtitleOf(d)],
    ['Al', fmtDate(d.referenceDate)],
    [],
    ['ATTIVITÀ', 'Importo (€)'],
    ...attivitaRows(d).map(r => [r.label, r.value]),
    [],
    ['PASSIVITÀ', 'Importo (€)'],
    ...passivitaRows(d).map(r => [r.label, r.value]),
  ]
  const ws = XLSX.utils.aoa_to_sheet(aoa)
  ws['!cols'] = [{ wch: 45 }, { wch: 16 }]
  XLSX.utils.book_append_sheet(wb, ws, 'Situazione patrimoniale')
  writeExcel(wb, filename)
}
