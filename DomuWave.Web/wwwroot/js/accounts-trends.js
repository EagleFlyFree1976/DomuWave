(() => {
    const accountSelect = document.getElementById('accountSelect');
    const monthsSelect = document.getElementById('monthsSelect');
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const balanceCtx = document.getElementById('balanceChart').getContext('2d');
    const flowCtx = document.getElementById('flowChart').getContext('2d');

    let accounts = [];
    let currentIndex = 0;
    let balanceChart, flowChart;

    async function fetchData(accountId, months) {
        const url = `/api/reports/accounts-trends?accountId=${accountId ?? ''}&months=${months}`;
        const res = await fetch(url, { credentials: 'same-origin' });
        if (!res.ok) throw new Error('Server error ' + res.status);
        return await res.json();
    }

    function renderCharts(payload) {
        const balanceSeries = payload.series.find(s => s.key === 'balance');
        const inflowSeries = payload.series.find(s => s.key === 'inflow');
        const outflowSeries = payload.series.find(s => s.key === 'outflow');

        const labels = balanceSeries.data.map(p => p.x);

        if (balanceChart) balanceChart.destroy();
        if (flowChart) flowChart.destroy();

        balanceChart = new Chart(balanceCtx, {
            type: 'line',
            data: {
                labels,
                datasets: [{
                    label: 'Saldo',
                    data: balanceSeries.data.map(p => p.y),
                    borderColor: 'rgba(33,150,243,1)',
                    backgroundColor: 'rgba(33,150,243,0.12)',
                    tension: 0.25,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: true } },
                scales: { x: { display: true }, y: { display: true } }
            }
        });

        flowChart = new Chart(flowCtx, {
            type: 'bar',
            data: {
                labels,
                datasets: [
                    {
                        label: 'Entrate',
                        data: inflowSeries.data.map(p => p.y),
                        backgroundColor: 'rgba(40,167,69,0.85)'
                    },
                    {
                        label: 'Uscite',
                        data: outflowSeries.data.map(p => p.y),
                        backgroundColor: 'rgba(220,53,69,0.85)'
                    }
                ]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: true } },
                scales: { x: { stacked: true }, y: { stacked: false } }
            }
        });
    }

    function populateAccounts(list, selectedId) {
        accounts = list;
        accountSelect.innerHTML = '';
        list.forEach((a, idx) => {
            const opt = document.createElement('option');
            opt.value = a.id;
            opt.text = a.name;
            accountSelect.appendChild(opt);
            if (a.id === selectedId) currentIndex = idx;
        });
        accountSelect.selectedIndex = currentIndex;
    }

    async function reload() {
        const months = parseInt(monthsSelect.value, 10) || 12;
        const accountId = accounts[currentIndex]?.id;
        try {
            const payload = await fetchData(accountId, months);
            populateAccounts(payload.accounts, payload.selectedAccountId);
            renderCharts(payload);
        } catch (err) {
            console.error(err);
            alert('Errore durante il caricamento dei dati');
        }
    }

    accountSelect.addEventListener('change', async () => {
        currentIndex = accountSelect.selectedIndex;
        await reload();
    });

    monthsSelect.addEventListener('change', async () => await reload());

    prevBtn.addEventListener('click', async () => {
        if (currentIndex > 0) currentIndex--;
        accountSelect.selectedIndex = currentIndex;
        await reload();
    });

    nextBtn.addEventListener('click', async () => {
        if (currentIndex < accounts.length - 1) currentIndex++;
        accountSelect.selectedIndex = currentIndex;
        await reload();
    });

    // initial load
    (async () => {
        try {
            const payload = await fetchData(null, parseInt(monthsSelect.value, 10));
            populateAccounts(payload.accounts, payload.selectedAccountId);
            renderCharts(payload);
        } catch (err) {
            console.error(err);
            accountSelect.innerHTML = '<option>Nessun account disponibile</option>';
        }
    })();
})();