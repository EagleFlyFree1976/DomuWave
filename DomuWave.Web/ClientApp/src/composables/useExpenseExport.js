import * as XLSX from 'xlsx'
import jsPDF from 'jspdf'
import autoTable from 'jspdf-autotable'
import { useTenantBranding } from '@/composables/useTenantBranding'

// ── helpers (stesso stile di useBilancioExport.js) ────────────────────────────

async function loadReportLogo() {
  try {
    return await useTenantBranding().getReportLogo()
  } catch {
    return null
  }
}

function drawPdfLogo(doc, logo) {
  if (!logo?.dataUrl) return false
  try {
    const fmtImg = (logo.mime || '').includes('png') ? 'PNG'
      : (logo.mime || '').includes('webp') ? 'WEBP'
      : 'JPEG'
    const pageW = doc.internal.pageSize.getWidth()
    const maxW = 32, maxH = 16
    const props = doc.getImageProperties(logo.dataUrl)
    const ratio = props.width / props.height
    let w = maxW, h = maxW / ratio
    if (h > maxH) { h = maxH; w = maxH * ratio }
    doc.addImage(logo.dataUrl, fmtImg, pageW - 14 - w, 10, w, h)
    return true
  } catch {
    return false
  }
}

const fmt = (v) =>
  v != null ? new Intl.NumberFormat('it-IT', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(v) : ''

const fmtDate = (d) => (d ? new Date(d).toLocaleDateString('it-IT') : '')

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

function pdfHeader(doc, title, subtitle, logo = null) {
  drawPdfLogo(doc, logo)
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

const MOVEMENT_LABELS = { 1: 'Entrata', 2: 'Spesa', 3: 'Patrimoniale' }
const PAY_STATUS_LABELS = { 1: 'Non evasa', 2: 'Evasa' }

const COLUMNS = ['Data', 'Tipo', 'Descrizione', 'Fornitore', 'Importo (€)', 'IVA (€)', 'Data pagamento', 'Stato']

function expenseRow(e) {
  return [
    fmtDate(e.documentDate),
    MOVEMENT_LABELS[e.accountType] ?? 'Spesa',
    e.name ?? '',
    e.supplierName ?? '—',
    fmt(e.grossAmount),
    fmt(e.vatAmount),
    e.paymentDate ? fmtDate(e.paymentDate) : '—',
    PAY_STATUS_LABELS[e.paymentStatusId] ?? 'Non evasa',
  ]
}

function totalOf(expenses) {
  return expenses.reduce((sum, e) => sum + (Number(e.grossAmount) || 0), 0)
}

function groupBySupplier(expenses) {
  const groups = new Map()
  for (const e of expenses) {
    const key = e.supplierName || 'Senza fornitore'
    if (!groups.has(key)) groups.set(key, [])
    groups.get(key).push(e)
  }
  return [...groups.entries()].sort((a, b) => a[0].localeCompare(b[0]))
}

// ═══════════════════════════════════════════════════════════════════════════
// PDF
// ═══════════════════════════════════════════════════════════════════════════
export async function exportExpensesPdf(expenses, { subtitle, groupBySupplier: grouped, filename = 'movimenti' } = {}) {
  const logo = await loadReportLogo()
  const doc = new jsPDF({ orientation: 'landscape', unit: 'mm', format: 'a4' })
  let y = pdfHeader(doc, 'Movimenti', subtitle, logo)

  if (grouped) {
    for (const [supplierName, rows] of groupBySupplier(expenses)) {
      doc.setFont('helvetica', 'bold')
      doc.setFontSize(10)
      doc.text(supplierName, 14, y + 4)
      autoTable(doc, {
        startY: y + 7,
        head: [COLUMNS],
        body: rows.map(expenseRow),
        foot: [['', '', '', 'Subtotale', fmt(totalOf(rows)), '', '', '']],
        styles:     { fontSize: 8, cellPadding: 1.5 },
        headStyles: { fillColor: [80, 80, 160], textColor: 255 },
        footStyles: { fillColor: [230, 230, 240], textColor: 0, fontStyle: 'bold' },
        columnStyles: { 4: { halign: 'right' }, 5: { halign: 'right' } },
        margin: { left: 14, right: 14 },
      })
      y = doc.lastAutoTable.finalY + 8
      if (y > doc.internal.pageSize.getHeight() - 30) { doc.addPage(); y = 15 }
    }
    doc.setFont('helvetica', 'bold')
    doc.setFontSize(11)
    doc.text(`Totale generale: € ${fmt(totalOf(expenses))}`, 14, y + 4)
  } else {
    autoTable(doc, {
      startY: y,
      head: [COLUMNS],
      body: expenses.map(expenseRow),
      foot: [['', '', '', 'Totale', fmt(totalOf(expenses)), '', '', '']],
      styles:     { fontSize: 8, cellPadding: 1.5 },
      headStyles: { fillColor: [80, 80, 160], textColor: 255 },
      footStyles: { fillColor: [230, 230, 240], textColor: 0, fontStyle: 'bold' },
      columnStyles: { 4: { halign: 'right' }, 5: { halign: 'right' } },
      margin: { left: 14, right: 14 },
    })
  }

  doc.save(`${filename}.pdf`)
}

// ═══════════════════════════════════════════════════════════════════════════
// Excel
// ═══════════════════════════════════════════════════════════════════════════
export function exportExpensesExcel(expenses, { subtitle, groupBySupplier: grouped, filename = 'movimenti' } = {}) {
  const wb = XLSX.utils.book_new()
  const aoa = [['MOVIMENTI'], [subtitle ?? ''], []]

  if (grouped) {
    for (const [supplierName, rows] of groupBySupplier(expenses)) {
      aoa.push([supplierName])
      aoa.push(COLUMNS)
      for (const e of rows) aoa.push(expenseRow(e))
      aoa.push(['', '', '', 'Subtotale', fmt(totalOf(rows))])
      aoa.push([])
    }
    aoa.push(['', '', '', 'Totale generale', fmt(totalOf(expenses))])
  } else {
    aoa.push(COLUMNS)
    for (const e of expenses) aoa.push(expenseRow(e))
    aoa.push(['', '', '', 'Totale', fmt(totalOf(expenses))])
  }

  const ws = XLSX.utils.aoa_to_sheet(aoa)
  ws['!cols'] = [{ wch: 12 }, { wch: 12 }, { wch: 40 }, { wch: 25 }, { wch: 14 }, { wch: 12 }, { wch: 14 }, { wch: 12 }]
  XLSX.utils.book_append_sheet(wb, ws, 'Movimenti')
  writeExcel(wb, filename)
}
