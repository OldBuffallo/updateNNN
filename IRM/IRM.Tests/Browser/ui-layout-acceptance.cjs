// Live browser layout & responsive UI acceptance against an isolated disposable IRM + SQL Server fixture.
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
    const evidence = path.resolve(process.env.IRM_UI_EVIDENCE_DIR || '.ui-layout-evidence');
    fs.mkdirSync(evidence, { recursive: true });

    const consoleErrors = [];
    const pageErrors = [];
    const browser = await chromium.launch({ channel: 'msedge', headless: true });
    const context = await browser.newContext({ viewport: { width: 1366, height: 768 } });
    const page = await context.newPage();
    page.on('console', msg => { if (msg.type() === 'error') consoleErrors.push(`${page.url()}: ${msg.text()}`); });
    page.on('pageerror', err => pageErrors.push(`${page.url()}: ${err.message}`));

    async function ready() {
        await page.locator('.irm-theme-loading').waitFor({ state: 'hidden', timeout: 10000 }).catch(() => {});
        await page.waitForFunction(() => ['light', 'dark'].includes(document.documentElement.getAttribute('data-theme')));
    }

    try {
        // 1. Login and import fixture workbook
        await page.goto(new URL('/login', base).href);
        await page.locator('#loginUser').waitFor();
        await ready();
        await page.fill('#loginUser', username);
        await page.fill('#loginPass', password);
        await Promise.all([
            page.waitForURL(url => !url.pathname.endsWith('/login')),
            page.getByRole('button', { name: 'Đăng nhập', exact: true }).click()
        ]);

        await page.goto(new URL('/import', base).href);
        await page.locator('input[type=file]').waitFor({ state: 'attached' });
        await ready();
        await page.locator('input[type=file]').setInputFiles(workbook);
        await page.getByRole('button', { name: 'Tiếp tục', exact: true }).click();
        await page.getByText('Cột Excel', { exact: true }).waitFor();
        await page.getByRole('button', { name: 'Tiếp tục', exact: true }).click();
        await page.getByRole('button', { name: 'Bắt đầu Import', exact: true }).waitFor();
        await page.getByRole('button', { name: 'Bắt đầu Import', exact: true }).click();
        await page.getByText('Import hoàn tất!', { exact: true }).waitFor({ timeout: 30000 });

        // 2. Desktop 1366x768 checks: Action column & Expiry column visible, Pager value visible, Section headers stacked
        await page.setViewportSize({ width: 1366, height: 768 });
        for (const route of ['/foreign-persons', '/companies', '/accommodations', '/inspections']) {
            await page.goto(new URL(route, base).href);
            await page.locator('.irm-data-table').first().waitFor();
            await ready();
            await page.waitForTimeout(300);

            const check1366 = await page.evaluate(() => {
                const headerDiv = document.querySelector('.section-header h3')?.parentElement;
                const h3 = headerDiv?.querySelector('h3');
                const meta = headerDiv?.querySelector('.section-meta');
                const headerStacked = (!h3 || !meta) ? true : (meta.getBoundingClientRect().top >= h3.getBoundingClientRect().bottom - 2);

                const tbl = document.querySelector('.irm-data-table');
                const container = tbl?.querySelector('.mud-table-container') || tbl;
                const cRect = container.getBoundingClientRect();
                const ths = Array.from(tbl.querySelectorAll('thead th'));
                const actionIdx = ths.findIndex(th => th.textContent.includes('Thao tác'));
                const actionTh = actionIdx >= 0 ? ths[actionIdx] : null;
                const prevTh = actionIdx > 0 ? ths[actionIdx - 1] : null;
                const thRect = actionTh ? actionTh.getBoundingClientRect() : null;
                const prevThRect = prevTh ? prevTh.getBoundingClientRect() : null;
                const actionVisible = thRect ? (thRect.right <= window.innerWidth + 1 && thRect.right <= cRect.right + 2 && thRect.left >= cRect.left) : false;
                const noStickyOverlap = (thRect && prevThRect) ? (prevThRect.right <= thRect.left + 2) : true;
                const maxFilterInputHeight = Array.from(document.querySelectorAll('.page-actions-bar .mud-input-control, .page-filter-card .mud-input-control'))
                    .reduce((m, el) => Math.max(m, el.getBoundingClientRect().height), 0);

                return {
                    headerStacked,
                    actionVisible,
                    noStickyOverlap,
                    prevThRight: prevThRect?.right,
                    thLeft: thRect?.left,
                    thRight: thRect?.right,
                    containerRight: cRect.right,
                    maxFilterInputHeight,
                };
            });

            assert.equal(check1366.headerStacked, true, `${route} at 1366px: .section-meta must stack cleanly below h3`);
            assert.equal(check1366.actionVisible, true, `${route} at 1366px: Thao tác column must be visible within container/viewport (got thRight=${check1366.thRight}, containerRight=${check1366.containerRight})`);
            assert.equal(check1366.noStickyOverlap, true, `${route} at 1366px: Sticky Thao tác column must not overlap adjacent data column (prevThRight=${check1366.prevThRight}, thLeft=${check1366.thLeft})`);
            assert.ok(check1366.maxFilterInputHeight <= 56, `${route} at 1366px: Filter inputs must stay normal single-line height (<=56px), got ${check1366.maxFilterInputHeight}px`);
        }

        // ForeignPersons pagination select displays "15"
        await page.goto(new URL('/foreign-persons', base).href);
        await page.locator('.irm-data-table').first().waitFor();
        await page.waitForTimeout(300);
        const pagerValue = await page.evaluate(() => {
            const pager = document.querySelector('.mud-data-grid-pager, .mud-table-pagination');
            const inputSlot = pager?.querySelector('.mud-select .mud-input-slot, .mud-select input');
            return inputSlot ? (inputSlot.textContent || inputSlot.value || '').trim() : '';
        });
        assert.equal(pagerValue, '15', `ForeignPersons pager select must visibly display '15', got '${pagerValue}'`);

        // Reports 1366x768 preview cells do not wrap into >2-line vertical towers
        await page.goto(new URL('/reports', base).href);
        await page.locator('.report-builder-preview .mud-simple-table').waitFor();
        await page.waitForTimeout(300);
        const maxReportRowHeight = await page.evaluate(() => {
            const rows = Array.from(document.querySelectorAll('.report-builder-preview .mud-simple-table tbody tr:not(.group-header)'));
            return rows.reduce((max, tr) => Math.max(max, tr.getBoundingClientRect().height), 0);
        });
        assert.ok(maxReportRowHeight > 0 && maxReportRowHeight <= 64, `Reports preview rows at 1366px must stay compact (<=64px), got ${maxReportRowHeight}px`);

        // 3. Split-screen 1024px checks: ForeignPersons rows stay <=68px (no 5-line name tower) & Statistics non-action last column not sticky-right
        await page.setViewportSize({ width: 1024, height: 768 });
        await page.goto(new URL('/foreign-persons', base).href);
        await page.locator('.irm-data-table').first().waitFor();
        await page.waitForTimeout(300);
        const maxFpRow1024 = await page.evaluate(() => {
            return Array.from(document.querySelectorAll('.irm-data-table tbody tr'))
                .reduce((m, tr) => Math.max(m, tr.getBoundingClientRect().height), 0);
        });
        assert.ok(maxFpRow1024 > 0 && maxFpRow1024 <= 68, `ForeignPersons rows at 1024px must stay compact (<=68px), got ${maxFpRow1024}px`);

        await page.goto(new URL('/statistics', base).href);
        await page.getByRole('button', { name: 'Tổng quan 54 địa bàn' }).click();
        await page.locator('.irm-data-table').first().waitFor();
        await page.waitForTimeout(300);
        const statsLastSticky = await page.evaluate(() => {
            const ths = Array.from(document.querySelectorAll('.irm-data-table thead th'));
            const tds = Array.from(document.querySelectorAll('.irm-data-table tbody tr:first-child td'));
            const lastTh = ths[ths.length - 1];
            const lastTd = tds[tds.length - 1];
            return {
                thRight: lastTh ? getComputedStyle(lastTh).right : null,
                tdPosition: lastTd ? getComputedStyle(lastTd).position : null,
            };
        });
        assert.equal(statsLastSticky.thRight, 'auto', 'Statistics non-action last header must not be sticky-right (right: 0px) at 1024px');
        assert.notEqual(statsLastSticky.tdPosition, 'sticky', 'Statistics non-action last body cell must not become sticky at 1024px');

        await page.setViewportSize({ width: 390, height: 844 });
        await page.waitForTimeout(300);
        const statsMobileLastLabel = await page.evaluate(() => {
            const tds = Array.from(document.querySelectorAll('.irm-data-table tbody tr:first-child td'));
            const lastTd = tds[tds.length - 1];
            return lastTd ? getComputedStyle(lastTd, '::before').content : '';
        });
        assert.ok(statsMobileLastLabel.includes('Tổng người nước ngoài'), `Statistics mobile card last cell must retain its ::before label, got ${statsMobileLastLabel}`);

        // 4. Mobile 390x844 checks on Companies, ForeignPersons, Inspections
        await page.goto(new URL('/companies', base).href);
        await page.locator('.irm-data-table').first().waitFor();
        await page.waitForTimeout(300);
        const companiesMobile = await page.evaluate(() => {
            const bar = document.querySelector('.page-actions-bar');
            const barRect = bar.getBoundingClientRect();
            const controls = Array.from(bar.querySelectorAll('.mud-input-control, button, a.mud-button-root'));
            const overflowing = controls.filter(el => el.getBoundingClientRect().right > barRect.right + 2);
            const tallButtons = Array.from(bar.querySelectorAll('button, a.mud-button-root')).filter(el => el.getBoundingClientRect().height > 48);
            const tallInputs = Array.from(bar.querySelectorAll('.mud-input-control')).filter(el => el.getBoundingClientRect().height > 56);
            const hasMudXsTable = Boolean(document.querySelector('.irm-data-table .mud-xs-table, .irm-data-table.mud-xs-table'));
            const docOverflow = document.documentElement.scrollWidth > window.innerWidth + 1;
            return { overflowingCount: overflowing.length, tallButtonCount: tallButtons.length, tallInputCount: tallInputs.length, hasMudXsTable, docOverflow };
        });
        assert.equal(companiesMobile.overflowingCount, 0, 'Companies 390px: filter inputs/buttons must not overflow .page-actions-bar');
        assert.equal(companiesMobile.tallButtonCount, 0, 'Companies 390px: action buttons must wrap instead of squishing vertically >48px');
        assert.equal(companiesMobile.tallInputCount, 0, 'Companies 390px: filter inputs must stay single-line height <=56px');
        assert.equal(companiesMobile.hasMudXsTable, false, 'Companies 390px: .irm-data-table must not activate conflicting .mud-xs-table');
        assert.equal(companiesMobile.docOverflow, false, 'Companies 390px: document must not overflow horizontally');

        await page.goto(new URL('/reports', base).href);
        await page.locator('.report-builder-preview').waitFor();
        await page.waitForTimeout(300);
        const reportsMobile = await page.evaluate(() => {
            const previewRect = document.querySelector('.report-builder-preview').getBoundingClientRect();
            return {
                docOverflow: document.documentElement.scrollWidth > window.innerWidth + 1,
                previewRight: previewRect.right,
                innerWidth: window.innerWidth,
            };
        });
        assert.equal(reportsMobile.docOverflow, false, 'Reports 390px: wide preview table must scroll internally without causing page horizontal overflow');
        assert.ok(reportsMobile.previewRight <= reportsMobile.innerWidth + 1, `Reports 390px: .report-builder-preview right edge (${reportsMobile.previewRight}) must fit within 390px viewport`);

        await page.goto(new URL('/inspections', base).href);
        await page.locator('.irm-data-table').first().waitFor();
        await page.waitForTimeout(300);
        const inspectionsMobileSplit = await page.evaluate(() => {
            const cells = Array.from(document.querySelectorAll('.irm-data-table tbody tr:first-child td:not(.col-actions)'));
            return cells.filter(td => td.children.length > 1).map(td => td.getAttribute('data-label'));
        });
        assert.deepEqual(inspectionsMobileSplit, [], 'Inspections 390px: non-action card cells must wrap multi-part values in a single container');

        assert.deepEqual(consoleErrors, [], `Expected zero browser console errors, got: ${consoleErrors.join('; ')}`);
        assert.deepEqual(pageErrors, [], `Expected zero page errors, got: ${pageErrors.join('; ')}`);

        console.log('PASS: Live UI layout & responsive acceptance passed across 1366px, 1024px, and 390px with 0 console/page errors.');
    } finally {
        await browser.close();
    }
}

main().catch(err => { console.error(err.message || err); process.exitCode = 1; });
