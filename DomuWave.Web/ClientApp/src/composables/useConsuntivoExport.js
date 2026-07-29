import * as XLSX from 'xlsx'
import jsPDF from 'jspdf'
import autoTable from 'jspdf-autotable'
import { useTenantBranding } from '@/composables/useTenantBranding'

// ── helpers ──────────────────────────────────────────────────────────────────
const fmt = (v) =>
  new Intl.NumberFormat('it-IT', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(v ?? 0)

const fmtDate = (d) => (d ? new Date(d).toLocaleDateString('it-IT') : '—')

// Logo del tenant (data-URL base64) per i report. Fail-safe: null.
async function loadReportLogo() {
  try {
    return await useTenantBranding().getReportLogo()
  } catch {
    return null
  }
}

// Disegna il logo in alto a destra nell'header PDF, se presente.
function drawPdfLogo(doc, logo) {
  if (!logo?.dataUrl) return
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
    doc.addImage(logo.dataUrl, fmtImg, pageW - 14 - w, 8, w, h)
  } catch { /* ignora logo non renderizzabile */ }
}

function triggerDownload(blob, filename) {
  const url = URL.createObjectURL(blob)
  const a   = Object.assign(document.createElement('a'), { href: url, download: filename })
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

// ── Excel ─────────────────────────────────────────────────────────────────────
export function exportConsuntivoExcel(detail, filename = 'consuntivo') {
  const wb = XLSX.utils.book_new()

  // ── Foglio: Per unità (pivot) ──────────────────────────────────────────────
  if (detail.units?.length) {
    const accounts = detail.accounts.map(a => ({
      accountId:   a.accountId,
      accountCode: a.accountCode,
      accountName: a.accountName,
    }))

    const header = [
      'Unità',
      ...accounts.map(a => `${a.accountCode} ${a.accountName}`),
      'Spese ripartite',
      'Dovuto (rate)',
      'Pagato',
      'Saldo',
    ]

    const rows = detail.units.map(u => {
      const cells = [u.unitName]
      for (const acc of accounts) {
        const entry = u.entries.find(e => e.accountId === acc.accountId)
        cells.push(entry?.allocatedAmount ?? 0)
      }
      cells.push(u.total ?? 0)
      cells.push(u.amountDue ?? 0)
      cells.push(u.amountPaid ?? 0)
      cells.push(u.balance ?? 0)
      return cells
    })

    // Riga totale
    const totals = ['Totale']
    for (const acc of accounts) {
      totals.push(detail.accounts.find(a => a.accountId === acc.accountId)?.totalAmount ?? 0)
    }
    totals.push(detail.units.reduce((s, u) => s + (u.total ?? 0), 0))
    totals.push(detail.units.reduce((s, u) => s + (u.amountDue ?? 0), 0))
    totals.push(detail.units.reduce((s, u) => s + (u.amountPaid ?? 0), 0))
    totals.push(detail.units.reduce((s, u) => s + (u.balance ?? 0), 0))

    const wsUnits = XLSX.utils.aoa_to_sheet([header, ...rows, totals])
    wsUnits['!cols'] = [
      { wch: 25 },
      ...accounts.map(() => ({ wch: 14 })),
      { wch: 16 }, { wch: 14 }, { wch: 12 }, { wch: 12 },
    ]
    XLSX.utils.book_append_sheet(wb, wsUnits, 'Per unità')
  }

  const buf  = XLSX.write(wb, { type: 'array', bookType: 'xlsx' })
  const blob = new Blob([buf], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
  triggerDownload(blob, `${filename}.xlsx`)
}

// ── PDF ───────────────────────────────────────────────────────────────────────
export async function exportConsuntivoPdf(detail, title = 'Dettaglio Consuntivo', filename = 'consuntivo') {
  const logo = await loadReportLogo()
  const doc = new jsPDF({ orientation: 'landscape', unit: 'mm', format: 'a4' })

  // ── Intestazione ──────────────────────────────────────────────────────────
  drawPdfLogo(doc, logo)
  doc.setFontSize(14)
  doc.setFont('helvetica', 'bold')
  doc.text(title, 14, 15)
  doc.setFontSize(9)
  doc.setFont('helvetica', 'normal')
  doc.setTextColor(120)
  doc.text(`Generato il ${new Date().toLocaleDateString('it-IT')}`, 14, 21)
  doc.setTextColor(0)

  let y = 28

  // ── Sezione Per unità (pivot) ──────────────────────────────────────────────
  if (detail.units?.length && detail.hasAllocations) {
    doc.setFontSize(11)
    doc.setFont('helvetica', 'bold')
    doc.text('Suddivisione per unità', 14, y)
    y += 4

    const accounts = detail.accounts.map(a => ({
      accountId:   a.accountId,
      accountCode: a.accountCode,
      accountName: a.accountName,
    }))

    const head = [
      'Unità',
      ...accounts.map(a => `${a.accountCode}\n${a.accountName}`),
      'Spese ripartite', 'Dovuto', 'Pagato', 'Saldo',
    ]

    const body = detail.units.map(u => {
      const cells = [u.unitName]
      for (const acc of accounts) {
        const entry = u.entries.find(e => e.accountId === acc.accountId)
        cells.push(fmt(entry?.allocatedAmount ?? 0))
      }
      cells.push(fmt(u.total ?? 0))
      cells.push(fmt(u.amountDue ?? 0))
      cells.push(fmt(u.amountPaid ?? 0))
      cells.push(fmt(u.balance ?? 0))
      return cells
    })

    const footTotals = [{ content: 'TOTALE', styles: { fontStyle: 'bold' } }]
    for (const acc of accounts) {
      footTotals.push({ content: fmt(detail.accounts.find(a => a.accountId === acc.accountId)?.totalAmount ?? 0), styles: { halign: 'right', fontStyle: 'bold' } })
    }
    footTotals.push({ content: fmt(detail.units.reduce((s, u) => s + (u.total ?? 0), 0)),     styles: { halign: 'right', fontStyle: 'bold' } })
    footTotals.push({ content: fmt(detail.units.reduce((s, u) => s + (u.amountDue ?? 0), 0)), styles: { halign: 'right', fontStyle: 'bold' } })
    footTotals.push({ content: fmt(detail.units.reduce((s, u) => s + (u.amountPaid ?? 0), 0)),styles: { halign: 'right', fontStyle: 'bold' } })
    footTotals.push({ content: fmt(detail.units.reduce((s, u) => s + (u.balance ?? 0), 0)),   styles: { halign: 'right', fontStyle: 'bold' } })

    const numericCols = {}
    for (let i = 1; i < head.length; i++) numericCols[i] = { halign: 'right' }

    autoTable(doc, {
      startY: y,
      head: [head],
      body,
      foot: [footTotals],
      styles:      { fontSize: 7.5, cellPadding: 1.5 },
      headStyles:  { fillColor: [80, 80, 160], textColor: 255, fontSize: 7 },
      footStyles:  { fillColor: [50, 50, 120], textColor: 255, fontSize: 8 },
      columnStyles: numericCols,
      margin: { left: 14, right: 14 },
      showFoot: 'lastPage',
    })
  }

  doc.save(`${filename}.pdf`)
}
