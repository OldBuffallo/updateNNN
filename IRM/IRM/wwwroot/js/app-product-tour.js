(function () {
    const STORAGE_KEY = 'irm-product-tour-v1-seen';
    let activeDriver = null;

    const STEP_DEFS = [
        {
            element: '[data-app-tour="admin-system"]',
            popover: {
                title: 'Bước 1: Thiết lập Quản trị hệ thống & Danh mục',
                description: 'Trước khi vận hành, Quản trị viên nên kiểm tra và bổ sung các danh mục nền (Quốc tịch, Quận/Huyện, Phường/Xã, Lĩnh vực, Ngành nghề, Địa bàn hành chính) và phân quyền tài khoản tại đây.',
                side: 'right',
                align: 'start'
            }
        },
        {
            element: '[data-app-tour="nav-dashboard"]',
            popover: {
                title: 'Bảng điều khiển',
                description: 'Theo dõi tổng quan số lượng người nước ngoài, cảnh báo sắp hết hạn tạm trú và biểu đồ phân bố quốc tịch, giấy phép lao động.',
                side: 'right',
                align: 'start'
            }
        },
        {
            element: '[data-app-tour="nav-foreign-persons"]',
            popover: {
                title: 'Hồ sơ Người nước ngoài',
                description: 'Quản lý hợp nhất Lao động, Du học sinh và Thăm thân. Lưu ý quan trọng: mọi hồ sơ đều PHẢI có Số hộ chiếu (trường bắt buộc duy nhất; các thông tin khác có thể bổ sung sau).',
                side: 'right',
                align: 'start'
            }
        },
        {
            element: '[data-app-tour="nav-companies"]',
            popover: {
                title: 'Doanh nghiệp',
                description: 'Quản lý doanh nghiệp bảo lãnh, địa điểm hoạt động/KCN, người đại diện pháp luật và hồ sơ pháp lý đính kèm.',
                side: 'right',
                align: 'start'
            }
        },
        {
            element: '[data-app-tour="nav-accommodations"]',
            popover: {
                title: 'Cơ sở lưu trú',
                description: 'Quản lý nơi cư trú, sức chứa và hợp đồng thuê lưu trú giữa doanh nghiệp với cơ sở lưu trú.',
                side: 'right',
                align: 'start'
            }
        },
        {
            element: '[data-app-tour="nav-inspections"]',
            popover: {
                title: 'Đợt kiểm tra NNN',
                description: 'Tạo đợt kiểm tra hiện trường, ghi nhận đối tượng kiểm tra, tình trạng hợp lệ và chi tiết vi phạm/xác minh.',
                side: 'right',
                align: 'start'
            }
        },
        {
            element: '[data-app-tour="nav-search"]',
            popover: {
                title: 'Tra cứu toàn cục (Ctrl+K)',
                description: 'Tìm kiếm nhanh theo Số hộ chiếu, họ tên, doanh nghiệp hoặc địa bàn trên toàn bộ hệ thống bất cứ lúc nào.',
                side: 'right',
                align: 'start'
            }
        },
        {
            element: '[data-app-tour="nav-reports"]',
            popover: {
                title: 'Báo cáo định kỳ',
                description: 'Tùy chọn cột hiển thị, gom nhóm theo Công ty / Quốc tịch / Lĩnh vực và xuất file Excel báo cáo nhanh chóng.',
                side: 'right',
                align: 'start'
            }
        },
        {
            element: '[data-app-tour="nav-statistics"]',
            popover: {
                title: 'Thống kê 54 địa bàn',
                description: 'Xem thống kê phân bố người nước ngoài theo 54 địa bàn hành chính, khu công nghiệp và tình trạng định danh điện tử.',
                side: 'right',
                align: 'start'
            }
        },
        {
            element: '[data-app-tour="nav-import"]',
            popover: {
                title: 'Nhập dữ liệu Excel & Chuẩn hóa',
                description: 'Tải file Excel nguồn (kể cả file nhiều cột lộn xộn), chọn đúng trường cần lấy, tự chuẩn hóa ngày dd/MM/yyyy và xem thông báo cảnh báo trước khi nạp. Lưu ý: mọi dòng dữ liệu đều PHẢI có Số hộ chiếu.',
                side: 'right',
                align: 'start'
            }
        },
        {
            element: '#btnAppUserGuide',
            popover: {
                title: 'Xem lại Hướng dẫn bất cứ lúc nào',
                description: 'Nhấn nút "Hướng dẫn sử dụng" trên thanh tiêu đề để mở lại hướng dẫn từng chức năng bất cứ khi nào bạn cần.',
                side: 'bottom',
                align: 'end'
            }
        }
    ];

    function hasSeen() {
        try {
            return localStorage.getItem(STORAGE_KEY) === '1';
        } catch {
            return false;
        }
    }

    function markSeen() {
        try {
            localStorage.setItem(STORAGE_KEY, '1');
        } catch {
            // Ignore blocked storage
        }
    }

    function buildSteps(doc) {
        const root = doc || document;
        if (!root || typeof root.querySelector !== 'function') return [];
        return STEP_DEFS.filter(step => root.querySelector(step.element) !== null);
    }

    function init(forceReplay) {
        if (!forceReplay && hasSeen()) return false;
        const driverFactory = window.driver && window.driver.js && window.driver.js.driver;
        if (typeof driverFactory !== 'function') return false;

        const steps = buildSteps(document);
        if (steps.length === 0) return false;

        if (activeDriver && typeof activeDriver.destroy === 'function') {
            try { activeDriver.destroy(); } catch { /* ignore */ }
        }

        activeDriver = driverFactory({
            showProgress: true,
            allowClose: true,
            nextBtnText: 'Tiếp theo →',
            prevBtnText: '← Quay lại',
            doneBtnText: 'Hoàn tất',
            progressText: '{{current}} / {{total}}',
            steps: steps,
            onHighlightStarted: function (element) {
                if (element && typeof element.scrollIntoView === 'function') {
                    try { element.scrollIntoView({ block: 'center', behavior: 'smooth' }); } catch { /* ignore */ }
                }
            },
            onDestroyed: function () {
                markSeen();
                activeDriver = null;
            }
        });

        activeDriver.drive();
        return true;
    }

    window.irmAppTour = {
        init: init,
        buildSteps: buildSteps,
        markSeen: markSeen
    };
})();
