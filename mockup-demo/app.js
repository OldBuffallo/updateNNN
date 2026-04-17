/* ============================================ */
/*  Immigration Report Manager v2.0 — Mockup    */
/*  Interactive JS + Dark Mode + Transitions     */
/* ============================================ */

// ============ THEME MANAGEMENT ============
function initTheme() {
    const saved = localStorage.getItem('irm-theme');
    if (saved) {
        document.documentElement.setAttribute('data-theme', saved);
    } else {
        // Default to light
        document.documentElement.setAttribute('data-theme', 'light');
    }
}
initTheme();

function toggleTheme() {
    const html = document.documentElement;
    const current = html.getAttribute('data-theme');
    const next = current === 'dark' ? 'light' : 'dark';
    html.setAttribute('data-theme', next);
    localStorage.setItem('irm-theme', next);

    // Re-init charts with new theme colors
    if (charts.nationality || charts.permit || charts.trend) {
        destroyCharts();
        setTimeout(initCharts, 100);
    }
}

// ============ LOADING SCREEN ============
window.addEventListener('load', function() {
    setTimeout(function() {
        const loader = document.getElementById('loadingScreen');
        if (loader) {
            loader.classList.add('fade-out');
            setTimeout(() => loader.remove(), 500);
        }
    }, 600);
});

// ============ HELP CONTENT DATA ============
const helpData = {
    dashboard: {
        title: '📊 Dashboard — Tổng quan',
        sections: [
            {
                title: '🔢 Thẻ KPI (phía trên)',
                content: 'Hiển thị 4 chỉ số chính: Tổng công ty, Tổng NLĐ, Sắp hết hạn, Đã có GPLĐ. Dữ liệu tự động cập nhật khi có thay đổi.'
            },
            {
                title: '📊 Biểu đồ',
                items: [
                    '<strong>Biểu đồ cột:</strong> NLĐ phân bổ theo quốc tịch (Top 10). Click vào cột để xem chi tiết.',
                    '<strong>Biểu đồ tròn:</strong> Tỷ lệ GPLĐ — nhanh chóng thấy bao nhiêu % đã có giấy phép.',
                    '<strong>Biểu đồ đường:</strong> Xu hướng hết hạn tạm trú trong 12 tháng tới.'
                ]
            },
            {
                title: '⚠️ Bảng cảnh báo',
                content: 'Danh sách NLĐ sắp hết hạn tạm trú, sắp xếp theo mức độ khẩn cấp. Màu <strong style="color:var(--red)">đỏ</strong> = dưới 7 ngày, <strong style="color:var(--orange)">cam</strong> = 7-20 ngày, <strong style="color:var(--blue)">xanh</strong> = 20-30 ngày.',
                tip: 'Nhấn "Xem tất cả" để mở danh sách đầy đủ có thể xuất Excel.'
            }
        ]
    },
    companies: {
        title: '🏢 Quản lý Công ty',
        sections: [
            {
                title: '📋 Danh sách công ty',
                content: 'Hiển thị toàn bộ công ty đang hoạt động. Mỗi dòng gồm: tên, loại hình, lĩnh vực, số lượng LĐ theo từng loại GPLĐ.'
            },
            {
                title: '🔍 Bộ lọc',
                items: [
                    '<strong>Lĩnh vực:</strong> Lọc theo lĩnh vực hoạt động (Sản xuất, Dịch vụ, Thương mại...)',
                    '<strong>Cán bộ TD:</strong> Lọc theo cán bộ theo dõi được phân công.'
                ]
            },
            {
                title: '✏️ Thao tác',
                items: [
                    '👁️ <strong>Xem:</strong> Xem chi tiết toàn bộ thông tin công ty',
                    '✏️ <strong>Sửa:</strong> Cập nhật thông tin công ty',
                    '📤 <strong>Import:</strong> Import danh sách NLĐ từ file Excel cho công ty này'
                ],
                tip: 'Nhấn "+ Thêm công ty" để thêm công ty mới. Nhấn "Import Excel" để cập nhật hàng loạt.'
            }
        ]
    },
    employees: {
        title: '👥 Quản lý Nhân viên',
        sections: [
            {
                title: '📋 Danh sách nhân viên',
                content: 'Hiển thị toàn bộ NLĐ nước ngoài đang làm việc. Cột "Trạng thái" thể hiện tình trạng hạn tạm trú.'
            },
            {
                title: '🔍 Bộ lọc nhanh',
                items: [
                    '<strong>Quốc tịch:</strong> Lọc theo quốc tịch (Trung Quốc, Hàn Quốc, Nhật Bản...)',
                    '<strong>GPLĐ:</strong> Lọc theo trạng thái Giấy phép lao động',
                    '<strong>Trạng thái:</strong> Đang làm / Đã nghỉ'
                ]
            },
            {
                title: '🏷️ Ý nghĩa màu thẻ trạng thái',
                items: [
                    '<span style="color:var(--red)">🔴 Sắp hết hạn</span> — Dưới 7 ngày, cần xử lý ngay',
                    '<span style="color:var(--orange)">🟠 Gần hết hạn</span> — 7-30 ngày, nên theo dõi',
                    '<span style="color:var(--green)">🟢 Bình thường</span> — Còn trên 30 ngày'
                ],
                tip: 'Nhấn "Xuất Excel" để tải danh sách hiện tại ra file Excel.'
            }
        ]
    },
    import: {
        title: '📤 Import dữ liệu từ Excel',
        sections: [
            {
                title: '📝 Quy trình 4 bước',
                items: [
                    '<strong>Bước 1 — Upload:</strong> Chọn file Excel (.xlsx) và chọn công ty nhận dữ liệu.',
                    '<strong>Bước 2 — Ghép cột:</strong> Hệ thống tự nhận dạng cột. Cột nào không nhận ra sẽ hiển thị ⚠️ để bạn chọn thủ công.',
                    '<strong>Bước 3 — Xem trước:</strong> Kiểm tra dữ liệu trước khi import. Bản ghi trùng (cùng hộ chiếu) sẽ được đánh dấu 🟡.',
                    '<strong>Bước 4 — Kết quả:</strong> Xem số lượng thêm mới, cập nhật, và lỗi.'
                ]
            },
            {
                title: '💡 Mẹo sử dụng',
                items: [
                    'File Excel phải có <strong>tiêu đề cột ở dòng 1</strong>, dữ liệu bắt đầu từ dòng 2.',
                    'Lưu template ghép cột để không cần ghép lại khi import file cùng định dạng.',
                    'Cột thiếu sẽ dùng giá trị mặc định, cột thừa có thể bỏ qua.'
                ],
                tip: 'Sau khi import, vào "Lịch sử Import" (Quản trị) để xem lại toàn bộ lần import.'
            }
        ]
    },
    search: {
        title: '🔍 Tìm kiếm & Thống kê',
        sections: [
            {
                title: '🔎 Tìm kiếm toàn cục',
                content: 'Nhập bất kỳ thông tin gì — tên người, số hộ chiếu, tên công ty, quốc tịch — hệ thống tự tìm trên <strong>tất cả các trường</strong> của cả Công ty và Nhân viên.'
            },
            {
                title: '📊 Thống kê tự động',
                content: 'Sau khi tìm, hệ thống tự hiển thị 4 thẻ thống kê: số nhân viên, công ty liên quan, tỷ lệ GPLĐ, sắp hết hạn.',
                tip: 'Từ khóa được <mark>đánh dấu vàng</mark> trong kết quả để dễ nhận biết.'
            },
            {
                title: '📋 Kết quả',
                items: [
                    'Kết quả chia theo loại: 👥 Nhân viên hoặc 🏢 Công ty',
                    'Nhấn "Xem →" để mở chi tiết bản ghi.'
                ]
            }
        ]
    },
    reports: {
        title: '📝 Báo cáo tùy chỉnh',
        sections: [
            {
                title: '⚙️ Panel cấu hình (bên trái)',
                items: [
                    '<strong>Nguồn dữ liệu:</strong> Chọn báo cáo theo Công ty, Nhân viên, hoặc cả hai.',
                    '<strong>Chọn cột:</strong> Tick/bỏ tick các cột muốn hiển thị trong báo cáo.',
                    '<strong>Điều kiện lọc:</strong> Thêm điều kiện (VD: hạn tạm trú ≤ 30 ngày).',
                    '<strong>Nhóm theo:</strong> Nhóm kết quả theo Công ty, Quốc tịch, hoặc Lĩnh vực.'
                ]
            },
            {
                title: '👁️ Xem trước (bên phải)',
                content: 'Kết quả hiển thị real-time khi thay đổi cấu hình. Kiểm tra trước khi xuất file.'
            },
            {
                title: '💾 Template',
                content: 'Lưu cấu hình báo cáo thành template để sử dụng lại. Nhấn "Chạy" để tạo báo cáo từ template đã lưu.',
                tip: 'Xuất file: nhấn "Xuất Excel" để tải .xlsx, hoặc "Xuất PDF" để tạo file PDF in ấn.'
            }
        ]
    },
    admin: {
        title: '⚙️ Quản trị hệ thống',
        sections: [
            {
                title: '📦 Danh mục quản lý',
                items: [
                    '<strong>Tài khoản:</strong> Thêm/sửa/khóa tài khoản người dùng. Phân quyền Admin/User.',
                    '<strong>Lĩnh vực:</strong> Quản lý danh mục lĩnh vực hoạt động của doanh nghiệp.',
                    '<strong>Ngành nghề:</strong> Quản lý danh mục nghề nghiệp, nhóm nghề.',
                    '<strong>Quốc tịch:</strong> Quản lý mã và tên quốc tịch.',
                    '<strong>Quận/Huyện, Phường/Xã:</strong> Quản lý địa bàn.'
                ]
            },
            {
                title: '📜 Tính năng mới',
                items: [
                    '<strong>Nhật ký hệ thống:</strong> Ghi lại mọi thao tác (thêm/sửa/xóa) để kiểm soát.',
                    '<strong>Lịch sử Import:</strong> Xem lại toàn bộ các lần import file Excel.'
                ],
                tip: 'Chỉ tài khoản Quản trị viên (Admin) mới truy cập được trang này.'
            }
        ]
    }
};

// ============ NAVIGATION ============
function showDashboard() {
    document.getElementById('loginScreen').classList.remove('active-screen');
    document.getElementById('mainApp').classList.add('active-screen');
    switchPage('dashboard');
    initCharts();
    animateKPIs();
}

function showLogin() {
    document.getElementById('mainApp').classList.remove('active-screen');
    document.getElementById('loginScreen').classList.add('active-screen');
}

let currentPage = 'dashboard';

function switchPage(page) {
    // Animate out current page
    const currentEl = document.querySelector('.page.active-page');
    if (currentEl && page !== currentPage) {
        currentEl.style.opacity = '0';
        currentEl.style.transform = 'translateY(8px)';
    }

    setTimeout(() => {
        document.querySelectorAll('.page').forEach(p => {
            p.classList.remove('active-page');
            p.style.opacity = '';
            p.style.transform = '';
        });
        const target = document.getElementById('page-' + page);
        if (target) {
            target.classList.add('active-page');
            target.style.animation = 'none';
            target.offsetHeight; // trigger reflow
            target.style.animation = '';
        }

        document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));
        const navItem = document.querySelector(`.nav-item[data-page="${page}"]`);
        if (navItem) navItem.classList.add('active');

        const titles = {
            dashboard: 'Dashboard', companies: 'Quản lý Công ty', employees: 'Quản lý Nhân viên',
            import: 'Import Excel', search: 'Tìm kiếm & Thống kê', reports: 'Báo cáo', admin: 'Quản trị hệ thống'
        };
        const breadcrumbs = {
            dashboard: 'Trang chủ / Dashboard',
            companies: 'Trang chủ / <a href="#" onclick="switchPage(\'companies\')">Quản lý dữ liệu</a> / Công ty',
            employees: 'Trang chủ / <a href="#" onclick="switchPage(\'employees\')">Quản lý dữ liệu</a> / Nhân viên',
            import: 'Trang chủ / <a href="#" onclick="switchPage(\'import\')">Quản lý dữ liệu</a> / Import Excel',
            search: 'Trang chủ / Phân tích & Báo cáo / Tìm kiếm',
            reports: 'Trang chủ / Phân tích & Báo cáo / Báo cáo',
            admin: 'Trang chủ / Quản trị hệ thống'
        };
        document.getElementById('pageTitle').textContent = titles[page] || page;
        document.getElementById('breadcrumb').innerHTML = breadcrumbs[page] || '';

        if (page === 'import') goImportStep(1);
        if (page === 'companies') populateCompanyTable();
        if (page === 'employees') populateEmployeeTable();

        updateHelpContent(page);
        currentPage = page;
    }, page !== currentPage ? 120 : 0);
}

function toggleSidebar() {
    document.getElementById('sidebar').classList.toggle('collapsed');
}

// ============ HELP PANEL ============
function toggleHelp() {
    const overlay = document.getElementById('helpOverlay');
    const btn = document.getElementById('btnHelp');
    overlay.classList.toggle('open');
    btn.classList.toggle('active');
}

function switchHelpTab(tab) {
    document.querySelectorAll('.help-tab').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.help-content').forEach(c => c.classList.remove('active'));
    event.target.classList.add('active');
    document.getElementById('help' + tab.charAt(0).toUpperCase() + tab.slice(1)).classList.add('active');
}

function updateHelpContent(page) {
    const data = helpData[page];
    const container = document.getElementById('helpContent');
    if (!data || !container) return;

    let html = `<div class="help-section"><h4>${data.title}</h4></div>`;
    data.sections.forEach(sec => {
        html += '<div class="help-section">';
        html += `<h4>${sec.title}</h4>`;
        if (sec.content) html += `<p>${sec.content}</p>`;
        if (sec.items) {
            html += '<ul>';
            sec.items.forEach(item => html += `<li>${item}</li>`);
            html += '</ul>';
        }
        if (sec.tip) html += `<div class="help-tip">💡 <strong>Mẹo:</strong> ${sec.tip}</div>`;
        html += '</div>';
    });
    container.innerHTML = html;
}

// ============ KPI ANIMATION ============
function animateKPIs() {
    animateNumber('kpiCompanies', 156, 1200);
    animateNumber('kpiEmployees', 1245, 1500);
    animateNumber('kpiExpiring', 23, 800);
    animateNumber('kpiPermit', 1062, 1300);
}

function animateNumber(id, target, duration) {
    const el = document.getElementById(id);
    if (!el) return;
    const startTime = performance.now();
    function update(currentTime) {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / duration, 1);
        const eased = 1 - Math.pow(1 - progress, 3);
        el.textContent = Math.floor(eased * target).toLocaleString('vi-VN');
        if (progress < 1) requestAnimationFrame(update);
    }
    requestAnimationFrame(update);
}

// ============ CHARTS ============
let charts = {};

function getChartColors() {
    const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
    return {
        textColor: isDark ? '#9aa0a6' : '#64748b',
        gridColor: isDark ? '#2d3139' : '#f1f5f9',
        borderColor: isDark ? '#2d3139' : '#e2e8f0',
    };
}

function destroyCharts() {
    Object.values(charts).forEach(c => { if (c) c.destroy(); });
    charts = {};
}

function initCharts() {
    if (charts.nationality) return;
    const colors = getChartColors();

    Chart.defaults.color = colors.textColor;
    Chart.defaults.borderColor = colors.borderColor;
    Chart.defaults.font.family = "'Inter', sans-serif";

    const ctxNat = document.getElementById('chartNationality');
    if (ctxNat) {
        charts.nationality = new Chart(ctxNat, {
            type: 'bar',
            data: {
                labels: ['Trung Quốc', 'Hàn Quốc', 'Nhật Bản', 'Đài Loan', 'Ấn Độ', 'Malaysia', 'Philippines', 'Thái Lan', 'Indonesia', 'Khác'],
                datasets: [{
                    label: 'Số lao động',
                    data: [245, 180, 156, 120, 98, 87, 76, 65, 54, 164],
                    backgroundColor: ['#2563eb','#7c3aed','#ec4899','#059669','#d97706','#8b5cf6','#14b8a6','#f97316','#6366f1','#94a3b8'],
                    borderRadius: 6, borderSkipped: false,
                }]
            },
            options: {
                responsive: true, maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: { beginAtZero: true, grid: { color: colors.gridColor } },
                    x: { grid: { display: false } }
                },
                animation: { duration: 1500, easing: 'easeOutQuart' }
            }
        });
    }

    const ctxPermit = document.getElementById('chartPermit');
    if (ctxPermit) {
        charts.permit = new Chart(ctxPermit, {
            type: 'doughnut',
            data: {
                labels: ['Đã có GPLĐ', 'Miễn GPLĐ', 'Chưa có GPLĐ', 'Khác'],
                datasets: [{
                    data: [620, 350, 180, 95],
                    backgroundColor: ['#059669','#2563eb','#d97706','#94a3b8'],
                    borderWidth: 2,
                    borderColor: document.documentElement.getAttribute('data-theme') === 'dark' ? '#1c1f26' : '#ffffff',
                    spacing: 3
                }]
            },
            options: {
                responsive: true, maintainAspectRatio: false, cutout: '62%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { padding: 18, usePointStyle: true, pointStyleWidth: 10, font: { size: 12 } }
                    }
                },
                animation: { animateRotate: true, duration: 1500 }
            }
        });
    }

    const ctxTrend = document.getElementById('chartTrend');
    if (ctxTrend) {
        charts.trend = new Chart(ctxTrend, {
            type: 'line',
            data: {
                labels: ['T4/26','T5/26','T6/26','T7/26','T8/26','T9/26','T10/26','T11/26','T12/26','T1/27','T2/27','T3/27'],
                datasets: [{
                    label: 'Hết hạn tạm trú', data: [23, 35, 28, 42, 19, 31, 45, 22, 37, 26, 33, 29],
                    borderColor: '#d97706', backgroundColor: 'rgba(217,119,6,0.08)', fill: true,
                    tension: 0.4, pointRadius: 4, pointHoverRadius: 7, pointBackgroundColor: '#d97706', borderWidth: 2.5,
                }, {
                    label: 'Đã gia hạn', data: [18, 29, 22, 35, 15, 25, 38, 18, 30, 20, 27, 22],
                    borderColor: '#059669', backgroundColor: 'rgba(5,150,105,0.08)', fill: true,
                    tension: 0.4, pointRadius: 4, pointHoverRadius: 7, pointBackgroundColor: '#059669', borderWidth: 2.5,
                }]
            },
            options: {
                responsive: true, maintainAspectRatio: false,
                plugins: { legend: { position: 'top', align: 'end', labels: { usePointStyle: true, pointStyleWidth: 10 } } },
                scales: {
                    y: { beginAtZero: true, grid: { color: colors.gridColor } },
                    x: { grid: { display: false } }
                },
                interaction: { mode: 'index', intersect: false },
                animation: { duration: 1800, easing: 'easeOutQuart' }
            }
        });
    }
}

// ============ TABLE DATA ============
function populateCompanyTable() {
    const table = document.querySelector('#companyTable tbody');
    if (table.children.length > 0) return;
    const companies = [
        { name: 'Cty TNHH Samsung Electronics VN', type: 'TNHH', field: 'Sản xuất', total: 125, permit: 98, notyet: 15, exempt: 12, tracker: 'Nguyễn Văn A' },
        { name: 'Cty CP Hyundai Aluminum Vina', type: 'Cổ phần', field: 'Công nghiệp', total: 89, permit: 72, notyet: 10, exempt: 7, tracker: 'Trần Thị B' },
        { name: 'Cty TNHH Canon Vietnam', type: 'TNHH', field: 'Sản xuất', total: 67, permit: 55, notyet: 5, exempt: 7, tracker: 'Nguyễn Văn A' },
        { name: 'Cty TNHH LG Display Vietnam', type: 'TNHH', field: 'Sản xuất', total: 56, permit: 48, notyet: 3, exempt: 5, tracker: 'Lê Văn C' },
        { name: 'Cty CP Posco Vietnam', type: 'Cổ phần', field: 'Thép', total: 45, permit: 38, notyet: 4, exempt: 3, tracker: 'Trần Thị B' },
        { name: 'Cty TNHH Foxconn Bình Dương', type: 'TNHH', field: 'Điện tử', total: 78, permit: 65, notyet: 8, exempt: 5, tracker: 'Nguyễn Văn A' },
        { name: 'Cty TNHH Panasonic VN', type: 'TNHH', field: 'Sản xuất', total: 34, permit: 28, notyet: 3, exempt: 3, tracker: 'Lê Văn C' },
        { name: 'Cty CP Formosa VN', type: 'Cổ phần', field: 'Hóa chất', total: 92, permit: 76, notyet: 9, exempt: 7, tracker: 'Trần Thị B' },
        { name: 'Cty TNHH Toyota Motor VN', type: 'TNHH', field: 'Ô tô', total: 41, permit: 35, notyet: 3, exempt: 3, tracker: 'Nguyễn Văn A' },
        { name: 'Cty TNHH Intel Products VN', type: 'TNHH', field: 'Bán dẫn', total: 112, permit: 95, notyet: 10, exempt: 7, tracker: 'Lê Văn C' },
    ];
    companies.forEach((c, i) => {
        const row = document.createElement('tr');
        row.style.animationDelay = `${i * 40}ms`;
        row.innerHTML = `<td>${i + 1}</td><td><strong>${c.name}</strong></td><td>${c.type}</td>
            <td><span class="tag tag-purple">${c.field}</span></td><td><strong>${c.total}</strong></td>
            <td><span class="tag tag-green">${c.permit}</span></td><td><span class="tag tag-orange">${c.notyet}</span></td>
            <td><span class="tag tag-blue">${c.exempt}</span></td><td>${c.tracker}</td>
            <td><button class="btn-action" title="Xem">👁️</button><button class="btn-action" title="Sửa">✏️</button><button class="btn-action" title="Import">📤</button></td>`;
        table.appendChild(row);
    });
}

function populateEmployeeTable() {
    const table = document.querySelector('#employeeTable tbody');
    if (table.children.length > 0) return;
    const employees = [
        { name: 'Zhang Wei', nation: 'Trung Quốc', passport: 'E12345678', company: 'Samsung Electronics VN', career: 'Kỹ sư', permit: 'Có GPLĐ', permitTag: 'green', stay: '18/04/2026', status: 'danger' },
        { name: 'Kim Soo-hyun', nation: 'Hàn Quốc', passport: 'M98765432', company: 'Hyundai Aluminum Vina', career: 'Quản lý', permit: 'Có GPLĐ', permitTag: 'green', stay: '15/08/2026', status: 'ok' },
        { name: 'Tanaka Yuki', nation: 'Nhật Bản', passport: 'TK4567890', company: 'Canon Vietnam', career: 'Phiên dịch', permit: 'Miễn', permitTag: 'blue', stay: '27/04/2026', status: 'warning' },
        { name: 'Park Ji-hoon', nation: 'Hàn Quốc', passport: 'M99887766', company: 'Hyundai Aluminum Vina', career: 'Kỹ thuật', permit: 'Có GPLĐ', permitTag: 'green', stay: '03/05/2026', status: 'warning' },
        { name: 'Wang Li', nation: 'Trung Quốc', passport: 'E98765432', company: 'Foxconn Bình Dương', career: 'Quản lý', permit: 'Có GPLĐ', permitTag: 'green', stay: '22/11/2026', status: 'ok' },
        { name: 'Lee Min-ho', nation: 'Hàn Quốc', passport: 'M11223344', company: 'LG Display Vietnam', career: 'Kỹ thuật', permit: 'Chưa có', permitTag: 'orange', stay: '30/09/2026', status: 'ok' },
        { name: 'Suzuki Taro', nation: 'Nhật Bản', passport: 'TJ1122334', company: 'Canon Vietnam', career: 'Giám đốc', permit: 'Miễn', permitTag: 'blue', stay: '15/12/2026', status: 'ok' },
        { name: 'Liu Chen', nation: 'Trung Quốc', passport: 'E55566677', company: 'Samsung Electronics VN', career: 'Kỹ thuật', permit: 'Có GPLĐ', permitTag: 'green', stay: '30/12/2026', status: 'ok' },
        { name: 'Chen Hao', nation: 'Đài Loan', passport: 'T44556677', company: 'Formosa VN', career: 'Quản lý SX', permit: 'Có GPLĐ', permitTag: 'green', stay: '05/07/2026', status: 'ok' },
        { name: 'Raj Patel', nation: 'Ấn Độ', passport: 'H88990011', company: 'Intel Products VN', career: 'KS phần mềm', permit: 'Có GPLĐ', permitTag: 'green', stay: '20/03/2027', status: 'ok' },
    ];
    employees.forEach((e, i) => {
        const row = document.createElement('tr');
        row.style.animationDelay = `${i * 40}ms`;
        const statusHtml = e.status === 'danger' ? '<span class="tag tag-red">Sắp hết hạn</span>'
            : e.status === 'warning' ? '<span class="tag tag-orange">Gần hết hạn</span>'
            : '<span class="tag tag-green">Bình thường</span>';
        row.innerHTML = `<td>${i + 1}</td><td><strong>${e.name}</strong></td><td>${e.nation}</td>
            <td><code style="font-size:12px;background:var(--bg-muted);padding:3px 8px;border-radius:4px;color:var(--text-primary)">${e.passport}</code></td>
            <td>${e.company}</td><td>${e.career}</td><td><span class="tag tag-${e.permitTag}">${e.permit}</span></td>
            <td>${e.stay}</td><td>${statusHtml}</td>
            <td><button class="btn-action" title="Xem">👁️</button><button class="btn-action" title="Sửa">✏️</button></td>`;
        table.appendChild(row);
    });
}

// ============ IMPORT WIZARD ============
function goImportStep(step) {
    for (let i = 1; i <= 4; i++) {
        const el = document.getElementById('importStep' + i);
        if (el) el.classList.add('hidden');
    }
    const target = document.getElementById('importStep' + step);
    if (target) {
        target.classList.remove('hidden');
        target.style.animation = 'none';
        target.offsetHeight;
        target.style.animation = '';
    }
    document.querySelectorAll('.wizard-step').forEach((ws, idx) => {
        ws.classList.remove('active', 'done');
        if (idx + 1 < step) ws.classList.add('done');
        if (idx + 1 === step) ws.classList.add('active');
    });
    document.querySelectorAll('.wizard-connector').forEach((conn, idx) => {
        conn.style.background = (idx + 1 < step) ? 'var(--green)' : 'var(--border)';
    });
}

// ============ SEARCH ============
function doSearch() {
    const results = document.getElementById('searchResults');
    results.style.display = 'block';
    results.style.animation = 'fadeUp 0.4s ease';
    results.scrollIntoView({ behavior: 'smooth' });
}

// ============ KEYBOARD SHORTCUTS ============
document.addEventListener('keydown', function(e) {
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault();
        const searchInput = document.getElementById('globalSearchInput');
        if (searchInput) searchInput.focus();
    }
    if (e.key === 'F1') {
        e.preventDefault();
        toggleHelp();
    }
    if (e.key === 'Escape') {
        const overlay = document.getElementById('helpOverlay');
        if (overlay.classList.contains('open')) toggleHelp();
    }
});

// ============ AUTO INIT ============
document.addEventListener('DOMContentLoaded', function() {
    document.getElementById('loginScreen').classList.add('active-screen');
    updateHelpContent('dashboard');
});
