const test = require('node:test');
const assert = require('node:assert/strict');
const { browser } = require('./theme-fixture.cjs');

const colors = {
    '--text-secondary': '#cbd5e1', '--border': '#475569', '--bg-surface': '#0f172a',
    '--chart-1': '#60a5fa', '--chart-2': '#a5b4fc', '--chart-3': '#f9a8d4',
    '--chart-4': '#86efac', '--chart-5': '#fcd34d',
};

function chart(type) {
    return {
        config: { type }, data: { datasets: [{ data: [2, 3], backgroundColor: ['old-a', 'old-b'], borderColor: 'old' }] },
        options: { plugins: { legend: { labels: { color: 'old' } } }, scales: { x: { ticks: {} }, y: { ticks: {}, grid: { color: 'old' } } } },
        updates: 0, update() { this.updates++; },
    };
}

test('chart labels and grid follow the computed theme instead of a separate hardcoded palette', () => {
    const b = browser({ colors });
    const bar = chart('bar');
    b.api._charts.bar = bar;
    b.api.refreshCharts();
    assert.equal(bar.options.scales.y.grid.color, '#475569');
    assert.equal(bar.options.scales.y.ticks.color, '#cbd5e1');
    assert.equal(bar.options.scales.x.ticks.color, '#cbd5e1');
    assert.equal(bar.options.plugins.legend.labels.color, '#cbd5e1');
    assert.equal(bar.data.datasets[0].backgroundColor[0], '#60a5fa');
    assert.deepEqual(bar.data.datasets[0].data, [2, 3]);
});

test('rendered palette is read after the frame even when preference storage is blocked', async () => {
    const b = browser({ colors, writeError: true });
    const doughnut = chart('doughnut');
    b.api._charts.permits = doughnut;
    const applying = b.api.applyTheme('dark');
    assert.equal(doughnut.updates, 0);
    b.frame();
    await applying;
    assert.equal(b.attributes.get('data-theme'), 'dark');
    assert.equal(doughnut.data.datasets[0].borderColor, '#0f172a');
    assert.equal(doughnut.data.datasets[0].backgroundColor[0], '#86efac');
    assert.equal(doughnut.updates, 1);
});

test('rapid theme changes leave charts using the last effective rendered palette', async () => {
    const active = { ...colors };
    const b = browser({ colors: active });
    const line = chart('line');
    b.api._charts.trend = line;
    const dark = b.api.applyTheme('dark');
    const light = b.api.applyTheme('light');
    active['--text-secondary'] = '#475569';
    active['--bg-surface'] = '#ffffff';
    b.frame();
    await Promise.all([dark, light]);
    assert.equal(b.attributes.get('data-theme'), 'light');
    assert.equal(line.options.plugins.legend.labels.color, '#475569');
    assert.equal(line.data.datasets[0].borderColor, '#fcd34d');
    assert.deepEqual(line.data.datasets[0].data, [2, 3]);
});
