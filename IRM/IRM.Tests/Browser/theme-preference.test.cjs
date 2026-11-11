const test = require('node:test');
const assert = require('node:assert/strict');
const { browser } = require('./theme-fixture.cjs');

test('restoring saved dark synchronizes the CSS mode before controls are exposed', () => {
    const b = browser({ saved: 'dark' });
    assert.equal(b.api.getTheme(), 'dark');
    assert.equal(b.attributes.get('data-theme'), 'dark');
});

for (const saved of [null, 'light', 'unexpected', 'DARK', '']) {
    test(`missing or invalid preference ${JSON.stringify(saved)} uses usable light mode`, () => {
        const b = browser({ saved });
        assert.equal(b.api.getTheme(), 'light');
        assert.equal(b.attributes.get('data-theme'), 'light');
    });
}

test('blocked preference reads fall back to light without breaking initialization', () => {
    const b = browser({ readError: true });
    assert.equal(b.api.getTheme(), 'light');
    assert.equal(b.attributes.get('data-theme'), 'light');
});

test('blocked preference writes still apply the selected mode to the current page', () => {
    const b = browser({ writeError: true });
    assert.doesNotThrow(() => b.api.setTheme('dark'));
    assert.equal(b.attributes.get('data-theme'), 'dark');
});

test('setting an invalid theme normalizes both the current page and stored preference', () => {
    const b = browser();
    b.api.setTheme('unexpected');
    assert.equal(b.attributes.get('data-theme'), 'light');
    assert.equal(b.storage.get('irm-theme'), 'light');
});

test('app.css uses government navy tokens without legacy purple radial gradients or transparent gradient titles', () => {
    const fs = require('node:fs');
    const path = require('node:path');
    const css = fs.readFileSync(path.join(__dirname, '..', '..', 'IRM', 'wwwroot', 'app.css'), 'utf8');
    assert.equal(/140,\s*87,\s*255|110,\s*57,\s*203/.test(css), false, 'app.css should not contain hardcoded Materio purple RGB');
    assert.equal(css.includes('-webkit-text-fill-color: transparent'), false, 'search hero title should use solid readable text');
});

test('App.razor rotates static asset cache token beyond legacy theme-20261110', () => {
    const fs = require('node:fs');
    const path = require('node:path');
    const appRazor = fs.readFileSync(path.join(__dirname, '..', '..', 'IRM', 'Components', 'App.razor'), 'utf8');
    assert.equal(appRazor.includes('v=theme-20261110'), false, 'App.razor must not serve stale theme-20261110 cache token');
    assert.equal(appRazor.includes('app.css?v=mui-gov-20261111'), true, 'App.razor must use updated mui-gov-20261111 cache token');
});

test('MainLayout docks responsive sidebar from Md 960px upward for split-screen consistency', () => {
    const fs = require('node:fs');
    const path = require('node:path');
    const layout = fs.readFileSync(path.join(__dirname, '..', '..', 'IRM', 'Components', 'Shared', 'MainLayout.razor'), 'utf8');
    assert.equal(layout.includes('Breakpoint="Breakpoint.Md"'), true, 'MainLayout MudDrawer should dock from Breakpoint.Md upward');
});
