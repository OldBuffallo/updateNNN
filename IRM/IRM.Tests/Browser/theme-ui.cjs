// Acceptance against a real, disposable IRM + SQL Server fixture. Playwright is test-only.
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const { chromium } = require(process.env.IRM_PLAYWRIGHT_PATH || 'playwright');

async function main() {
    const base = new URL(process.env.IRM_UI_BASE_URL || 'http://127.0.0.1:15050');
    assert.ok(['127.0.0.1', 'localhost'].includes(base.hostname), 'Use a loopback disposable fixture only.');
    assert.equal(process.env.IRM_UI_ALLOW_WRITES, 'isolated-fixture', 'Explicit disposable-fixture opt-in required.');
    const username = process.env.IRM_UI_USERNAME;
    const password = process.env.IRM_UI_PASSWORD;
    const workbook = process.env.IRM_UI_WORKBOOK;
    assert.ok(username && password && workbook, 'Provide fixture account and workbook through environment variables.');
    const evidence = path.resolve(process.env.IRM_UI_EVIDENCE_DIR || '.theme-evidence');
    fs.mkdirSync(evidence, { recursive: true });
    const report = { base: base.origin, cases: [], measurements: [], pageErrors: [] };
    const browser = await chromium.launch({ channel: 'msedge', headless: true });
    const context = await browser.newContext({ viewport: { width: 1366, height: 768 } });
    const page = await context.newPage();
    page.on('pageerror', error => report.pageErrors.push(error.message));

    async function ready(target = page) {
        await target.locator('.irm-theme-loading').waitFor({ state: 'hidden' });
        await target.waitForFunction(() => ['light', 'dark'].includes(document.documentElement.getAttribute('data-theme')));
    }
    async function mode(value, target = page) {
        if (await target.locator('html').getAttribute('data-theme') !== value) await target.locator('.theme-toggle-btn').click();
        await target.waitForFunction(value => document.documentElement.getAttribute('data-theme') === value, value);
        await target.waitForTimeout(400); // Wait for existing theme color transitions, not for data behavior.
    }
    async function capture(name) {
        await page.mouse.move(1, 1);
        await page.screenshot({ path: path.join(evidence, name + '.png'), fullPage: true, animations: 'disabled' });
    }
    async function contrast(selector, name, minimum = 4.5, target = page) {
        const values = await target.locator(selector).evaluateAll((elements, minimum) => {
            const rgba = value => {
                const n = value.match(/[\d.]+/g)?.map(Number);
                return n?.length >= 3 ? [n[0], n[1], n[2], n[3] ?? 1] : [0, 0, 0, 0];
            };
            const luminance = rgb => rgb.slice(0, 3).map(n => n / 255).map(n => n <= .04045 ? n / 12.92 : ((n + .055) / 1.055) ** 2.4)
                .reduce((sum, n, i) => sum + n * [.2126, .7152, .0722][i], 0);
            const over = (front, back) => front.slice(0, 3).map((n, i) => n * front[3] + back[i] * (1 - front[3]));
            return elements.filter(el => el.getClientRects().length && !el.closest(':disabled,[aria-disabled="true"]')).map(el => {
                const ancestors = [];
                for (let node = el; node; node = node.parentElement) ancestors.unshift(node);
                if (ancestors.some(node => getComputedStyle(node).backgroundImage !== 'none')) return { skipped: 'gradient' };
                const bg = ancestors.reduce((back, node) => over(rgba(getComputedStyle(node).backgroundColor), back), [255, 255, 255]);
                const fg = over(rgba(getComputedStyle(el).color), bg);
                const a = luminance(fg), b = luminance(bg);
                return { ratio: (Math.max(a, b) + .05) / (Math.min(a, b) + .05), minimum, color: getComputedStyle(el).color,
                    background: bg.map(Math.round), element: el.tagName, className: el.className?.toString().slice(0, 100) };
            });
        }, minimum);
        report.measurements.push({ name, values });
        const failures = values.filter(x => !x.skipped && x.ratio < minimum);
        assert.deepEqual(failures, [], `${name}: insufficient effective contrast`);
    }

    try {
        await page.goto(new URL('/login', base).href);
        await page.locator('#loginUser').waitFor();
        await ready();
        await capture('login-light');
        await page.fill('#loginUser', username);
        await page.fill('#loginPass', password);
        await Promise.all([page.waitForURL(url => !url.pathname.endsWith('/login')), page.getByRole('button', { name: 'Đăng nhập', exact: true }).click()]);
        await page.goto(new URL('/import', base).href);
        await page.locator('input[type=file]').waitFor({ state: 'attached' });
        await ready();
        assert.equal(await page.getByRole('button', { name: 'Tiếp tục', exact: true }).isDisabled(), true);
        report.cases.push('upload remains disabled before choosing a workbook');
        await page.locator('input[type=file]').setInputFiles(workbook);
        await page.getByRole('button', { name: 'Tiếp tục', exact: true }).click();
        await page.getByText('Cột Excel', { exact: true }).waitFor();
        assert.equal(await page.locator('.mud-simple-table tbody tr').count(), 19);
        assert.ok(await page.getByText('⚠️ Bỏ qua', { exact: true }).count());

        for (const value of ['light', 'dark']) {
            await mode(value);
            await contrast('.mud-simple-table tbody td strong, .mud-simple-table .mud-input-slot, .mud-chip-content', `mapping ${value}`);
            await contrast('.mud-button-root:not(:disabled) .mud-button-label', `mapping buttons ${value}`);
            await capture('mapping-' + value);
            await page.locator('.mud-select').first().click();
            await page.locator('.mud-popover-open').last().waitFor();
            await contrast('.mud-popover-open .mud-list-item-text', `mapping dropdown ${value}`);
            await page.keyboard.press('Escape');
            report.cases.push(`mapping rows, skip row, buttons and open dropdown: ${value}`);
        }
        await page.getByRole('button', { name: 'Tiếp tục', exact: true }).click();
        await page.getByRole('button', { name: 'Bắt đầu Import', exact: true }).waitFor();
        for (const value of ['light', 'dark']) {
            await mode(value);
            await contrast('.mud-button-root:not(:disabled) .mud-button-label', `preview buttons ${value}`);
        }
        await page.getByRole('button', { name: 'Bắt đầu Import', exact: true }).click();
        await page.locator('.mud-progress-circular:visible').first().waitFor({ timeout: 5000 });
        await capture('import-loading');
        report.cases.push('real import loading spinner visible with slow disposable SQL fixture');
        await page.getByText('Import hoàn tất!', { exact: true }).waitFor({ timeout: 30000 });
        report.cases.push('real import completed in isolated fixture');
        for (const value of ['light', 'dark']) {
            await mode(value);
            await contrast('.mud-button-root:not(:disabled) .mud-button-label', `result buttons ${value}`);
        }

        const routes = ['/', '/companies', '/company-profiles', '/employees', '/foreign-persons', '/family-visitors',
            '/students', '/accommodations', '/inspections', '/reports', '/search', '/statistics', '/admin', '/import'];
        for (const value of ['light', 'dark']) {
            for (const route of routes) {
                await page.goto(new URL(route, base).href);
                await page.locator('.theme-toggle-btn').waitFor();
                await ready(); await mode(value);
                await contrast('.mud-main-content .mud-button-root:not(:disabled) .mud-button-label', `${route} buttons ${value}`);
                assert.equal(await page.locator('#blazor-error-ui:visible').count(), 0);
                report.cases.push(`${route}: ${value}`);
            }
            await page.reload(); await page.locator('.theme-toggle-btn').waitFor(); await ready();
            assert.equal(await page.locator('html').getAttribute('data-theme'), value);
            const tab = await context.newPage();
            await tab.goto(new URL('/import', base).href); await tab.locator('.theme-toggle-btn').waitFor(); await ready(tab);
            assert.equal(await tab.locator('html').getAttribute('data-theme'), value);
            await tab.close();
            await page.bringToFront();
            await page.locator('.user-pill').click();
            await page.locator('.mud-popover-open .logout-menu-item').waitFor();
            await contrast('.mud-popover-open .mud-menu-item, .mud-popover-open .logout-menu-item', `user menu ${value}`);
            await capture('account-menu-' + value);
            await page.keyboard.press('Escape');
            await page.locator('.user-pill').focus();
            await page.keyboard.press('Enter');
            await page.locator('.mud-popover-open .logout-menu-item').waitFor();
            await page.keyboard.press('Escape');
            await page.goto(new URL('/companies', base).href);
            await page.getByRole('button', { name: '+ Thêm công ty', exact: true }).click();
            await page.getByText('➕ Thêm công ty mới', { exact: true }).waitFor();
            await contrast('.mud-dialog .mud-button-label, .mud-dialog .mud-input-label', `dialog ${value}`);
            await capture('company-dialog-' + value);
            await page.getByRole('button', { name: 'Hủy', exact: true }).click();
            report.cases.push(`reload, new tab, user menu, company dialog: ${value}`);
        }
        await page.goto(new URL('/import', base).href);
        await page.locator('.theme-toggle-btn').waitFor();
        const initial = await page.locator('html').getAttribute('data-theme');
        for (let i = 0; i < 4; i++) await page.locator('.theme-toggle-btn').click();
        await page.waitForTimeout(400);
        assert.equal(await page.locator('html').getAttribute('data-theme'), initial);
        await page.locator('.irm-nav-link[href="/companies"]').click();
        await page.waitForURL('**/companies');
        assert.equal(await page.locator('html').getAttribute('data-theme'), initial);
        report.cases.push('rapid toggles and navigation preserve the final mode');

        await page.setViewportSize({ width: 1920, height: 1080 });
        await page.goto(new URL('/import', base).href); await page.locator('.theme-toggle-btn').waitFor();
        report.cases.push('desktop 1920x1080 loaded');
        await page.setViewportSize({ width: 390, height: 844 });
        await page.reload(); await page.locator('.theme-toggle-btn').waitFor();
        report.cases.push('narrow 390px horizontal overflow: ' + await page.evaluate(() => document.documentElement.scrollWidth > innerWidth));

        await page.setViewportSize({ width: 1366, height: 768 });
        await page.goto(new URL('/import', base).href); await page.locator('.theme-toggle-btn').waitFor();
        await mode('dark');
        await page.locator('.user-pill').click();
        await page.getByRole('button', { name: 'Đăng xuất', exact: true }).click();
        await page.locator('#loginUser').waitFor(); await ready();
        assert.equal(await page.locator('html').getAttribute('data-theme'), 'dark');
        await capture('login-dark');
        report.cases.push('logout/login page retains dark preference');

        if (process.env.IRM_UI_EDITOR_USERNAME && process.env.IRM_UI_EDITOR_PASSWORD) {
            const editorContext = await browser.newContext();
            const editor = await editorContext.newPage();
            await editor.goto(new URL('/login', base).href);
            await editor.locator('#loginUser').waitFor();
            await editor.fill('#loginUser', process.env.IRM_UI_EDITOR_USERNAME);
            await editor.fill('#loginPass', process.env.IRM_UI_EDITOR_PASSWORD);
            await Promise.all([editor.waitForURL(url => !url.pathname.endsWith('/login')), editor.getByRole('button', { name: 'Đăng nhập', exact: true }).click()]);
            await editor.goto(new URL('/import', base).href);
            await editor.locator('.theme-toggle-btn').waitFor();
            assert.equal(await editor.locator('.irm-nav-link[href="/admin"]').count(), 0);
            for (const value of ['light', 'dark']) {
                await mode(value, editor);
                await contrast('.mud-button-root:not(:disabled) .mud-button-label', `editor controls ${value}`, 4.5, editor);
            }
            await editorContext.close();
            report.cases.push('DATA_EDITOR login/import controls in both modes; admin navigation stays hidden');
        }

        const blocked = await browser.newContext();
        await blocked.addInitScript(() => {
            Object.defineProperty(window, 'localStorage', { get() { throw new Error('Blocked storage fixture'); } });
        });
        const fallback = await blocked.newPage();
        await fallback.goto(new URL('/login', base).href); await fallback.locator('#loginUser').waitFor(); await ready(fallback);
        assert.equal(await fallback.locator('html').getAttribute('data-theme'), 'light');
        await blocked.close();
        report.cases.push('blocked storage renders usable light login');
        assert.deepEqual(report.pageErrors, []);
        report.success = true;
        console.log(`PASS: ${report.cases.length} browser acceptance cases; ${report.measurements.length} contrast groups.`);
    } catch (error) {
        report.success = false;
        report.failure = error.message;
        await capture('failure').catch(() => {});
        throw error;
    } finally {
        fs.writeFileSync(path.join(evidence, 'browser-results.json'), JSON.stringify(report, null, 2));
        await browser.close();
    }
}

main().catch(error => { console.error(error.message); process.exitCode = 1; });
