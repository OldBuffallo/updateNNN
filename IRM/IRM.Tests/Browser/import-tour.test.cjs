const test = require('node:test');
const assert = require('node:assert/strict');
const { browser } = require('./import-tour-fixture.cjs');
const fs = require('node:fs');
const path = require('node:path');
const state = (stage = 1, extra = {}) => ({ stage, isBusy: false, isManual: false, contextVersion: 0, ...extra });

test('dropdown portal layer stays above the pinned Driver popover', () => {
    const root = path.join(__dirname, '../../IRM/wwwroot');
    const vendor = fs.readFileSync(path.join(root, 'vendor/driverjs/1.12.1/driver.css'), 'utf8');
    const css = fs.readFileSync(path.join(root, 'css/import-guidance.css'), 'utf8');
    const popover = Number(vendor.match(/\.driver-popover\{[^}]*z-index:(\d+)/)?.[1]);
    const portalRule = css.match(/\.driver-active \.mud-popover-open\.driver-allowed-element\s*\{([^}]+)\}/)?.[1] ?? '';
    const portal = Number(portalRule.match(/--driver-allowed-z-index:\s*(\d+)/)?.[1] ?? 0);
    assert.ok(portal > popover, `MudBlazor portal layer ${portal} must exceed Driver popover ${popover}`);
    assert.match(portalRule, /(?:^|;)\s*z-index:\s*var\(--driver-allowed-z-index\)\s*!important/, 'Portal must override the inline z-index set by MudBlazor');
});

test('start uses current chapter without application clicks or external effects', () => {
    const b = browser();
    assert.equal(b.api.start(b.rootId, state(3)), true);
    assert.equal(b.activeHint(), 'preview-summary');
    for (let i = 0; i < 8; i++) b.advanceHint();
    assert.equal(b.activeHint(), 'execute-import');
    assert.ok(b.driverConfigs.at(-1).steps.at(-1).popover.instruction);
    assert.equal(b.clickCount + b.networkCount + b.storageCount, 0);
});
test('rollback guidance requires unchanged data and no newer dependency', () => {
    const b = browser(); b.api.start(b.rootId, state(4));
    const description = b.driverConfigs.at(-1).steps.at(-1).popover.description;
    assert.match(description, /chưa bị chỉnh sửa và chưa có phiên\/phụ thuộc mới hơn/);
    assert.doesNotMatch(description, /hoặc có phiên/);
});
for (const reason of ['close', 'escape']) test(`${reason} survives later sync without restarting`, () => {
    const b = browser(); b.api.start(b.rootId, state(2)); b.userClose(reason);
    const starts = b.driverConfigs.length;
    b.api.sync(b.rootId, state(3));
    assert.equal(b.driverConfigs.length, starts);
    assert.equal(b.activeHint(), null);
    assert.equal(b.resourceCount, 0);
});
test('sync never starts a tour that was not requested', () => {
    const b = browser(); b.api.sync(b.rootId, state(1)); assert.equal(b.driverConfigs.length, 0);
});
test('same render preserves the active hint and stage changes follow app state', () => {
    const b = browser(); b.api.start(b.rootId, state(1)); b.advanceHint();
    b.api.sync(b.rootId, state(1)); assert.equal(b.activeHint(), 'company');
    assert.equal(b.driverConfigs.length, 1);
    b.api.sync(b.rootId, state(2)); assert.equal(b.activeHint(), 'field-lookup');
    b.api.sync(b.rootId, state(1)); assert.equal(b.activeHint(), 'upload');
});
test('busy pauses then resumes at the same hint when request fails without changing stage', () => {
    const b = browser(); b.api.start(b.rootId, state(2)); b.advanceHint();
    b.api.sync(b.rootId, state(2, { isBusy: true })); assert.equal(b.activeHint(), null);
    b.api.sync(b.rootId, state(2)); assert.equal(b.activeHint(), 'mapping');
    b.api.sync(b.rootId, state(2, { isBusy: true }));
    b.api.sync(b.rootId, state(3)); assert.equal(b.activeHint(), 'preview-summary');
});
for (const extra of [{ isManual: true }, { contextVersion: 1 }]) test(`context stop ${JSON.stringify(extra)} does not reopen`, () => {
    const b = browser(); b.api.start(b.rootId, state());
    b.api.sync(b.rootId, state(1, extra)); b.api.sync(b.rootId, state());
    assert.equal(b.activeHint(), null); assert.equal(b.resourceCount, 0);
});
test('required target can render late, or times out at 1500 ms with a text notice', () => {
    const b = browser(); b.removeTarget('mapping'); b.api.start(b.rootId, state(2));
    assert.equal(b.activeHint(), null); b.tick(1499); assert.equal(b.activeHint(), null);
    b.addTarget('mapping'); assert.equal(b.activeHint(), 'field-lookup');
    b.api.dispose(b.rootId); b.removeTarget('mapping'); b.api.start(b.rootId, state(2)); b.tick(1500);
    assert.equal(b.activeHint(), null); assert.match(b.targets.get('notice').textContent, /Bạn có thể mở lại/);
    assert.equal(b.resourceCount, 0);
});
test('dispose cancels waits and stale callbacks and is idempotent', () => {
    const b = browser(); b.removeTarget('mapping'); b.api.start(b.rootId, state(2));
    b.api.dispose(b.rootId); b.api.dispose(b.rootId); b.addTarget('mapping'); b.tick(2000);
    assert.equal(b.driverConfigs.length, 0); assert.equal(b.resourceCount, 0);
    b.api.start(b.rootId, state()); b.detach(); assert.equal(b.activeHint(), null); assert.equal(b.resourceCount, 0);
});
test('optional result errors may be absent and library absence does not touch import', () => {
    const b = browser(); b.removeTarget('result-errors'); b.api.start(b.rootId, state(4));
    assert.equal(b.driverConfigs.at(-1).steps.length, 2);
    const unavailable = browser({ library: false });
    assert.equal(unavailable.api.start(unavailable.rootId, state()), false);
    assert.equal(unavailable.clickCount + unavailable.networkCount + unavailable.storageCount, 0);
});
test('dropdown portals and navigation controls stay usable; reduced motion is respected', () => {
    const b = browser({ reducedMotion: true }); b.api.start(b.rootId, state());
    const config = b.driverConfigs.at(-1), allowed = config.allowedElements();
    assert.ok(allowed.includes(b.portal)); assert.ok(allowed.includes(b.controls[0]));
    assert.equal(config.animate, false); assert.equal(config.overlayClickBehavior, 'none');
});
test('active target loss stops safely instead of showing an orphaned popover', () => {
    const b = browser(); b.api.start(b.rootId, state(2)); b.loseActiveTarget();
    assert.equal(b.activeHint(), null); assert.match(b.targets.get('notice').textContent, /Bạn có thể mở lại/);
    assert.equal(b.resourceCount, 0);
});
