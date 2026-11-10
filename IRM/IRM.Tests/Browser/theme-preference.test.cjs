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
