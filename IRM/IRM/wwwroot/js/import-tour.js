(function () {
    'use strict';
    const sessions = new Map();
    const notice = 'Hướng dẫn chưa sẵn sàng ở vị trí này. Bạn có thể mở lại; thao tác import vẫn dùng được.';
    const action = 'Thực hiện thao tác trên màn hình để tiếp tục.';
    // Fixed copy only: never interpolate file names, headers or personal data into Driver HTML.
    const chapters = {
        0: [
            ['prepare-download', 'Tải file Excel mẫu', 'Tải mẫu có đủ 35 cột và hướng dẫn từng trường. Đối chiếu danh mục trong ứng dụng rồi điền dữ liệu tại sheet Hồ sơ từ dòng 2.'],
            ['prepare-fields', 'Điền đủ thông tin và đối chiếu', 'Chín trường bắt buộc đặt trước trong file mẫu. Mở Tra cứu 35 trường khi cần. Ô trống có thể ảnh hưởng dữ liệu cũ khi cập nhật.'],
            ['prepare-next', 'Chuyển sang nạp file đã điền', 'Khi đã chuẩn bị file, bấm Tôi đã chuẩn bị file. Có thể dùng lại file theo mẫu đã điền từ trước.', true]
        ],
        1: [
            ['upload', 'Chuẩn bị file Excel', 'Dùng .xlsx. Đặt số hộ chiếu, GPLĐ, visa và mã thành Text trước khi dán để giữ số 0 đầu. Rà Nam/Nữ trong file.'],
            ['company', 'Đúng công ty', 'Chọn công ty mặc định. Ô Công ty trống trong file sẽ dùng công ty này; tên có nội dung phải khớp công ty đích.'],
            ['career-group', 'Đúng nhóm nghề', 'Tên nghề phải khớp đầy đủ, có dấu và thuộc nhóm đã chọn. Quốc tịch, nghề, phường/xã theo danh mục database đích.'],
            ['read-options', 'Đúng sheet và vùng dòng', 'Chọn sheet dữ liệu, dòng tiêu đề và dòng đầu/cuối theo số dòng Excel. Ngày lõi dùng dd/MM/yyyy, năm bốn chữ số.'],
            ['create-company', 'Tạo công ty là lựa chọn riêng', 'Chỉ bật khi thật sự muốn tạo công ty chưa có. Đừng bật để bỏ qua lỗi chính tả; kiểm tra tên tại xem trước.'],
            ['go-mapping', 'Chuyển sang ghép cột', 'Chọn file và nhóm nghề rồi bấm Tiếp tục của ứng dụng.', true]
        ],
        2: [
            ['field-lookup', 'Tra cứu 35 trường', 'Mở tra cứu hoặc nút ? cạnh trường để xem kiểu Excel, giá trị, ô trống và cách sửa lỗi. Mở tra cứu sẽ dừng tour; có thể mở lại.'],
            ['mapping', 'Ghép theo ý nghĩa', 'Rà chín trường bắt buộc, GPLĐ và Số GPLĐ riêng. Chưa có GPLĐ thì để trống cả hai; chữ “Chưa có” có thể bị hiểu thành số. Cột không ghép khác với ghép nhưng ô trống khi cập nhật.'],
            ['go-preview', 'Tạo xem trước', 'Tự kiểm tra cặp trường mở rộng, thứ tự danh sách giấy tờ dùng dấu ; và ngày từ chung, rồi bấm Tiếp tục của ứng dụng.', true]
        ],
        3: [
            ['preview-summary', 'Đọc trạng thái từng dòng', 'Mới dự kiến được thêm; đã có sẵn được bỏ qua; thay đổi mặc định bỏ qua; lỗi cần sửa theo số dòng Excel thật.'],
            ['preview-table', 'Chủ động quyết định cập nhật', 'Chỉ chọn Cập nhật khi đã đối chiếu bản cũ. Cảnh báo không luôn chặn dòng. Bảng này không hiển thị giới tính: kiểm tra file trước, hồ sơ sau import.'],
            ['execute-import', 'Bạn quyết định ghi dữ liệu', 'Xem trước không bảo đảm ghi thành công. Dữ liệu đổi sau preview cần xem lại; lỗi khi ghi phần mở rộng có thể hủy cả giao dịch. Chỉ bấm Bắt đầu Import khi đã sẵn sàng.', true]
        ],
        4: [
            ['result-summary', 'Đối chiếu kết quả đã ghi', 'Đọc số thêm mới, cập nhật, bỏ qua và lỗi. Thành công có thể vẫn có dòng lỗi. Kiểm tra hồ sơ đã lưu, nhất là giới tính và các trường mở rộng.'],
            ['result-errors', 'Đọc nguyên nhân lỗi', 'Lỗi ghi có thể hủy cả giao dịch. Sửa nguồn dữ liệu, danh mục hoặc khoảng ngày theo thông báo rồi tạo xem trước mới.', false, true],
            ['result-actions', 'Lịch sử và hoàn tác', 'Hoàn tác phụ thuộc quyền và dữ liệu chưa bị chỉnh sửa và chưa có phiên/phụ thuộc mới hơn. Không phải lúc nào cũng hoàn tác được; xử lý phiên liên quan từ mới về cũ.']
        ]
    };

    function registered(entry) { return sessions.get(entry.id) === entry; }
    function cancelWait(entry) {
        if (!entry.wait) return;
        clearTimeout(entry.wait.timer); entry.wait.observer.disconnect(); entry.wait = null;
    }
    function releaseDriver(entry) {
        entry.generation++;
        const driver = entry.driver; entry.driver = null;
        if (driver) driver.destroy();
    }
    function dispose(id) {
        const entry = sessions.get(id);
        if (!entry) return;
        sessions.delete(id);
        cancelWait(entry); entry.rootObserver?.disconnect(); releaseDriver(entry);
    }
    function unavailable(entry) {
        const target = entry.root.querySelector('[data-import-tour="notice"]');
        dispose(entry.id);
        if (target) target.textContent = notice;
    }
    function activate(entry) {
        if (!registered(entry)) return;
        if (!entry.root.isConnected) { dispose(entry.id); return; }
        const definitions = chapters[entry.snapshot.stage];
        if (!definitions) { unavailable(entry); return; }
        const find = name => entry.root.querySelector(`[data-import-tour="${name}"]`);
        if (definitions.some(([name, , , , optional]) => !optional && !find(name))) {
            if (!entry.wait) {
                const observer = new MutationObserver(() => activate(entry));
                observer.observe(entry.root, { childList: true, subtree: true });
                entry.wait = { observer, timer: setTimeout(() => { if (registered(entry)) unavailable(entry); }, 1500) };
            }
            return;
        }
        cancelWait(entry);
        const generation = entry.generation;
        const steps = definitions.filter(([name, , , , optional]) => !optional || find(name)).map(([name, title, description, waits]) => ({
            element: () => find(name),
            popover: { title, description, ...(waits ? { instruction: action } : {}) }
        }));
        const reduced = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches ?? false;
        const driver = window.driver.js.driver({
            steps, animate: !reduced, smoothScroll: !reduced, allowClose: true,
            overlayClickBehavior: 'none', showProgress: true, progressText: 'Gợi ý {{current}}/{{total}}',
            nextBtnText: 'Tiếp', prevBtnText: 'Quay lại', doneBtnText: 'Hoàn tất', closeBtnLabel: 'Đóng hướng dẫn',
            popoverClass: 'irm-import-tour', waitForElement: 1500,
            allowedElements: () => [...document.querySelectorAll('.mud-popover-open'), ...entry.root.querySelectorAll('[data-import-tour-control]')],
            onHighlighted: (_element, _step, options) => { if (registered(entry) && generation === entry.generation) entry.index = options.index ?? 0; },
            onDestroyed: () => { if (registered(entry) && generation === entry.generation) dispose(entry.id); },
            onElementMissing: () => { if (registered(entry) && generation === entry.generation) unavailable(entry); return false; }
        });
        entry.driver = driver;
        driver.drive(Math.min(entry.index, steps.length - 1));
    }
    function start(id, snapshot) {
        dispose(id);
        const root = document.getElementById(id);
        if (!root || !window.driver?.js?.driver || snapshot.isBusy || snapshot.isManual || !chapters[snapshot.stage]) return false;
        const entry = { id, root, snapshot: { ...snapshot }, driver: null, index: 0, generation: 0, wait: null };
        sessions.set(id, entry);
        const target = root.querySelector('[data-import-tour="notice"]');
        if (target) target.textContent = '';
        entry.rootObserver = new MutationObserver(() => { if (!root.isConnected && registered(entry)) dispose(id); });
        entry.rootObserver.observe(document.body, { childList: true, subtree: true });
        try { activate(entry); return true; }
        catch { dispose(id); return false; }
    }
    function sync(id, snapshot) {
        const entry = sessions.get(id);
        if (!entry) return;
        if (snapshot.isManual || snapshot.contextVersion !== entry.snapshot.contextVersion || !entry.root.isConnected) { dispose(id); return; }
        const previous = entry.snapshot; entry.snapshot = { ...snapshot };
        const changed = snapshot.stage !== previous.stage;
        if (changed) entry.index = 0;
        try {
            if (snapshot.isBusy) { cancelWait(entry); releaseDriver(entry); return; }
            if (changed || previous.isBusy) { cancelWait(entry); releaseDriver(entry); activate(entry); }
            else entry.driver?.refresh();
        } catch { unavailable(entry); }
    }
    window.irmImportTour = { start, sync, stop: dispose, dispose };
})();
