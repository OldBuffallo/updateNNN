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

test('app.css targets MudNavLink inner anchor without double padding, offsets 62px topbar, and keeps table actions inline', () => {
    const fs = require('node:fs');
    const path = require('node:path');
    const css = fs.readFileSync(path.join(__dirname, '..', '..', 'IRM', 'wwwroot', 'app.css'), 'utf8');
    assert.equal(css.includes('.irm-nav-link .mud-nav-link.active'), true, 'app.css must style MudNavLink inner active anchor');
    assert.equal(css.includes('--mud-appbar-height: 62px'), true, 'app.css must sync --mud-appbar-height with 62px topbar');
    assert.equal(css.includes('.mud-data-grid .mud-table-cell:last-child'), true, 'app.css must keep table action buttons on a single horizontal row');
});

test('NavMenu MudNavLink items do not set OnClick alongside Href so MudBlazor renders real anchor links', () => {
    const fs = require('node:fs');
    const path = require('node:path');
    const navMenu = fs.readFileSync(path.join(__dirname, '..', '..', 'IRM', 'Components', 'Shared', 'NavMenu.razor'), 'utf8');
    assert.equal(navMenu.includes('NotifyPageChanged'), false, 'MudNavLink must not bind OnClick NotifyPageChanged which suppresses Href anchor rendering in MudBlazor');
});

test('irmInterop supports table density toggle (comfortable vs compact) with storage persistence', () => {
    const b = browser();
    assert.equal(b.api.getDensity(), 'comfortable');
    assert.equal(b.attributes.get('data-density'), 'comfortable');
    b.api.setDensity('compact');
    assert.equal(b.attributes.get('data-density'), 'compact');
    assert.equal(b.storage.get('irm-density'), 'compact');
    b.api.setDensity('invalid');
    assert.equal(b.attributes.get('data-density'), 'comfortable');
});

test('app.css defines Enterprise Structured Grid classes and replaces hardcoded light row borders', () => {
    const fs = require('node:fs');
    const path = require('node:path');
    const css = fs.readFileSync(path.join(__dirname, '..', '..', 'IRM', 'wwwroot', 'app.css'), 'utf8');
    assert.equal(css.includes('rgba(226, 232, 240, 0.85)'), false, 'app.css must not use hardcoded light border on table cells');
    assert.equal(css.includes('.cell-stack'), true, 'app.css must define .cell-stack for two-line structured cells');
    assert.equal(css.includes('.code-pill'), true, 'app.css must define .code-pill for monospace identifiers');
    assert.equal(css.includes('.status-pill'), true, 'app.css must define .status-pill for expiry/violation badges');
    assert.equal(css.includes('.tabular-nums'), true, 'app.css must define .tabular-nums for numeric alignment');
    assert.equal(css.includes('[data-density="compact"]'), true, 'app.css must support compact table density mode');
    assert.equal(css.includes('.table-avatar.bg-blue-soft'), true, 'app.css must preserve soft background contrast on table avatars');
    assert.equal(css.includes('.row-accent-danger'), true, 'app.css must define .row-accent-danger for urgent/violation rows');
    assert.equal(css.includes('.mud-simple-table > .mud-table-container'), true, 'app.css must allow sticky headers inside MudSimpleTable');
    assert.equal(css.includes('@media (max-width: 768px)'), true, 'app.css must define responsive mobile card layout for tables');
});
