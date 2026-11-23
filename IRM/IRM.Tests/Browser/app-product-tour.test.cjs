const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const scriptPath = path.resolve(__dirname, '../../IRM/wwwroot/js/app-product-tour.js');

function createHarness({ isAdmin = true, seen = null, storageThrows = false, hasDriver = true } = {}) {
  const presentSelectors = new Set([
    '[data-app-tour="nav-dashboard"]',
    '[data-app-tour="nav-foreign-persons"]',
    '[data-app-tour="nav-companies"]',
    '[data-app-tour="nav-accommodations"]',
    '[data-app-tour="nav-inspections"]',
    '[data-app-tour="nav-search"]',
    '[data-app-tour="nav-reports"]',
    '[data-app-tour="nav-statistics"]',
    '[data-app-tour="nav-import"]',
    '#btnAppUserGuide'
  ]);
  if (isAdmin) presentSelectors.add('[data-app-tour="admin-system"]');

  const storage = new Map();
  if (seen !== null) storage.set('irm-product-tour-v1-seen', seen);

  let drivenConfig = null;
  let driveCount = 0;

  const doc = {
    querySelector(sel) {
      return presentSelectors.has(sel)
        ? { selector: sel, scrollIntoView() {} }
        : null;
    }
  };

  const localStorage = {
    getItem(key) {
      if (storageThrows) throw new Error('blocked');
      return storage.has(key) ? storage.get(key) : null;
    },
    setItem(key, value) {
      if (storageThrows) throw new Error('blocked');
      storage.set(key, String(value));
    }
  };

  const sandbox = {
    window: {},
    document: doc,
    localStorage,
    setTimeout(fn) { fn(); }
  };
  sandbox.window = sandbox;
  if (hasDriver) {
    sandbox.window.driver = {
      js: {
        driver(config) {
          drivenConfig = config;
          return {
            drive() { driveCount++; },
            destroy() {}
          };
        }
      }
    };
  }

  const code = fs.readFileSync(scriptPath, 'utf8');
  vm.runInNewContext(code, sandbox);
  return {
    api: sandbox.window.irmAppTour,
    getConfig: () => drivenConfig,
    getDriveCount: () => driveCount,
    storage
  };
}

test('admin user gets 11 steps starting with admin-system followed by exact sidebar order', () => {
  const { api } = createHarness({ isAdmin: true });
  const steps = api.buildSteps();
  assert.equal(steps.length, 11);
  assert.deepEqual(Array.from(steps, s => s.element), [
    '[data-app-tour="admin-system"]',
    '[data-app-tour="nav-dashboard"]',
    '[data-app-tour="nav-foreign-persons"]',
    '[data-app-tour="nav-companies"]',
    '[data-app-tour="nav-accommodations"]',
    '[data-app-tour="nav-inspections"]',
    '[data-app-tour="nav-search"]',
    '[data-app-tour="nav-reports"]',
    '[data-app-tour="nav-statistics"]',
    '[data-app-tour="nav-import"]',
    '#btnAppUserGuide'
  ]);
  assert.match(steps[2].popover.description, /Số hộ chiếu/i);
  assert.match(steps[9].popover.description, /Số hộ chiếu/i);
});

test('non-admin user skips step 1 (admin-system) and starts at nav-dashboard with 10 steps', () => {
  const { api } = createHarness({ isAdmin: false });
  const steps = api.buildSteps();
  assert.equal(steps.length, 10);
  assert.equal(steps[0].element, '[data-app-tour="nav-dashboard"]');
  assert.ok(!steps.some(s => s.element === '[data-app-tour="admin-system"]'));
});

test('init runs once on first visit, marks seen on destroy, skips repeat unless forceReplay is true', () => {
  const h = createHarness({ isAdmin: true, seen: null });
  assert.equal(h.api.init(false), true);
  assert.equal(h.getDriveCount(), 1);

  // Simulate user finishing/closing tour
  h.getConfig().onDestroyed();
  assert.equal(h.storage.get('irm-product-tour-v1-seen'), '1');

  // Second visit without forceReplay skips
  assert.equal(h.api.init(false), false);
  assert.equal(h.getDriveCount(), 1);

  // Replay with forceReplay = true runs again
  assert.equal(h.api.init(true), true);
  assert.equal(h.getDriveCount(), 2);
});

test('missing driver.js or blocked localStorage never throws', () => {
  const noDriver = createHarness({ hasDriver: false });
  assert.doesNotThrow(() => noDriver.api.init(false));
  assert.equal(noDriver.api.init(false), false);

  const blockedStorage = createHarness({ storageThrows: true });
  assert.doesNotThrow(() => blockedStorage.api.init(false));
  assert.equal(blockedStorage.getDriveCount(), 1);
});
