// =========================================================
// 1. TOAST NOTIFICATION SYSTEM (Thông báo đẹp)
// =========================================================

function showToast(message, type = 'info') {
    if ($('#toast-container').length === 0) {
        $('body').append('<div id="toast-container" class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 9999;"></div>');
    }

    const icons = {
        success: '<i class="fas fa-check-circle"></i>',
        error: '<i class="fas fa-exclamation-circle"></i>',
        warning: '<i class="fas fa-exclamation-triangle"></i>',
        info: '<i class="fas fa-info-circle"></i>'
    };

    const bgColors = {
        success: 'bg-success',
        error: 'bg-danger',
        warning: 'bg-warning',
        info: 'bg-info'
    };

    const toastId = 'toast-' + Date.now();
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center text-white ${bgColors[type] || 'bg-info'} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    ${icons[type] || ''} ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    $('#toast-container').append(toastHtml);
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, { delay: 3000 });
    toast.show();

    $(toastElement).on('hidden.bs.toast', function () {
        $(this).remove();
    });
}

// =========================================================
// 2. EVENT DELEGATION (Xử lý sự kiện tập trung)
// =========================================================

// Dùng cú pháp $(function() {}) thay cho $(document).ready(...) để tránh lỗi TS6385
$(function () {

    // ===== PRODUCT DETAILS PAGE =====

    // Tăng/giảm số lượng
    $(document).on('click', '[data-action="update-qty"]', function () {
        const delta = parseInt($(this).data('delta')) || 0;
        const input = $('#qty');
        const max = parseInt(input.attr('max')) || 999;
        const currentVal = parseInt(input.val()) || 1;
        let newVal = currentVal + delta;

        if (newVal < 1) newVal = 1;
        if (newVal > max) newVal = max;

        input.val(newVal);
    });

    // Đổi ảnh thumbnail
    $(document).on('click', '[data-action="change-image"]', function () {
        const src = $(this).data('image-src');
        if (src) {
            $('#mainImage').attr('src', src).hide().fadeIn(200);
            $('.thumb-img').removeClass('border-primary');
            $(this).addClass('border-primary');
        }
    });

    // Submit order (Mua ngay / Thêm vào giỏ từ chi tiết)
    $(document).on('click', '[data-action="submit-order"]', function () {
        const type = $(this).data('order-type');
        const pId = $("input[name='productID']").val();
        const qty = $("#qty").val();
        const btn = $(this);

        if (!pId || !qty) {
            showToast('Vui lòng chọn sản phẩm và số lượng!', 'warning');
            return;
        }

        btn.addClass("btn-loading");

        $.ajax({
            url: "/Order/AddToCart",
            type: 'POST',
            data: { productID: pId, quantity: qty },
            success: function (response) {
                btn.removeClass("btn-loading");

                if (response.success === false || response.code === 0) {
                    showToast(response.message || 'Có lỗi xảy ra!', 'error');
                    return;
                }

                if (response.cartItemCount !== undefined) {
                    $("#cartItemCount").text(response.cartItemCount);
                }

                if (type === 'buyNow') {
                    showToast('Đang chuyển đến giỏ hàng...', 'info');
                    setTimeout(() => {
                        window.location.href = "/Order/ShoppingCart";
                    }, 500);
                } else {
                    showToast(`Đã thêm ${qty} sản phẩm vào giỏ hàng!`, 'success');
                }
            },
            error: function (xhr) {
                btn.removeClass("btn-loading");
                handleLoginRedirect(xhr);
            }
        });
    });

    // ===== PRODUCT LIST PAGE (Trang chủ/Danh sách) =====

    // Thêm vào giỏ nhanh
    $(document).on('click', '[data-action="add-to-cart"]', function () {
        const productId = $(this).data('product-id');
        const btn = $(this);

        if (!productId) {
            showToast('Không tìm thấy sản phẩm!', 'error');
            return;
        }

        btn.prop('disabled', true);
        const originalHtml = btn.html();
        btn.html('<i class="fas fa-spinner fa-spin"></i>');

        $.ajax({
            url: "/Order/AddToCart",
            type: "POST",
            data: { productID: productId, quantity: 1 },
            success: function (response) {
                btn.prop('disabled', false);
                btn.html(originalHtml);

                if (response.success === false || response.code === 0) {
                    showToast(response.message || 'Có lỗi xảy ra!', 'error');
                    return;
                }

                if (response.cartItemCount !== undefined) {
                    $("#cartItemCount").text(response.cartItemCount);
                }
                showToast('Đã thêm sản phẩm vào giỏ hàng!', 'success');
            },
            error: function (xhr) {
                btn.prop('disabled', false);
                btn.html(originalHtml);
                handleLoginRedirect(xhr);
            }
        });
    });

    // ===== SHOPPING CART PAGE (Đã sửa lỗi không cập nhật) =====

    // Cập nhật số lượng trong giỏ
    $(document).on('click', '[data-action="update-cart-qty"]', function () {
        const productId = $(this).data('product-id');
        const delta = parseInt($(this).data('delta')) || 0;
        const input = $("#qty-" + productId);

        if (!productId || !input.length) {
            // Nếu không tìm thấy input, có thể do chưa đặt ID trong View.
            // Nhớ kiểm tra bước sửa View bên dưới!
            return;
        }

        input.prop('disabled', true);

        $.ajax({
            url: "/Order/UpdateQuantity",
            type: "POST",
            data: { id: productId, quantity: delta },
            success: function (result) {
                input.prop('disabled', false);

                if (result.success === false || result.code === 0) {
                    showToast(result.message || 'Có lỗi xảy ra!', 'error');
                    return;
                }

                // Code 1: Cập nhật thành công
                if (result.code === 1) {
                    // Cập nhật input
                    const currentVal = parseInt(input.val()) || 0;
                    input.val(currentVal + delta);

                    // Cập nhật Thành tiền (Item Total)
                    if (result.itemTotal) {
                        const totalElement = $("#total-" + productId);
                        if (totalElement.length) {
                            totalElement.text(result.itemTotal)
                                .css('color', '#ed7117')
                                .animate({ color: '#333' }, 1000);
                        }
                    }

                    // Cập nhật Tổng tiền giỏ hàng
                    if (result.cartTotal) {
                        $("#cartTotalMoney").text(result.cartTotal);
                    }

                    // Cập nhật icon giỏ hàng
                    if (result.cartQty !== undefined) {
                        $("#cartItemCount").text(result.cartQty);
                    }

                    showToast('Cập nhật thành công!', 'success');
                }
                // Code 2: Đã xóa sản phẩm (số lượng về 0)
                else if (result.code === 2) {
                    showToast(result.message || 'Đã xóa sản phẩm!', 'info');
                    setTimeout(() => {
                        window.location.reload();
                    }, 1000);
                }
            },
            error: function () {
                input.prop('disabled', false);
                showToast('Có lỗi kết nối server!', 'error');
            }
        });
    });

    // ===== PRODUCT REVIEW =====

    $(document).on('click', '[data-action="post-review"]', function () {
        const form = $('#formReview');
        const comment = $('#txtComment').val();
        const btn = $(this);

        if (comment.trim() === "") {
            showToast('Vui lòng nhập nội dung đánh giá!', 'warning');
            return;
        }

        btn.prop('disabled', true);
        const originalHtml = btn.html();
        btn.html('<i class="fas fa-spinner fa-spin"></i> Đang gửi...');

        $.ajax({
            url: "/Product/Review",
            type: "POST",
            data: form.serialize(),
            success: function (response) {
                btn.prop('disabled', false);
                btn.html(originalHtml);

                if (response.success) {
                    showToast(response.message || 'Cảm ơn bạn đã đánh giá!', 'success');
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                } else {
                    if (response.code === -1) {
                        window.location.href = "/Account/Login?returnUrl=" + window.location.pathname;
                    } else {
                        showToast(response.message || 'Có lỗi xảy ra!', 'error');
                    }
                }
            },
            error: function () {
                btn.prop('disabled', false);
                btn.html(originalHtml);
                showToast('Có lỗi xảy ra khi gửi đánh giá!', 'error');
            }
        });
    });

    // ===== AUTO SUBMIT & CONFIRM =====

    $(document).on('change', '[data-auto-submit]', function () {
        $(this).closest('form')[0].submit();
    });

    $(document).on('click', '[data-confirm]', function (e) {
        const message = $(this).data('confirm');
        if (!confirm(message)) {
            e.preventDefault();
            return false;
        }
    });

}); 

// ===== CONTACT FORM (Xử lý liên hệ) =====
$(document).on('submit', '#contactForm', function (e) {
    e.preventDefault(); // Chặn submit mặc định

    const form = $(this);
    const btn = form.find('button[type="submit"]');

    // Lấy giá trị các ô input (cần thêm name attribute bên view)
    const fullName = form.find('input[name="FullName"]').val().trim();
    const email = form.find('input[name="Email"]').val().trim();
    const message = form.find('textarea[name="Message"]').val().trim();

    // 1. Validate dữ liệu
    if (!fullName || !email || !message) {
        showToast('Vui lòng điền đầy đủ các trường bắt buộc!', 'warning');
        return;
    }

    // Validate Email bằng Regex chuẩn
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
        showToast('Địa chỉ Email không hợp lệ!', 'warning');
        return;
    }

    if (message.length < 10) {
        showToast('Nội dung tin nhắn quá ngắn (tối thiểu 10 ký tự).', 'warning');
        return;
    }

    // 2. Hiệu ứng Loading
    const originalHtml = btn.html();
    btn.prop('disabled', true).addClass('btn-loading');
    btn.html('<i class="fas fa-spinner fa-spin"></i> Đang gửi...');

    // 3. Giả lập gửi API (Delay 1.5 giây cho giống thật)
    setTimeout(() => {
        btn.prop('disabled', false).removeClass('btn-loading');
        btn.html(originalHtml);

        // Hiện thông báo thành công
        showToast('Gửi tin nhắn thành công! Chúng tôi sẽ liên hệ lại sớm nhất.', 'success');

        // Reset form
        form[0].reset();
    }, 1500);
});

// =========================================================
// 3. HÀM TIỆN ÍCH CHUNG
// =========================================================

function handleLoginRedirect(xhr) {
    if (xhr.status === 401) {
        showToast('Vui lòng đăng nhập để tiếp tục!', 'warning');
        setTimeout(() => {
            const returnUrl = window.location.pathname + window.location.search;
            window.location.href = "/Account/Login?returnUrl=" + encodeURIComponent(returnUrl);
        }, 1500);
    } else {
        showToast('Có lỗi xảy ra. Vui lòng thử lại!', 'error');
    }
}