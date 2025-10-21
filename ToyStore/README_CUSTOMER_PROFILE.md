# Chức năng Chỉnh sửa Thông tin Cá nhân cho Customer

## Tổng quan
Chức năng này cho phép khách hàng (Customer) tự chỉnh sửa thông tin cá nhân của mình thông qua giao diện web.

## Các tính năng

### 1. Chỉnh sửa thông tin cơ bản
- **Họ và tên**: Cập nhật tên đầy đủ
- **Email**: Thay đổi địa chỉ email (kiểm tra trùng lặp)
- **Số điện thoại**: Cập nhật số điện thoại
- **Địa chỉ**: Thay đổi địa chỉ

### 2. Đổi mật khẩu (tùy chọn)
- Nhập mật khẩu hiện tại để xác thực
- Nhập mật khẩu mới
- Xác nhận mật khẩu mới
- Mật khẩu được hash bằng SHA256

## Cách sử dụng

### Truy cập chức năng
1. Đăng nhập với tài khoản Customer
2. Click vào tên người dùng ở góc phải trên
3. Chọn "Thông tin cá nhân" từ dropdown menu

### Chỉnh sửa thông tin
1. Điền thông tin mới vào các trường tương ứng
2. Để đổi mật khẩu:
   - Nhập mật khẩu hiện tại
   - Nhập mật khẩu mới
   - Xác nhận mật khẩu mới
3. Click "Cập nhật thông tin"

## Bảo mật

### Xác thực
- Chỉ Customer đã đăng nhập mới có thể truy cập
- Customer chỉ có thể chỉnh sửa thông tin của chính mình
- Kiểm tra quyền truy cập qua `AuthorizeRole("Customer")`

### Validation
- Kiểm tra email không trùng với tài khoản khác
- Xác thực mật khẩu hiện tại trước khi đổi
- Validation đầy đủ cho tất cả các trường

## Cấu trúc Code

### Models
- `EditCustomerViewModel`: ViewModel cho form chỉnh sửa

### Controllers
- `CustomersController.MyProfile()`: GET action hiển thị form
- `CustomersController.MyProfile(EditCustomerViewModel)`: POST action xử lý cập nhật

### Views
- `Views/Customers/MyProfile.cshtml`: Giao diện chỉnh sửa thông tin

### Services
- Sử dụng `IAuthService` để hash/verify password
- Đảm bảo tính nhất quán trong việc xử lý mật khẩu

## API Endpoints

```
GET /Customers/MyProfile
POST /Customers/MyProfile
```

## Lưu ý kỹ thuật

1. **Password Hashing**: Sử dụng SHA256 thông qua `IAuthService`
2. **Session Management**: Sử dụng `AuthHelper.GetCurrentUser()` để lấy thông tin user hiện tại
3. **Authorization**: Sử dụng `[AuthorizeRole("Customer")]` attribute
4. **Validation**: Client-side và server-side validation
5. **Error Handling**: Hiển thị thông báo lỗi chi tiết cho từng trường

## Testing

Chức năng được test tự động trong `TestAuthSystem.cs`:
- Test đăng ký Customer
- Test cập nhật thông tin Customer
- Verify dữ liệu được lưu đúng

## UI/UX

- Giao diện responsive với Bootstrap
- Icons Font Awesome
- Thông báo thành công/lỗi rõ ràng
- Form validation real-time
- Layout nhất quán với hệ thống

