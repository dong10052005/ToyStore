# Hướng dẫn sử dụng hệ thống đăng nhập/đăng ký

## Tổng quan
Hệ thống đã được tích hợp đầy đủ chức năng đăng nhập và đăng ký cho 3 loại tài khoản:
- **Customer (Khách hàng)**: Có thể đăng ký và mua sắm
- **Staff (Nhân viên)**: Có thể đăng ký và quản lý hệ thống
- **Admin (Quản trị viên)**: Tài khoản mặc định, có quyền cao nhất

## Tài khoản mặc định
Khi khởi động ứng dụng lần đầu, hệ thống sẽ tự động tạo tài khoản admin:
- **Username**: admin
- **Password**: admin123

## Các tính năng đã được tích hợp

### 1. Đăng nhập (`/Auth/Login`)
- Hỗ trợ đăng nhập cho cả 3 loại tài khoản
- Sử dụng email cho Customer, username cho Admin/Staff
- Có tùy chọn "Ghi nhớ đăng nhập"
- Validation đầy đủ

### 2. Đăng ký (`/Auth/Register`)
- Khách hàng và nhân viên có thể đăng ký
- Admin chỉ có thể được tạo bởi hệ thống
- Validation email, mật khẩu, xác nhận mật khẩu
- Kiểm tra email/username trùng lặp

### 3. Đăng xuất (`/Auth/Logout`)
- Xóa toàn bộ session
- Redirect về trang chủ

### 4. Quản lý Session
- Session middleware tự động xử lý thông tin user
- Thời gian session: 30 phút
- Lưu trữ thông tin user trong HttpContext

### 5. Phân quyền truy cập
- **AuthHelper**: Các method tiện ích để kiểm tra quyền
- **AuthorizeRoleAttribute**: Attribute để bảo vệ các action
- Menu động dựa trên loại tài khoản

## Cách sử dụng

### 1. Đăng nhập
```csharp
// Kiểm tra user đã đăng nhập
if (AuthHelper.IsAuthenticated(HttpContext))
{
    var user = AuthHelper.GetCurrentUser(HttpContext);
    // Sử dụng thông tin user
}
```

### 2. Kiểm tra quyền
```csharp
// Kiểm tra quyền admin
if (AuthHelper.IsAdmin(HttpContext))
{
    // Code cho admin
}

// Kiểm tra quyền staff hoặc admin
if (AuthHelper.IsStaff(HttpContext))
{
    // Code cho staff và admin
}
```

### 3. Bảo vệ Action
```csharp
[AuthorizeRole("Admin", "Staff")]
public IActionResult ManageProducts()
{
    // Chỉ Admin và Staff mới truy cập được
}
```

### 4. Sử dụng trong View
```html
@if (Context.Session.GetString("IsAuthenticated") == "True")
{
    <p>Xin chào, @Context.Session.GetString("FullName")</p>
}
```

## Cấu trúc dữ liệu

### Customer
- CustomerId (PK)
- FullName
- Email (Unique)
- PasswordHash
- Phone
- Address
- CreatedAt

### Admin
- AdminId (PK)
- Username (Unique)
- PasswordHash
- FullName
- Role (Admin/Staff)

### UserSession
- UserId
- Username
- Email
- FullName
- UserType (Customer/Admin/Staff)
- Role
- IsAuthenticated

## Bảo mật
- Mật khẩu được hash bằng SHA256
- Session được bảo vệ bằng HttpOnly cookie
- Validation đầy đủ cho tất cả input
- Kiểm tra quyền truy cập ở mọi level

## Lưu ý
- Đảm bảo database đã được tạo và migration đã chạy
- Kiểm tra connection string trong appsettings.json
- Tài khoản admin mặc định sẽ được tạo tự động khi khởi động




