const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

function createFeedbackEnv({ seen = null, reducedMotion = false } = {}) {
    const storage = new Map(seen == null ? [] : [['irm-r6-feedback-seen', seen]]);
    const classes = new Set();
    let clickHandler = null;
    const btn = {
        id: 'btnCustomerFeedback',
        classList: {
            add: (...names) => names.forEach(n => classes.add(n)),
            remove: (...names) => names.forEach(n => classes.delete(n)),
            contains: name => classes.has(name),
        },
        addEventListener: (event, fn) => {
            if (event === 'click') clickHandler = fn;
        },
        click: () => {
            if (clickHandler) clickHandler();
        }
    };

    const driverCalls = [];
    let lastDriverConfig = null;
    const driverFactory = (config) => {
        lastDriverConfig = config;
        return {
            drive: (step = 0) => driverCalls.push({ action: 'drive', step, config }),
            destroy: () => {
                driverCalls.push({ action: 'destroy' });
                if (config && typeof config.onDestroyStarted === 'function') {
                    config.onDestroyStarted();
                }
            }
        };
    };

    const root = {
        setAttribute() {},
        getAttribute() { return null; }
    };

    const context = {
        window: {
            matchMedia: () => ({ matches: reducedMotion }),
            driver: { js: { driver: driverFactory } }
        },
        document: {
            documentElement: root,
            body: root,
            addEventListener() {},
            getElementById: (id) => id === 'btnCustomerFeedback' ? btn : null,
            querySelector: (sel) => sel === '#btnCustomerFeedback' ? btn : null
        },
        localStorage: {
            getItem: (k) => storage.get(k) ?? null,
            setItem: (k, v) => storage.set(k, String(v)),
        },
        getComputedStyle: () => ({ getPropertyValue: () => '' }),
        requestAnimationFrame: (cb) => cb(0),
        setTimeout: (cb) => { cb(); return 1; },
        clearTimeout: () => {},
        Chart: { defaults: {} },
        console,
    };
    context.window.getComputedStyle = context.getComputedStyle;
    context.window.requestAnimationFrame = context.requestAnimationFrame;
    context.window.localStorage = context.localStorage;
    context.window.document = context.document;

    vm.createContext(context);
    const scriptPath = path.join(__dirname, '../../IRM/wwwroot/irm-interop.js');
    vm.runInContext(fs.readFileSync(scriptPath, 'utf8'), context);

    return {
        api: context.window.irmInterop,
        storage,
        classes,
        btn,
        driverCalls,
        getLastDriverConfig: () => lastDriverConfig
    };
}

test('initFeedbackSpotlight activates highlight and Driver.js spotlight on first visit', () => {
    const env = createFeedbackEnv();
    assert.equal(typeof env.api.initFeedbackSpotlight, 'function');
    const started = env.api.initFeedbackSpotlight(false);
    assert.equal(started, true);
    assert.equal(env.classes.has('irm-feedback-highlight'), true);
    assert.equal(env.driverCalls.length > 0, true);
    assert.equal(env.driverCalls[0].action, 'drive');
});

test('dismissFeedbackSpotlight marks seen in localStorage and removes highlight', () => {
    const env = createFeedbackEnv();
    env.api.initFeedbackSpotlight(false);
    env.api.dismissFeedbackSpotlight();
    assert.equal(env.storage.get('irm-r6-feedback-seen'), '1');
    assert.equal(env.classes.has('irm-feedback-highlight'), false);
});

test('initFeedbackSpotlight skips when already seen unless forceReplay is true', () => {
    const env = createFeedbackEnv({ seen: '1' });
    assert.equal(env.api.initFeedbackSpotlight(false), false);
    assert.equal(env.classes.has('irm-feedback-highlight'), false);

    assert.equal(env.api.initFeedbackSpotlight(true), true);
    assert.equal(env.classes.has('irm-feedback-highlight'), true);
});
