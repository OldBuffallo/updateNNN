const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const irmWebRoot = path.join(__dirname, '..', '..', 'IRM');

function createDomTableContext({ readyState = 'complete', headerTexts = [], hasSortIconText = false } = {}) {
    let setAttrCalls = 0;
    const tds = headerTexts.map(() => {
        const attrs = new Map();
        return {
            getAttribute: k => attrs.get(k) ?? null,
            setAttribute: (k, v) => { setAttrCalls++; attrs.set(k, v); },
        };
    });
    const ths = headerTexts.map(label => ({
        textContent: hasSortIconText ? `${label} arrow_upward` : label,
        querySelector: sel => sel === '.mud-column-header-text' ? { textContent: label } : null,
        cloneNode: () => ({
            textContent: label,
            querySelectorAll: () => [],
        }),
    }));
    const row = {
        querySelectorAll: sel => sel === 'td' ? tds : [],
    };
    const table = {
        querySelectorAll: sel => {
            if (sel === 'thead th') return ths;
            if (sel === 'tbody tr:not(.group-header)') return [row];
            return [];
        },
    };
    let domContentLoadedHandler = null;
    const context = {
        window: {},
        document: {
            readyState,
            documentElement: { setAttribute() {}, getAttribute() { return 'light'; } },
            body: {},
            addEventListener(evt, fn) {
                if (evt === 'DOMContentLoaded') domContentLoadedHandler = fn;
            },
            querySelectorAll: sel => sel.includes('.mud-table-root') ? [table] : [],
        },
        localStorage: { getItem: () => null, setItem: () => {} },
        getComputedStyle: () => ({ getPropertyValue: () => '' }),
        requestAnimationFrame: fn => fn(0),
        MutationObserver: function () { return { observe() {} }; },
        Chart: { defaults: {} },
        console,
    };
    context.window.getComputedStyle = context.getComputedStyle;
    context.window.requestAnimationFrame = context.requestAnimationFrame;
    vm.createContext(context);
    vm.runInContext(fs.readFileSync(path.join(irmWebRoot, 'wwwroot', 'irm-interop.js'), 'utf8'), context);
    return {
        api: context.window.irmInterop,
        tds,
        ths,
        getSetAttrCalls: () => setAttrCalls,
        resetSetAttrCalls: () => { setAttrCalls = 0; },
        hasDomContentLoadedHandler: () => domContentLoadedHandler !== null,
    };
}

test('irm-interop initializes table labels immediately even when loaded after DOMContentLoaded', () => {
    const dom = createDomTableContext({
        readyState: 'complete',
        headerTexts: ['Người nước ngoài', 'Hộ chiếu', 'Thao tác'],
        hasSortIconText: true,
    });
    assert.deepEqual(
        dom.tds.map(td => td.getAttribute('data-label')),
        ['Người nước ngoài', 'Hộ chiếu', 'Thao tác'],
        'syncTableLabels must run immediately when document.readyState is already interactive/complete'
    );
});

test('syncTableLabels updates stale data-label when Blazor reuses td nodes across tab switches without redundant writes', () => {
    const dom = createDomTableContext({
        readyState: 'complete',
        headerTexts: ['Mã địa bàn', 'Tên xã / phường'],
    });
    assert.deepEqual(dom.tds.map(td => td.getAttribute('data-label')), ['Mã địa bàn', 'Tên xã / phường']);

    dom.resetSetAttrCalls();
    dom.api.syncTableLabels();
    assert.equal(dom.getSetAttrCalls(), 0, 'syncTableLabels must not rewrite identical data-label attributes');

    // Simulate Blazor switching tabs and updating header text while reusing existing <td> DOM elements
    dom.ths[0].querySelector = () => ({ textContent: 'Tên khu kinh tế' });
    dom.ths[1].querySelector = () => ({ textContent: 'Số doanh nghiệp' });
    dom.api.syncTableLabels();
    assert.deepEqual(
        dom.tds.map(td => td.getAttribute('data-label')),
        ['Tên khu kinh tế', 'Số doanh nghiệp'],
        'syncTableLabels must refresh stale data-label attributes when table headers change'
    );
});

test('App.razor does not request missing IRM.styles.css bundle when no scoped razor.css files exist', () => {
    const appRazor = fs.readFileSync(path.join(irmWebRoot, 'Components', 'App.razor'), 'utf8');
    assert.equal(
        appRazor.includes('href="IRM.styles.css"'),
        false,
        'App.razor must not request non-existent IRM.styles.css which causes 404 console errors on every page'
    );
});

test('app.css keeps section-header h3 block-level so h3 and section-meta never collide inline', () => {
    const css = fs.readFileSync(path.join(irmWebRoot, 'wwwroot', 'app.css'), 'utf8');
    assert.equal(
        /\.section-header\s+h3\s*\{[^}]*display:\s*inline-flex/s.test(css),
        false,
        '.section-header h3 must use block-level flex, never inline-flex which glues h3 onto sibling .section-meta'
    );
});

test('app.css scopes sticky right column and mobile footer to explicit .col-actions instead of all :last-child data columns', () => {
    const css = fs.readFileSync(path.join(irmWebRoot, 'wwwroot', 'app.css'), 'utf8');
    assert.equal(
        css.includes('.irm-data-table .mud-table-body .mud-table-cell:last-child::before'),
        false,
        'Mobile card CSS must not strip ::before labels from :last-child on non-action tables like Statistics.razor'
    );
    assert.equal(
        /\.irm-data-table\s+\.mud-table-body\s+\.mud-table-cell:last-child\s*\{\s*position:\s*sticky/s.test(css),
        false,
        'Sticky right column must target .col-actions, not :last-child on read-only tables'
    );
    assert.equal(
        css.includes('.col-actions'),
        true,
        'app.css must define explicit .col-actions rules for sticky desktop actions and mobile card footer'
    );
});

test('ForeignPersons and Employees MudDataGridPager include 15 in PageSizeOptions when RowsPerPage="15"', () => {
    for (const rel of ['Features/ForeignPersons/ForeignPersons.razor', 'Features/Employees/Employees.razor']) {
        const razor = fs.readFileSync(path.join(irmWebRoot, rel), 'utf8');
        if (razor.includes('RowsPerPage="15"')) {
            assert.equal(
                /MudDataGridPager[^>]*PageSizeOptions=.*15/s.test(razor),
                true,
                `${rel} uses RowsPerPage="15" so MudDataGridPager must include 15 in PageSizeOptions to avoid a blank select`
            );
        }
    }
});

test('All .irm-data-table MudDataGrid components disable MudBlazor Breakpoint.Xs clash and mark Thao tác columns with col-actions', () => {
    const featureFiles = [
        'Features/ForeignPersons/ForeignPersons.razor',
        'Features/Employees/Employees.razor',
        'Features/Students/Students.razor',
        'Features/ForeignPersons/FamilyVisitors.razor',
        'Features/Companies/Companies.razor',
        'Features/Companies/CompanyProfiles.razor',
        'Features/Accommodations/Accommodations.razor',
        'Features/Inspections/Inspections.razor',
        'Features/Reports/Statistics.razor',
    ];
    for (const rel of featureFiles) {
        const razor = fs.readFileSync(path.join(irmWebRoot, rel), 'utf8');
        const grids = razor.match(/<MudDataGrid[\s\S]*?>/g) || [];
        for (const tag of grids) {
            if (tag.includes('irm-data-table')) {
                assert.equal(
                    tag.includes('Breakpoint="Breakpoint.None"'),
                    true,
                    `${rel} .irm-data-table MudDataGrid must set Breakpoint="Breakpoint.None" so .mud-xs-table does not clash at <600px`
                );
            }
        }
        const actionCols = razor.match(/<TemplateColumn[^>]*Title="Thao tác"[^>]*>/g) || [];
        for (const col of actionCols) {
            assert.equal(
                col.includes('col-actions'),
                true,
                `${rel} TemplateColumn Title="Thao tác" must carry col-actions class`
            );
        }
    }
});

test('Companies, Inspections, and Reports avoid narrow-viewport flex overflow and multi-element mobile cell splits', () => {
    const companies = fs.readFileSync(path.join(irmWebRoot, 'Features/Companies/Companies.razor'), 'utf8');
    assert.equal(
        companies.includes('Wrap="Wrap.Wrap"'),
        true,
        'Companies.razor .page-actions-bar MudStack must enable Wrap="Wrap.Wrap" to prevent 390px overflow'
    );

    const inspections = fs.readFileSync(path.join(irmWebRoot, 'Features/Inspections/Inspections.razor'), 'utf8');
    const timeColMatch = inspections.match(/<TemplateColumn[^>]*Title="Thời gian"[\s\S]*?<\/TemplateColumn>/);
    assert.ok(timeColMatch, 'Inspections.razor must contain Thời gian column');
    assert.equal(
        timeColMatch[0].includes('cell-inline'),
        true,
        'Inspections.razor Thời gian cell must wrap date and time inside a single .cell-inline container for mobile cards'
    );

    const reports = fs.readFileSync(path.join(irmWebRoot, 'Features/Reports/Reports.razor'), 'utf8');
    assert.equal(
        reports.includes('<MudItem xs="3">'),
        false,
        'Reports.razor summary cards must use responsive xs="6" sm="3" instead of cramped xs="3" on mobile'
    );
});

test('MudTextField and MudSelect never set flex-basis in inline Style which inflates column-flex input height to 200px', () => {
    const companies = fs.readFileSync(path.join(irmWebRoot, 'Features/Companies/Companies.razor'), 'utf8');
    assert.equal(
        /Style="[^"]*flex\s*:/i.test(companies),
        false,
        'Companies.razor must not put flex shorthand on MudTextField/MudSelect Style (inflates vertical flex-basis to 200px)'
    );
});

test('app.css constrains report-builder grid children with min-width: 0 and minmax(0, 1fr) so mobile 390px never clips right edge', () => {
    const css = fs.readFileSync(path.join(irmWebRoot, 'wwwroot', 'app.css'), 'utf8');
    assert.equal(
        /\.report-builder-sidebar,\s*\.report-builder-preview\s*\{[^}]*min-width:\s*0/s.test(css),
        true,
        '.report-builder-sidebar and .report-builder-preview must set min-width: 0 so wide preview tables scroll internally instead of stretching the page'
    );
    assert.equal(
        /\.report-builder-grid\s*\{\s*grid-template-columns:\s*minmax\(0,\s*1fr\)/s.test(css),
        true,
        '@media (max-width: 1200px) .report-builder-grid must use minmax(0, 1fr)'
    );
});

test('ForeignPersons 1366px laptop budget keeps Diện single-line and cell widths compact so sticky Thao tác never overlaps Hạn tạm trú', () => {
    const fp = fs.readFileSync(path.join(irmWebRoot, 'Features/ForeignPersons/ForeignPersons.razor'), 'utf8');
    const dienCol = fp.match(/<TemplateColumn[^>]*Title="Diện"[\s\S]*?<\/TemplateColumn>/);
    assert.ok(dienCol, 'ForeignPersons.razor must define Title="Diện" column');
    assert.equal(
        dienCol[0].includes('cell-truncate'),
        true,
        'ForeignPersons.razor Diện cell must use cell-truncate so multi-purpose labels stay single-line on 1366px laptops'
    );
});

test('app.css keeps .cell-inline .cell-primary single-line nowrap even when .cell-inline wraps secondary metadata at <=1180px', () => {
    const css = fs.readFileSync(path.join(irmWebRoot, 'wwwroot', 'app.css'), 'utf8');
    assert.equal(
        /\.cell-inline\s+\.cell-primary\s*\{[^}]*white-space:\s*nowrap/s.test(css),
        true,
        '.cell-inline .cell-primary must explicitly set white-space: nowrap so 1024px split-screen rows stay <=2 lines'
    );
});
