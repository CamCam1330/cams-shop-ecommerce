# 🛒 E-Commerce Platform & Management System

> Một hệ thống thương mại điện tử hoàn chỉnh bao gồm ứng dụng mua sắm cho khách hàng (Shop) và trang quản trị (Admin). Dự án được xây dựng trên nền tảng **.NET 9.0** với kiến trúc **Multi-Layer Architecture**, sử dụng **Dapper** cho tầng truy cập dữ liệu.

## 🚀 Tech Stack

* **Framework/Platform:** .NET 9.0 (ASP.NET Core MVC)
* **Architecture:** N-Tier / Multi-Layer Architecture
* **Database Access:** Dapper (Micro-ORM), Microsoft.Data.SqlClient
* **Authentication:** Cookie Authentication & Session Management
* **Localization:** Cấu hình văn hóa mặc định `vi-VN`

## 🏗️ Architecture & Project Structure

Giải pháp (Solution) được tổ chức theo kiến trúc phân lớp, tách biệt rõ ràng giữa tầng Giao diện (Presentation), tầng Nghiệp vụ (Business) và tầng Dữ liệu (Data):

```text
SV22T1020670.sln
├── Applications/ (Presentation Layer)
│   ├── SV22T1020670.Admin/        # Web App: Giao diện quản trị viên (yêu cầu phân quyền)
│   └── SV22T1020670.Shop/         # Web App: Giao diện cửa hàng thương mại điện tử
│
└── Libraries/ (Business & Data Layers)
    ├── SV22T1020670.BusinessLayers/ # Services xử lý logic nghiệp vụ
    ├── SV22T1020670.DataLayers/     # Data Access Layer giao tiếp CSDL qua Dapper
    └── SV22T1020670.DomainModels/   # Domain Entities (Product, Order, Customer...)
```
## ✨ Key Features

**🛍️ Shop Application (Dành cho khách hàng)**
* **Duyệt & Tìm kiếm:** Lọc sản phẩm theo danh mục và khoảng giá.
* **Chi tiết sản phẩm:** Xem thông tin chi tiết và đánh giá (review) sản phẩm.
* **Giỏ hàng (Shopping Cart):** Thêm/sửa/xóa sản phẩm trong giỏ hàng (quản lý qua Session).
* **Thanh toán (Checkout):** Xác nhận đơn hàng.
* **Quản lý cá nhân:** Xem lịch sử mua hàng và trạng thái đơn hàng.

**⚙️ Admin Application (Dành cho Ban quản trị)**
* **Phân quyền (Authorization):** Bảo mật các endpoint qua `[Authorize]` và Cookie Authentication.
* **Quản lý Đơn hàng:** Tìm kiếm, lập đơn, duyệt đơn, chuyển giao hàng, hủy hoặc xóa đơn hàng.
* **Quản lý Sản phẩm & Quảng cáo:** CRUD thông tin sản phẩm, banner quảng cáo.
* **Quản lý Đối tác:** Quản lý thông tin nhà cung cấp (Supplier) và đơn vị vận chuyển (Shipper).

## 📸 Screenshots

<img width="1915" height="1006" alt="image" src="https://github.com/user-attachments/assets/33c953a2-9ce1-4267-9c15-f1adeb1f2b8a" />
<img width="1919" height="1002" alt="image" src="https://github.com/user-attachments/assets/b6649b07-5f07-4f1a-88b7-fa67f7270cbc" />

## 🛠️ Getting Started

### Prerequisites (Yêu cầu hệ thống)
* .NET 9.0 SDK trở lên.
* SQL Server (LocalDB hoặc bản đầy đủ).
* Visual Studio 2022 (khuyến nghị) hoặc Visual Studio Code.

### Installation & Setup

1. **Clone repository:**
   ```bash
   git clone [https://github.com/CamCam1330/cams-shop-ecommerce.git](https://github.com/CamCam1330/cams-shop-ecommerce.git)

2. Thiết lập Cơ sở dữ liệu:
  Tìm file script SQL trong thư mục dự án.

  Chạy script trên SQL Server Management Studio (SSMS) để tạo database và dữ liệu mẫu.

3. Cấu hình chuỗi kết nối (Connection String):

  Mở file appsettings.json trong cả 2 project SV22T1020670.Admin và SV22T1020670.Shop.

  Cập nhật ConnectionString trỏ tới CSDL SQL Server của bạn. Chuỗi kết nối này sẽ được BusinessLayers khởi tạo và tiêm vào các DAL.

## 👨‍💻 Author
Họ và tên: Thân Hoàng Phước Minh

Mã sinh viên: 22T1020670
