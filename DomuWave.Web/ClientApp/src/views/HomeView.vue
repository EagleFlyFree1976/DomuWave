<template>
  <div class="dw-landing">
    <div class="grain"></div>
    <div class="progress" ref="progressBar"></div>

    <!-- ============ NAV ============ -->
    <nav ref="navEl">
      <div class="nav-in">
        <RouterLink class="logo" to="/">
          <span class="logo-mark">
            <svg viewBox="0 0 40 40" fill="none">
              <rect x="4" y="14" width="14" height="22" rx="2" fill="#2e9c6c" />
              <rect x="22" y="6" width="14" height="30" rx="2" fill="#c9a55c" />
              <rect x="8" y="19" width="6" height="3" rx="1" fill="#f6f2e9" opacity="0.85" />
              <rect x="8" y="26" width="6" height="3" rx="1" fill="#f6f2e9" opacity="0.85" />
              <rect x="26" y="11" width="6" height="3" rx="1" fill="#0c1f17" opacity="0.5" />
              <rect x="26" y="18" width="6" height="3" rx="1" fill="#0c1f17" opacity="0.5" />
              <rect x="26" y="25" width="6" height="3" rx="1" fill="#0c1f17" opacity="0.5" />
            </svg>
          </span>
          DomuWave
        </RouterLink>
        <div class="nav-links">
          <a class="muted" href="#problema">Perché</a>
          <a class="muted" href="#funzioni">Funzioni</a>
          <a class="muted" href="#prezzi">Prezzi</a>
          <RouterLink class="muted" to="/login">Accedi</RouterLink>
          <RouterLink class="btn btn-gold" to="/register">Inizia gratis</RouterLink>
        </div>
      </div>
    </nav>

    <!-- ============ HERO ============ -->
    <header class="hero">
      <div class="aurora"><i></i><i></i><i></i></div>

      <div class="wrap hero-grid">
        <div>
          <div class="eyebrow">Per amministratori di condominio</div>
          <h1>L'amministrazione condominiale, <span class="it">finalmente</span> <span class="underline">sotto controllo.</span></h1>
          <p class="lead">Contabilità, millesimi, assemblee e comunicazioni di <b>tutti i tuoi condòmini</b> in un unico posto. Parti gratis con il primo stabile: DomuWave cresce solo quando cresci tu.</p>
          <div class="hero-cta">
            <RouterLink class="btn btn-gold" to="/register">Prova gratis il primo condominio →</RouterLink>
            <a class="btn btn-line" href="#funzioni">Guarda come funziona</a>
          </div>
          <div class="hero-note">
            <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round"><path d="M20 6L9 17l-5-5" /></svg>
            Nessuna carta di credito · attivazione in 2 minuti
          </div>
        </div>

        <div style="position:relative;">
          <div class="glass">
            <div class="glass-head">
              <span class="glass-title">Il tuo studio, oggi</span>
              <span class="live-pill"><span class="p"></span>10 condòmini</span>
            </div>
            <div class="g-row" style="border-top:none;">
              <div class="g-ico"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7"><path d="M3 21h18M5 21V7l7-4 7 4v14M9 9h.01M15 9h.01M9 13h.01M15 13h.01M9 17h.01M15 17h.01" /></svg></div>
              <div><div class="g-name">Condominio Via Verdi 12</div><div class="g-sub">24 unità · bilancio approvato</div></div>
              <div class="g-amt"><b>€ 18.400</b><div class="g-bar"><i data-w="92%"></i></div></div>
            </div>
            <div class="g-row">
              <div class="g-ico"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7"><path d="M3 21h18M5 21V7l7-4 7 4v14M9 9h.01M15 9h.01M9 13h.01M15 13h.01M9 17h.01M15 17h.01" /></svg></div>
              <div><div class="g-name">Residenza Il Giardino</div><div class="g-sub">assemblea il 14 giugno</div></div>
              <div class="g-amt"><b>€ 9.250</b><div class="g-bar"><i data-w="78%"></i></div></div>
            </div>
            <div class="g-row">
              <div class="g-ico"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7"><path d="M3 21h18M5 21V7l7-4 7 4v14M9 9h.01M15 9h.01M9 13h.01M15 13h.01M9 17h.01M15 17h.01" /></svg></div>
              <div><div class="g-name">Palazzo Aurora</div><div class="g-sub">3 fornitori da saldare</div></div>
              <div class="g-amt"><b>€ 31.700</b><div class="g-bar"><i data-w="85%"></i></div></div>
            </div>
          </div>

          <div class="float-card a">
            <div class="fc-ico green"><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" /><path d="M22 4L12 14.01l-3-3" /></svg></div>
            <div><b>Verbale generato</b><span>pronto per l'invio</span></div>
          </div>
          <div class="float-card b">
            <div class="fc-ico gold"><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2"><path d="M12 1v22M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" /></svg></div>
            <div><b>Rata riscossa</b><span>Via Verdi · int. 7</span></div>
          </div>
        </div>
      </div>

      <div class="marquee">
        <div class="marquee-track">
          <span><i class="star">✦</i> Contabilità condominiale</span>
          <span><i class="star">✦</i> Tabelle millesimali</span>
          <span><i class="star">✦</i> Assemblee e verbali</span>
          <span><i class="star">✦</i> Comunicazioni ai condòmini</span>
          <span><i class="star">✦</i> Portale condòmini</span>
          <span><i class="star">✦</i> Rate e riscossioni</span>
          <span><i class="star">✦</i> Contabilità condominiale</span>
          <span><i class="star">✦</i> Tabelle millesimali</span>
          <span><i class="star">✦</i> Assemblee e verbali</span>
          <span><i class="star">✦</i> Comunicazioni ai condòmini</span>
          <span><i class="star">✦</i> Portale condòmini</span>
          <span><i class="star">✦</i> Rate e riscossioni</span>
        </div>
      </div>
    </header>

    <!-- ============ PROBLEMA ============ -->
    <section class="sec-pad" id="problema">
      <div class="wrap">
        <div class="sec-label reveal">Il problema</div>
        <h2 class="sec-title reveal">Più condòmini gestisci, <em>più cresce il caos.</em></h2>
        <p class="sec-intro reveal">Fogli di calcolo diversi per ogni stabile, scadenze annotate a mano, condòmini che chiamano per ogni rata. La burocrazia si moltiplica e divora il tempo che dovresti dedicare a far crescere lo studio.</p>
        <div class="pain-grid">
          <div class="pain-card reveal">
            <div class="pain-num">01 —</div>
            <h3>Il tempo evapora</h3>
            <p>Ripartizioni millesimali, preparazione assemblee, rincorsa dei pagamenti: ore di lavoro manuale, da rifare identiche per ogni condominio.</p>
          </div>
          <div class="pain-card reveal reveal-d1">
            <div class="pain-num">02 —</div>
            <h3>Gli errori costano cari</h3>
            <p>Una rata calcolata male o una scadenza dimenticata diventa una contestazione in assemblea. E la fiducia, una volta persa, non torna.</p>
          </div>
          <div class="pain-card reveal reveal-d2">
            <div class="pain-num">03 —</div>
            <h3>Il telefono non smette</h3>
            <p>"Quanto devo pagare? A che punto sono i lavori?" Le stesse domande, ogni giorno, senza un posto dove i condòmini trovino le risposte da soli.</p>
          </div>
        </div>
      </div>
    </section>

    <!-- ============ FUNZIONI ============ -->
    <section id="funzioni">
      <div class="band-dark sec-pad">
        <div class="aurora"><i></i><i></i></div>
        <div class="wrap" style="position:relative;z-index:2;">
          <div class="sec-label reveal">La soluzione</div>
          <h2 class="sec-title reveal">Un solo posto per amministrare <em>tutto, davvero.</em></h2>
          <p class="sec-intro reveal">DomuWave copre l'intero ciclo di vita del condominio: dalla contabilità all'assemblea, dai documenti alle comunicazioni. Nessun modulo da comprare a parte per fare il tuo lavoro di base.</p>

          <div class="feat">
            <div class="feat-text reveal">
              <h3>Contabilità e millesimi, senza fatica</h3>
              <p class="feat-desc">Preventivo, consuntivo, riparto per tabelle millesimali, rate e quote: tutto calcolato in automatico, sempre in regola con la normativa italiana.</p>
              <ul class="feat-list">
                <li><span class="ck"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M20 6L9 17l-5-5" /></svg></span><span><b>Bilanci guidati</b> — preventivo e consuntivo passo passo</span></li>
                <li><span class="ck"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M20 6L9 17l-5-5" /></svg></span><span><b>Riparto automatico</b> — per millesimi, scale e unità</span></li>
                <li><span class="ck"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M20 6L9 17l-5-5" /></svg></span><span><b>Riscossioni in tempo reale</b> — chi ha pagato, chi è in ritardo</span></li>
              </ul>
            </div>
            <div class="glass reveal reveal-d1">
              <div class="glass-head"><span class="glass-title">Riparto spese · Via Verdi 12</span><span class="live-pill">consuntivo</span></div>
              <div class="mini-head"><span>Scala A — riscaldamento</span><b>€ 6.240</b></div>
              <div class="mini-bar"><i data-w="88%"></i></div>
              <div class="mini-head"><span>Scala B — pulizie</span><b>€ 2.180</b></div>
              <div class="mini-bar gold"><i data-w="54%"></i></div>
              <div class="mini-head"><span>Parti comuni — ascensore</span><b>€ 3.900</b></div>
              <div class="mini-bar"><i data-w="70%"></i></div>
              <div style="font-size:13px;color:rgba(246,242,233,0.55);margin-top:8px;display:flex;align-items:center;gap:8px;">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10" /><path d="M12 8v4M12 16h.01" /></svg>
                Riparto ricalcolato automaticamente a ogni nuova spesa
              </div>
            </div>
          </div>

          <div class="feat rev">
            <div class="feat-text reveal">
              <h3>Assemblee pronte in minuti, non in giorni</h3>
              <p class="feat-desc">Convocazioni, ordine del giorno, registro presenze e verbale: prepari un'assemblea in pochi clic e comunichi con tutti i condòmini da un unico posto.</p>
              <ul class="feat-list">
                <li><span class="ck"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M20 6L9 17l-5-5" /></svg></span><span><b>Convocazioni e verbali</b> — generati e pronti da inviare</span></li>
                <li><span class="ck"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M20 6L9 17l-5-5" /></svg></span><span><b>Comunicazioni massive</b> — a tutto il condominio in un invio</span></li>
                <li><span class="ck"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M20 6L9 17l-5-5" /></svg></span><span><b>Portale condòmini</b> — documenti e rate consultabili da soli</span></li>
              </ul>
            </div>
            <div class="glass reveal reveal-d1">
              <div class="glass-head"><span class="glass-title">Assemblea · 14 giugno, 18:30</span><span class="live-pill"><span class="p"></span>convocata</span></div>
              <div class="odg-row"><span class="odg-num">i.</span><div><b>Approvazione consuntivo 2025</b><span>delibera a maggioranza semplice</span></div></div>
              <div class="odg-row"><span class="odg-num">ii.</span><div><b>Lavori facciata — scelta preventivo</b><span>3 preventivi allegati alla convocazione</span></div></div>
              <div class="odg-row"><span class="odg-num">iii.</span><div><b>Nomina nuovo consigliere scala B</b><span>candidature aperte</span></div></div>
              <div style="font-size:13px;color:rgba(246,242,233,0.55);margin-top:14px;display:flex;align-items:center;gap:8px;">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" /><circle cx="9" cy="7" r="4" /><path d="M23 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" /></svg>
                18 condòmini convocati · presenze e deleghe tracciate
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ============ PREZZI ============ -->
    <section class="sec-pad" id="prezzi">
      <div class="wrap">
        <div class="sec-label reveal">Prezzi onesti</div>
        <h2 class="sec-title reveal">Paghi solo per i condòmini <em>che gestisci.</em></h2>
        <p class="sec-intro reveal">Inizia gratis con uno stabile. Quando il tuo studio cresce, cresce anche DomuWave — con sconti progressivi su ogni condominio in più. Nessun canone gonfiato all'ingresso.</p>

        <div class="price-grid">
          <div class="price-card reveal">
            <div class="price-name">Avvio</div>
            <p class="price-for">Per provare DomuWave su un condominio reale, senza rischi né scadenze.</p>
            <div class="price-tag">€0 <span>/ per sempre</span></div>
            <ul class="price-feats">
              <li><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5" /></svg>1 condominio completo</li>
              <li><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5" /></svg>Contabilità e millesimi inclusi</li>
              <li><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5" /></svg>1 assemblea inclusa</li>
              <li><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5" /></svg>Portale condòmini</li>
            </ul>
            <RouterLink class="btn btn-dark" to="/register">Inizia gratis</RouterLink>
          </div>

          <div class="price-card hero-plan reveal reveal-d1">
            <span class="plan-badge">Più scelto</span>
            <div class="price-name">Studio</div>
            <p class="price-for">Per l'amministratore in crescita: condòmini illimitati, sconti progressivi dal 5° stabile.</p>
            <div class="price-tag">€6 <span>/ condominio / mese</span></div>
            <ul class="price-feats">
              <li><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5" /></svg>Condòmini illimitati</li>
              <li><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5" /></svg>Assemblee e comunicazioni incluse</li>
              <li><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5" /></svg>Sconti progressivi fino al 30%</li>
              <li><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5" /></svg>Assistenza prioritaria</li>
            </ul>
            <RouterLink class="btn btn-gold" to="/register">Prova 60 giorni gratis</RouterLink>
          </div>

          <div class="price-card reveal reveal-d2">
            <div class="price-name">A consumo</div>
            <p class="price-for">Solo quando ti serve di più: ricarichi le quote e paghi l'uso effettivo.</p>
            <div class="price-tag" style="font-size:38px;padding-top:10px;">Add-on</div>
            <ul class="price-feats">
              <li><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5" /></svg>Assemblee extra</li>
              <li><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5" /></svg>Invii PEC e raccomandate</li>
              <li><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5" /></svg>Spazio documenti aggiuntivo</li>
              <li><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5" /></svg>Ricariche quando vuoi</li>
            </ul>
            <RouterLink class="btn" style="background:var(--cream-2);color:var(--ink);" to="/register">Scopri gli extra</RouterLink>
          </div>
        </div>
        <p class="price-note">Prezzi indicativi, IVA esclusa · nessun vincolo annuale · cambi o disdici quando vuoi</p>
      </div>
    </section>

    <!-- ============ NUMERI ============ -->
    <section class="sec-pad" style="padding-top:30px;">
      <div class="wrap">
        <div class="numbers">
          <div class="num-item reveal"><div class="num-val">€0</div><div class="num-lbl">per iniziare<br>col primo condominio</div></div>
          <div class="num-item reveal reveal-d1"><div class="num-val">100<span class="gold">%</span></div><div class="num-lbl">italiano: normativa<br>e fisco condominiale</div></div>
          <div class="num-item reveal reveal-d2"><div class="num-val">1</div><div class="num-lbl">unico posto per contabilità,<br>assemblee e comunicazioni</div></div>
          <div class="num-item reveal reveal-d3"><div class="num-val"><span class="gold">∞</span></div><div class="num-lbl">condòmini, con sconti<br>che crescono col tuo studio</div></div>
        </div>
      </div>
    </section>

    <!-- ============ FINAL CTA ============ -->
    <section class="final">
      <div class="aurora"><i></i><i></i></div>
      <h2 class="reveal">Prova DomuWave sul tuo <em>prossimo</em> condominio.</h2>
      <p class="reveal">Attivi il piano gratuito in due minuti, importi uno stabile e vedi con i tuoi occhi quanto tempo ti restituisce.</p>
      <RouterLink class="btn btn-gold reveal" to="/register">Inizia gratis ora →</RouterLink>
      <div class="hero-note reveal" style="margin-top:24px;">
        <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round"><path d="M20 6L9 17l-5-5" /></svg>
        Senza carta di credito · senza impegno
      </div>
    </section>

    <!-- ============ FOOTER ============ -->
    <footer>
      <div class="wrap foot-in">
        <RouterLink class="logo" to="/" style="font-size:20px;">
          <span class="logo-mark" style="width:28px;height:28px;">
            <svg viewBox="0 0 40 40" fill="none">
              <rect x="4" y="14" width="14" height="22" rx="2" fill="#2e9c6c" />
              <rect x="22" y="6" width="14" height="30" rx="2" fill="#c9a55c" />
            </svg>
          </span>
          DomuWave
        </RouterLink>
        <div class="foot-links">
          <a href="#funzioni">Funzioni</a>
          <a href="#prezzi">Prezzi</a>
          <a href="#">Privacy</a>
          <a href="#">Contatti</a>
        </div>
        <div>© {{ new Date().getFullYear() }} DomuWave · Il gestionale condominiale italiano</div>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { onMounted, onBeforeUnmount, ref } from 'vue'

const navEl = ref(null)
const progressBar = ref(null)

let onScroll = null
let revealObserver = null
let barsObserver = null

onMounted(() => {
  // nav on scroll + progress bar
  onScroll = () => {
    if (navEl.value) navEl.value.classList.toggle('scrolled', window.scrollY > 30)
    const h = document.documentElement
    if (progressBar.value) {
      progressBar.value.style.width = (h.scrollTop / (h.scrollHeight - h.clientHeight) * 100) + '%'
    }
  }
  window.addEventListener('scroll', onScroll, { passive: true })

  // reveals
  revealObserver = new IntersectionObserver((es) => {
    es.forEach(e => {
      if (e.isIntersecting) {
        e.target.classList.add('in')
        revealObserver.unobserve(e.target)
      }
    })
  }, { threshold: 0.14 })
  document.querySelectorAll('.dw-landing .reveal').forEach(el => revealObserver.observe(el))

  // animated bars when visible
  barsObserver = new IntersectionObserver((es) => {
    es.forEach(e => {
      if (e.isIntersecting) {
        e.target.querySelectorAll('[data-w]').forEach((b, i) =>
          setTimeout(() => { b.style.width = b.dataset.w }, 150 + i * 180))
        barsObserver.unobserve(e.target)
      }
    })
  }, { threshold: 0.3 })
  document.querySelectorAll('.dw-landing .glass').forEach(el => barsObserver.observe(el))
})

onBeforeUnmount(() => {
  if (onScroll) window.removeEventListener('scroll', onScroll)
  if (revealObserver) revealObserver.disconnect()
  if (barsObserver) barsObserver.disconnect()
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600&display=swap');

.dw-landing {
  --night: #0c1f17;
  --night-2: #11291f;
  --pine: #1b4a35;
  --emerald: #2e9c6c;
  --mint: #8fd6b4;
  --cream: #f6f2e9;
  --cream-2: #ece6d8;
  --ink: #14201a;
  --ink-soft: #4d5c54;
  --brass: #c9a55c;
  --brass-soft: #e3cf9e;
  --line-dark: rgba(246, 242, 233, 0.12);
  --line-light: rgba(20, 32, 26, 0.1);

  font-family: 'Outfit', sans-serif;
  background: var(--cream);
  color: var(--ink);
  line-height: 1.65;
  overflow-x: hidden;
  position: relative;
}

.dw-landing ::selection { background: var(--emerald); color: var(--cream); }
.dw-landing h1, .dw-landing h2, .dw-landing h3 {
  font-family: 'Clash Display', 'Outfit', sans-serif;
  font-weight: 500; line-height: 1.08; letter-spacing: -0.015em;
}
.wrap { max-width: 1200px; margin: 0 auto; padding: 0 32px; }
.dw-landing a { text-decoration: none; color: inherit; }
.dw-landing img { max-width: 100%; }

/* grain */
.grain {
  position: fixed; inset: -50%; width: 200%; height: 200%; pointer-events: none; z-index: 100; opacity: 0.05;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='240' height='240'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
  animation: grainShift 9s steps(8) infinite;
}
@keyframes grainShift {
  0%, 100% { transform: translate(0, 0) } 12% { transform: translate(-3%, 2%) } 25% { transform: translate(2%, -3%) }
  37% { transform: translate(-2%, -2%) } 50% { transform: translate(3%, 3%) } 62% { transform: translate(-3%, 1%) }
  75% { transform: translate(2%, 2%) } 87% { transform: translate(-1%, -3%) }
}

/* scroll progress */
.progress { position: fixed; top: 0; left: 0; height: 3px; background: linear-gradient(90deg, var(--emerald), var(--brass)); z-index: 200; width: 0%; transition: width 0.1s linear; }

/* ============ NAV ============ */
.dw-landing nav { position: fixed; top: 0; left: 0; right: 0; z-index: 90; transition: background .35s, box-shadow .35s, border-color .35s; border-bottom: 1px solid transparent; }
.dw-landing nav.scrolled { background: rgba(12, 31, 23, 0.85); backdrop-filter: blur(16px); border-color: var(--line-dark); }
.nav-in { display: flex; align-items: center; justify-content: space-between; padding: 20px 32px; max-width: 1200px; margin: 0 auto; }
.logo { display: flex; align-items: center; gap: 11px; font-family: 'Clash Display', 'Outfit', sans-serif; font-weight: 500; font-size: 23px; color: var(--cream); letter-spacing: -0.01em; }
.logo-mark { width: 34px; height: 34px; flex-shrink: 0; position: relative; }
.logo-mark svg { width: 100%; height: 100%; }
.nav-links { display: flex; align-items: center; gap: 34px; font-size: 15px; }
.nav-links a.muted { color: rgba(246, 242, 233, 0.72); transition: color .2s; position: relative; }
.nav-links a.muted::after { content: ""; position: absolute; left: 0; bottom: -5px; width: 0; height: 1.5px; background: var(--brass); transition: width .25s; }
.nav-links a.muted:hover { color: var(--cream); }
.nav-links a.muted:hover::after { width: 100%; }
.btn {
  display: inline-flex; align-items: center; gap: 9px; font-weight: 500; font-size: 15px; font-family: inherit;
  padding: 12px 26px; border-radius: 999px; cursor: pointer; border: none;
  transition: transform .18s cubic-bezier(.34, 1.56, .64, 1), box-shadow .25s, background .25s;
  position: relative; overflow: hidden;
}
.btn-gold { background: linear-gradient(120deg, var(--brass-soft), var(--brass)); color: var(--night); box-shadow: 0 8px 30px -8px rgba(201, 165, 92, 0.55); }
.btn-gold:hover { transform: translateY(-2px) scale(1.02); box-shadow: 0 14px 38px -8px rgba(201, 165, 92, 0.7); }
.btn-green { background: var(--emerald); color: var(--cream); box-shadow: 0 10px 32px -10px rgba(46, 156, 108, 0.6); }
.btn-green:hover { transform: translateY(-2px) scale(1.02); }
.btn-line { background: transparent; border: 1px solid rgba(246, 242, 233, 0.3); color: var(--cream); }
.btn-line:hover { background: rgba(246, 242, 233, 0.08); border-color: rgba(246, 242, 233, 0.55); }
@media (max-width: 760px) { .nav-links .muted { display: none; } }

/* ============ HERO ============ */
.hero {
  position: relative; min-height: 100svh; display: flex; flex-direction: column; justify-content: center;
  background: var(--night); color: var(--cream); overflow: hidden; padding: 140px 0 0;
}
.aurora { display: none; }
.aurora i { position: absolute; border-radius: 50%; filter: blur(90px); opacity: .5; mix-blend-mode: screen; }
.aurora i:nth-child(1) { width: 520px; height: 520px; background: #2e9c6c; top: -160px; right: -80px; animation: drift1 14s ease-in-out infinite alternate; }
.aurora i:nth-child(2) { width: 420px; height: 420px; background: #c9a55c; bottom: -180px; left: -120px; opacity: .25; animation: drift2 18s ease-in-out infinite alternate; }
.aurora i:nth-child(3) { width: 300px; height: 300px; background: #8fd6b4; top: 35%; left: 42%; opacity: .15; animation: drift1 22s ease-in-out infinite alternate-reverse; }
@keyframes drift1 { from { transform: translate(0, 0) scale(1); } to { transform: translate(-60px, 50px) scale(1.15); } }
@keyframes drift2 { from { transform: translate(0, 0); } to { transform: translate(70px, -40px); } }

.hero-grid { display: grid; grid-template-columns: 1.1fr 0.9fr; gap: 60px; align-items: center; position: relative; z-index: 3; }
.eyebrow { display: inline-flex; align-items: center; gap: 10px; font-size: 12.5px; font-weight: 600; letter-spacing: 0.16em; text-transform: uppercase; color: var(--brass-soft); margin-bottom: 28px; }
.eyebrow::before { content: ""; width: 36px; height: 1px; background: var(--brass); }
.hero h1 { font-size: clamp(42px, 5.8vw, 72px); margin-bottom: 26px; font-weight: 500; }
.hero h1 .it { font-style: normal; font-weight: 600; color: var(--mint); }
.hero h1 .underline { position: relative; white-space: nowrap; }
.hero h1 .underline::after {
  content: ""; position: absolute; left: 0; right: 0; bottom: 6px; height: 10px;
  background: linear-gradient(90deg, var(--brass), transparent); opacity: 0.5; z-index: -1;
  transform: scaleX(0); transform-origin: left; animation: drawLine 1s .9s cubic-bezier(.22, 1, .36, 1) forwards;
}
@keyframes drawLine { to { transform: scaleX(1); } }
.hero p.lead { font-size: 19px; font-weight: 300; color: rgba(246, 242, 233, 0.78); max-width: 530px; margin-bottom: 38px; }
.hero p.lead b { font-weight: 500; color: var(--cream); }
.hero-cta { display: flex; gap: 16px; align-items: center; flex-wrap: wrap; }
.hero-note { font-size: 14px; color: rgba(246, 242, 233, 0.6); margin-top: 22px; display: flex; align-items: center; gap: 9px; }
.hero-note svg { color: var(--mint); flex-shrink: 0; }

.glass {
  background: linear-gradient(160deg, rgba(246, 242, 233, 0.1), rgba(246, 242, 233, 0.04));
  border: 1px solid rgba(246, 242, 233, 0.16);
  border-radius: 24px; backdrop-filter: blur(14px);
  box-shadow: 0 40px 80px -30px rgba(0, 0, 0, 0.55), inset 0 1px 0 rgba(246, 242, 233, 0.15);
  padding: 26px; position: relative;
}
.glass-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 20px; }
.glass-title { font-family: 'Clash Display', 'Outfit', sans-serif; font-size: 17px; letter-spacing: 0.01em; }
.live-pill { display: inline-flex; align-items: center; gap: 7px; font-size: 12px; font-weight: 600; padding: 5px 13px; border-radius: 999px; background: rgba(46, 156, 108, 0.2); color: var(--mint); border: 1px solid rgba(143, 214, 180, 0.25); }
.live-pill .p { width: 7px; height: 7px; border-radius: 50%; background: var(--mint); animation: pulse 2s infinite; }
@keyframes pulse { 0%, 100% { box-shadow: 0 0 0 0 rgba(143, 214, 180, 0.5); } 50% { box-shadow: 0 0 0 6px rgba(143, 214, 180, 0); } }
.g-row { display: flex; align-items: center; gap: 15px; padding: 15px 4px; border-top: 1px solid var(--line-dark); transition: background .2s, padding-left .2s; border-radius: 10px; }
.g-row:hover { background: rgba(246, 242, 233, 0.05); padding-left: 12px; }
.g-ico { width: 42px; height: 42px; border-radius: 12px; background: rgba(246, 242, 233, 0.08); border: 1px solid var(--line-dark); display: flex; align-items: center; justify-content: center; flex-shrink: 0; color: var(--mint); }
.g-ico svg { width: 21px; height: 21px; }
.g-name { font-weight: 500; font-size: 15px; }
.g-sub { font-size: 12.5px; color: rgba(246, 242, 233, 0.55); }
.g-amt { margin-left: auto; text-align: right; }
.g-amt b { font-family: 'Clash Display', 'Outfit', sans-serif; font-size: 17px; font-weight: 500; }
.g-bar { width: 90px; height: 5px; border-radius: 3px; background: rgba(246, 242, 233, 0.12); margin-top: 6px; overflow: hidden; }
.g-bar i { display: block; height: 100%; border-radius: 3px; background: linear-gradient(90deg, var(--emerald), var(--mint)); width: 0; transition: width 1.4s cubic-bezier(.22, 1, .36, 1); }
.float-card {
  position: absolute; background: var(--cream); color: var(--ink); border-radius: 16px; padding: 14px 18px;
  box-shadow: 0 24px 50px -18px rgba(0, 0, 0, 0.5); display: flex; align-items: center; gap: 12px; z-index: 4;
  animation: floaty 5s ease-in-out infinite;
}
.float-card.a { bottom: -26px; left: -34px; animation-delay: 0s; }
.float-card.b { top: 40%; right: -36px; animation-delay: 2.2s; }
@keyframes floaty { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(-9px); } }
.fc-ico { width: 36px; height: 36px; border-radius: 10px; display: flex; align-items: center; justify-content: center; color: #fff; flex-shrink: 0; }
.fc-ico.green { background: linear-gradient(135deg, var(--emerald), var(--pine)); }
.fc-ico.gold { background: linear-gradient(135deg, var(--brass), #a8843c); }
.float-card b { font-family: 'Clash Display', 'Outfit', sans-serif; display: block; font-size: 14.5px; font-weight: 500; line-height: 1.2; }
.float-card span { font-size: 12px; color: var(--ink-soft); }

@media (max-width: 920px) {
  .hero { padding-top: 120px; }
  .hero-grid { grid-template-columns: 1fr; gap: 70px; }
  .float-card.a { left: 0; }
  .float-card.b { right: 0; }
}

/* marquee */
.marquee { position: relative; z-index: 3; border-top: 1px solid var(--line-dark); margin-top: 80px; overflow: hidden; background: rgba(0, 0, 0, 0.18); }
.marquee-track { display: flex; gap: 60px; white-space: nowrap; padding: 16px 0; animation: scrollX 26s linear infinite; width: max-content; }
.marquee-track span { display: inline-flex; align-items: center; gap: 12px; font-size: 13.5px; letter-spacing: 0.12em; text-transform: uppercase; color: rgba(246, 242, 233, 0.55); font-weight: 500; }
.marquee-track .star { color: var(--brass); font-size: 15px; }
@keyframes scrollX { from { transform: translateX(0); } to { transform: translateX(-50%); } }

/* ============ SECTIONS (light) ============ */
.dw-landing section { position: relative; }
.sec-pad { padding: 110px 0; }
.sec-label { display: inline-flex; align-items: center; gap: 10px; font-size: 12.5px; font-weight: 600; letter-spacing: 0.16em; text-transform: uppercase; color: var(--pine); margin-bottom: 20px; }
.sec-label::before { content: ""; width: 36px; height: 1px; background: var(--brass); }
.sec-title { font-size: clamp(32px, 4.4vw, 52px); margin-bottom: 20px; max-width: 700px; font-weight: 400; }
.sec-title em { font-style: normal; color: var(--pine); }
.sec-intro { font-size: 18px; color: var(--ink-soft); max-width: 600px; font-weight: 300; }

/* pain cards */
.pain-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 24px; margin-top: 60px; }
.pain-card { background: #fff; border: 1px solid var(--line-light); border-radius: 22px; padding: 34px 30px; position: relative; overflow: hidden; transition: transform .3s cubic-bezier(.22, 1, .36, 1), box-shadow .3s; }
.pain-card::before { content: ""; position: absolute; top: 0; left: 0; right: 0; height: 4px; background: linear-gradient(90deg, var(--emerald), var(--brass)); transform: scaleX(0); transform-origin: left; transition: transform .4s; }
.pain-card:hover { transform: translateY(-6px); box-shadow: 0 30px 60px -25px rgba(27, 74, 53, 0.3); }
.pain-card:hover::before { transform: scaleX(1); }
.pain-num { font-family: 'Clash Display', 'Outfit', sans-serif; font-style: normal; font-size: 15px; color: var(--brass); margin-bottom: 16px; }
.pain-card h3 { font-size: 23px; margin-bottom: 12px; font-weight: 500; }
.pain-card p { font-size: 15.5px; color: var(--ink-soft); font-weight: 300; }
@media(max-width: 880px) { .pain-grid { grid-template-columns: 1fr; } }

/* features dark band */
.band-dark { background: var(--night); color: var(--cream); border-radius: 36px; margin: 0 20px; position: relative; overflow: hidden; }
.band-dark .aurora i:nth-child(1) { width: 420px; height: 420px; top: -120px; right: -60px; opacity: 0.35; }
.band-dark .aurora i:nth-child(2) { width: 340px; height: 340px; bottom: -120px; left: -80px; opacity: 0.18; }
.band-dark .sec-label { color: var(--brass-soft); }
.band-dark .sec-intro { color: rgba(246, 242, 233, 0.7); }
.band-dark .sec-title em { color: var(--mint); }

.feat { display: grid; grid-template-columns: 1fr 1fr; gap: 70px; align-items: center; margin-top: 80px; position: relative; z-index: 2; }
.feat.rev .feat-text { order: 2; }
.feat h3 { font-size: 31px; font-weight: 500; }
.feat .feat-desc { color: rgba(246, 242, 233, 0.7); font-size: 16.5px; margin-top: 14px; font-weight: 300; }
.feat-list { list-style: none; margin-top: 28px; display: flex; flex-direction: column; gap: 17px; }
.feat-list li { display: flex; gap: 14px; align-items: flex-start; font-size: 16px; font-weight: 300; }
.feat-list li b { font-weight: 500; }
.feat-list .ck { width: 26px; height: 26px; border-radius: 8px; background: rgba(46, 156, 108, 0.18); border: 1px solid rgba(143, 214, 180, 0.3); display: flex; align-items: center; justify-content: center; flex-shrink: 0; margin-top: 1px; color: var(--mint); }
@media(max-width: 920px) { .feat { grid-template-columns: 1fr; gap: 40px; } .feat.rev .feat-text { order: 0; } .band-dark { margin: 0 10px; border-radius: 26px; } }

.mini-head { display: flex; justify-content: space-between; align-items: center; font-size: 14px; margin-bottom: 6px; }
.mini-head b { font-family: 'Clash Display', 'Outfit', sans-serif; font-weight: 500; font-size: 15.5px; }
.mini-bar { height: 12px; border-radius: 7px; background: rgba(246, 242, 233, 0.1); margin-bottom: 16px; overflow: hidden; }
.mini-bar i { display: block; height: 100%; border-radius: 7px; background: linear-gradient(90deg, var(--emerald), var(--mint)); width: 0; transition: width 1.3s cubic-bezier(.22, 1, .36, 1); }
.mini-bar.gold i { background: linear-gradient(90deg, var(--brass), var(--brass-soft)); }

.odg-row { display: flex; gap: 14px; align-items: flex-start; padding: 14px 4px; border-top: 1px solid var(--line-dark); }
.odg-row:first-of-type { border-top: none; }
.odg-num { font-family: 'Clash Display', 'Outfit', sans-serif; font-style: normal; color: var(--brass-soft); font-size: 15px; flex-shrink: 0; width: 22px; }
.odg-row b { font-weight: 500; font-size: 15px; display: block; }
.odg-row span { font-size: 13px; color: rgba(246, 242, 233, 0.55); }

/* ============ PRICING ============ */
.price-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 26px; margin-top: 64px; align-items: stretch; }
.price-card { background: #fff; border: 1px solid var(--line-light); border-radius: 26px; padding: 38px 32px; display: flex; flex-direction: column; position: relative; transition: transform .3s, box-shadow .3s; }
.price-card:hover { transform: translateY(-6px); box-shadow: 0 35px 70px -30px rgba(27, 74, 53, 0.35); }
.price-card.hero-plan { background: var(--night); color: var(--cream); border: 1px solid rgba(201, 165, 92, 0.4); transform: scale(1.03); box-shadow: 0 45px 90px -35px rgba(12, 31, 23, 0.6); }
.price-card.hero-plan:hover { transform: scale(1.03) translateY(-6px); }
.plan-badge {
  position: absolute; top: -14px; left: 50%; transform: translateX(-50%);
  font-size: 11.5px; font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase;
  background: linear-gradient(120deg, var(--brass-soft), var(--brass)); color: var(--night);
  padding: 6px 18px; border-radius: 999px; box-shadow: 0 8px 20px -6px rgba(201, 165, 92, 0.6); white-space: nowrap;
}
.price-name { font-family: 'Clash Display', 'Outfit', sans-serif; font-size: 22px; font-weight: 500; margin-bottom: 4px; }
.price-for { font-size: 14px; color: var(--ink-soft); font-weight: 300; min-height: 42px; }
.hero-plan .price-for { color: rgba(246, 242, 233, 0.65); }
.price-tag { font-family: 'Clash Display', 'Outfit', sans-serif; font-size: 52px; font-weight: 400; margin: 18px 0 2px; line-height: 1; }
.price-tag sup { font-size: 22px; vertical-align: super; }
.price-tag span { font-family: 'Outfit', sans-serif; font-size: 15px; font-weight: 300; color: var(--ink-soft); }
.hero-plan .price-tag span { color: rgba(246, 242, 233, 0.6); }
.price-feats { list-style: none; display: flex; flex-direction: column; gap: 13px; font-size: 15px; margin: 26px 0 32px; font-weight: 300; }
.price-feats li { display: flex; gap: 11px; align-items: flex-start; }
.price-feats svg { flex-shrink: 0; margin-top: 3px; color: var(--emerald); }
.hero-plan .price-feats svg { color: var(--mint); }
.price-card .btn { width: 100%; justify-content: center; margin-top: auto; }
.btn-dark { background: var(--night); color: var(--cream); }
.btn-dark:hover { transform: translateY(-2px); }
.price-note { text-align: center; font-size: 14px; color: var(--ink-soft); margin-top: 36px; font-weight: 300; }
@media(max-width: 920px) { .price-grid { grid-template-columns: 1fr; } .price-card.hero-plan { transform: none; } .price-card.hero-plan:hover { transform: translateY(-6px); } }

/* numbers band */
.numbers { display: grid; grid-template-columns: repeat(4, 1fr); gap: 30px; margin-top: 70px; }
.num-item { text-align: center; position: relative; padding: 24px 10px; }
.num-item:not(:last-child)::after { content: ""; position: absolute; right: -15px; top: 25%; bottom: 25%; width: 1px; background: var(--line-light); }
.num-val { font-family: 'Clash Display', 'Outfit', sans-serif; font-size: clamp(40px, 5vw, 58px); font-weight: 400; color: var(--pine); line-height: 1; }
.num-val .gold { color: var(--brass); }
.num-lbl { font-size: 14.5px; color: var(--ink-soft); margin-top: 12px; font-weight: 300; }
@media(max-width: 760px) { .numbers { grid-template-columns: 1fr 1fr; } .num-item::after { display: none; } }

/* ============ FINAL CTA ============ */
.final { background: var(--night); color: var(--cream); border-radius: 36px; margin: 0 20px 26px; position: relative; overflow: hidden; text-align: center; padding: 110px 32px; }
.final h2 { font-size: clamp(36px, 5vw, 60px); margin-bottom: 22px; font-weight: 400; position: relative; z-index: 2; }
.final h2 em { font-style: normal; color: var(--mint); }
.final p { font-size: 18px; color: rgba(246, 242, 233, 0.72); max-width: 540px; margin: 0 auto 40px; font-weight: 300; position: relative; z-index: 2; }
.final .btn { position: relative; z-index: 2; font-size: 16.5px; padding: 16px 36px; }
.final .hero-note { justify-content: center; position: relative; z-index: 2; }
@media(max-width: 920px) { .final { margin: 0 10px 16px; border-radius: 26px; } }

.dw-landing footer { background: var(--night); color: rgba(246, 242, 233, 0.6); padding: 56px 0 40px; font-size: 14px; }
.foot-in { display: flex; justify-content: space-between; align-items: center; gap: 26px; flex-wrap: wrap; }
.foot-links { display: flex; gap: 28px; }
.foot-links a { transition: color .2s; }
.foot-links a:hover { color: var(--cream); }

/* reveal */
.reveal { opacity: 0; transform: translateY(30px); transition: opacity .8s cubic-bezier(.22, 1, .36, 1), transform .8s cubic-bezier(.22, 1, .36, 1); }
.reveal.in { opacity: 1; transform: none; }
.reveal-d1 { transition-delay: .1s; } .reveal-d2 { transition-delay: .2s; } .reveal-d3 { transition-delay: .3s; }
</style>
