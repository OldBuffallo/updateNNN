const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

function browser({ library = true, reducedMotion = false } = {}) {
    const names = ['prepare-download', 'prepare-fields', 'prepare-next', 'upload', 'company', 'career-group', 'read-options', 'create-company', 'go-mapping',
        'field-lookup', 'mapping', 'go-preview', 'preview-summary', 'preview-table', 'execute-import',
        'result-summary', 'result-errors', 'result-actions', 'notice'];
    let clickCount = 0, networkCount = 0, storageCount = 0, now = 0, timerId = 0, current = null;
    const targets = new Map(names.map(name => [name, { name, textContent: '', click() { clickCount++; } }]));
    const controls = [{ name: 'manual-toggle' }], portal = { name: 'open-mud-popover' };
    const root = {
        isConnected: true,
        querySelector(selector) { return targets.get(selector.match(/data-import-tour="([^"]+)"/)?.[1]) ?? null; },
        querySelectorAll(selector) { return selector === '[data-import-tour-control]' ? controls : []; }
    };
    const observers = new Set(), timers = new Map(), driverConfigs = [];
    class Observer {
        constructor(callback) { this.callback = callback; }
        observe() { observers.add(this); }
        disconnect() { observers.delete(this); }
    }
    const document = {
        body: {}, documentElement: {},
        getElementById(id) { return id === 'test-root' && root.isConnected ? root : null; },
        querySelectorAll(selector) { return selector === '.mud-popover-open' ? [portal] : []; }
    };
    function driver(config) {
        driverConfigs.push(config);
        const item = {
            index: 0, active: false,
            drive(index = 0) {
                item.index = index; item.active = true; current = item;
                config.onHighlighted?.(config.steps[index].element(), config.steps[index], { index, driver: item });
            },
            destroy() { item.active = false; if (current === item) current = null; config.onDestroyed?.(); },
            getActiveIndex() { return item.index; },
            getActiveStep() { return config.steps[item.index]; },
            refresh() {},
            moveNext() { if (!config.steps[item.index].popover.instruction && item.index + 1 < config.steps.length) item.drive(item.index + 1); },
            movePrevious() { if (item.index > 0) item.drive(item.index - 1); }
        };
        return item;
    }
    const window = { driver: library ? { js: { driver } } : undefined, matchMedia: () => ({ matches: reducedMotion }) };
    const context = {
        window, document, MutationObserver: Observer, console,
        setTimeout(callback, ms) { const id = ++timerId; timers.set(id, { callback, due: now + ms }); return id; },
        clearTimeout(id) { timers.delete(id); },
        fetch() { networkCount++; throw new Error('Tour must not use network'); },
        localStorage: { getItem() { storageCount++; }, setItem() { storageCount++; } }
    };
    vm.createContext(context);
    vm.runInContext(fs.readFileSync(path.join(__dirname, '../../IRM/wwwroot/js/import-tour.js'), 'utf8'), context);
    return {
        api: window.irmImportTour, rootId: 'test-root', driverConfigs, targets, root, controls, portal,
        get clickCount() { return clickCount; }, get networkCount() { return networkCount; }, get storageCount() { return storageCount; },
        get resourceCount() { return observers.size + timers.size; },
        activeHint() { return current?.getActiveStep()?.element()?.name ?? null; },
        userClose() { current?.destroy(); }, advanceHint() { current?.moveNext(); },
        removeTarget(name) { targets.delete(name); for (const o of [...observers]) o.callback(); },
        addTarget(name) { targets.set(name, { name, textContent: '' }); for (const o of [...observers]) o.callback(); },
        detach() { root.isConnected = false; for (const o of [...observers]) o.callback(); },
        loseActiveTarget() { driverConfigs.at(-1)?.onElementMissing?.(null, null, { cause: 'removed' }); },
        tick(ms) { now += ms; for (const [id, task] of [...timers]) if (task.due <= now) { timers.delete(id); task.callback(); } }
    };
}
module.exports = { browser };
