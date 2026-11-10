const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

function browser({ saved = null, readError = false, writeError = false, colors = {} } = {}) {
    const attributes = new Map();
    const storage = new Map(saved == null ? [] : [['irm-theme', saved]]);
    const frames = [];
    const root = {
        setAttribute: (key, value) => attributes.set(key, value),
        getAttribute: key => attributes.get(key) ?? null,
    };
    const computed = { getPropertyValue: key => colors[key] ?? '' };
    const context = {
        window: {},
        document: { documentElement: root, body: root, addEventListener() {} },
        localStorage: {
            getItem(key) { if (readError) throw new Error('Storage unavailable'); return storage.get(key) ?? null; },
            setItem(key, value) { if (writeError) throw new Error('Storage unavailable'); storage.set(key, value); },
        },
        getComputedStyle: () => computed,
        requestAnimationFrame: callback => frames.push(callback),
        Chart: { defaults: {} },
        console,
    };
    context.window.getComputedStyle = context.getComputedStyle;
    context.window.requestAnimationFrame = context.requestAnimationFrame;
    vm.createContext(context);
    vm.runInContext(fs.readFileSync(path.join(__dirname, '../../IRM/wwwroot/irm-interop.js'), 'utf8'), context);
    return {
        api: context.window.irmInterop, attributes, storage, frames,
        frame() { const callbacks = frames.splice(0); for (const callback of callbacks) callback(0); },
    };
}

module.exports = { browser };
