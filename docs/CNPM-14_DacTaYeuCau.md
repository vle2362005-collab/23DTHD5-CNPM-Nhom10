# CNPM-14 - Tài liệu đặc tả yêu cầu hệ thống

## 1. Thông tin chung

| Nội dung | Mô tả |
|---|---|
| Tên đề tài | Hệ thống web hỗ trợ dược sĩ ra quyết định bán thuốc an toàn |
| Nhóm thực hiện | Nhóm 10 |
| Công nghệ chính | Vue.js, ASP.NET Core Web API, SQL Server |
| Mục tiêu hệ thống | Hỗ trợ dược sĩ tiếp nhận khách hàng, quản lý thông tin thuốc, kiểm tra rủi ro an toàn và lưu lịch sử bán thuốc |
| Phạm vi sử dụng | Nhà thuốc, quầy thuốc, hệ thống nội bộ phục vụ dược sĩ và quản trị viên |

## 2. Mục tiêu tài liệu

Tài liệu này mô tả các yêu cầu chức năng, yêu cầu phi chức năng, phân quyền người dùng và các use case chính của hệ thống. Nội dung đặc tả được xây dựng dựa trên quy trình nghiệp vụ đã khảo sát ở CNPM-13, bao gồm tiếp nhận khách hàng, thu thập thông tin an toàn, kiểm tra dị ứng, bệnh nền, tương tác thuốc, tạo phiếu bán thuốc và lưu lịch sử cảnh báo.

Tài liệu này là cơ sở cho các task tiếp theo:

| Task | Nội dung sử dụng từ tài liệu này |
|---|---|
| CNPM-17, CNPM-18 | Thiết kế giao diện Vue.js |
| CNPM-19, CNPM-20 | Đăng nhập, phân quyền, quản lý người dùng |
| CNPM-21, CNPM-22 | Quản lý thuốc, hoạt chất, nhóm thuốc, cảnh báo |
| CNPM-23, CNPM-24 | Quản lý bệnh nhân, dị ứng, bệnh nền |
| CNPM-25, CNPM-26 | Kiểm tra dị ứng, chống chỉ định, tương tác thuốc |
| CNPM-27 | Tạo phiếu bán thuốc và lưu lịch sử |
| CNPM-28 | Báo cáo thống kê cảnh báo và doanh số |
| CNPM-29, CNPM-30 | Kiểm thử, triển khai, bàn giao hệ thống |

---

# CNPM-43 - Danh sách chức năng hệ thống

## 3.1. Nhóm chức năng tổng quan

Hệ thống được chia thành các nhóm chức năng chính sau:

| Mã nhóm | Nhóm chức năng | Mô tả |
|---|---|---|
| F01 | Xác thực và phân quyền | Đăng nhập, đăng xuất, kiểm soát quyền truy cập |
| F02 | Quản lý người dùng | Quản lý tài khoản dược sĩ, quản trị viên và vai trò |
| F03 | Quản lý thuốc | Quản lý thông tin thuốc, hoạt chất, hàm lượng, dạng bào chế |
| F04 | Quản lý dữ liệu an toàn thuốc | Quản lý dị ứng, chống chỉ định, tương tác thuốc, cảnh báo |
| F05 | Quản lý khách hàng/bệnh nhân | Lưu hồ sơ khách hàng, dị ứng, bệnh nền, thuốc đang dùng |
| F06 | Tiếp nhận yêu cầu mua thuốc | Ghi nhận khách hàng có đơn hoặc không có đơn thuốc |
| F07 | Tạo giỏ hàng thuốc | Chọn thuốc dự định bán, số lượng, liều dùng, hướng dẫn sử dụng |
| F08 | Kiểm tra an toàn bán thuốc | Kiểm tra dị ứng, bệnh nền, tương tác, đối tượng đặc biệt |
| F09 | Tạo phiếu bán thuốc | Lưu giao dịch bán thuốc, chi tiết thuốc, dược sĩ bán |
| F10 | Quản lý cảnh báo | Lưu và hiển thị cảnh báo an toàn phát sinh |
| F11 | Báo cáo thống kê | Thống kê số phiếu bán, thuốc bán, cảnh báo, doanh số |
| F12 | Quản trị hệ thống | Quản lý cấu hình, dữ liệu nền, phân quyền và trạng thái dữ liệu |

## 3.2. Danh sách chức năng chi tiết

### F01 - Xác thực và phân quyền

| Mã chức năng | Tên chức năng | Mô tả | Actor |
|---|---|---|---|
| F01.01 | Đăng nhập | Người dùng đăng nhập bằng email và mật khẩu | Admin, Dược sĩ, Quản lý |
| F01.02 | Đăng xuất | Người dùng thoát khỏi phiên làm việc | Admin, Dược sĩ, Quản lý |
| F01.03 | Kiểm tra phiên đăng nhập | Hệ thống kiểm tra token/phiên đăng nhập trước khi cho truy cập chức năng | Hệ thống |
| F01.04 | Phân quyền truy cập | Hệ thống giới hạn chức năng theo vai trò người dùng | Hệ thống |

### F02 - Quản lý người dùng

| Mã chức năng | Tên chức năng | Mô tả | Actor |
|---|---|---|---|
| F02.01 | Xem danh sách người dùng | Hiển thị danh sách tài khoản trong hệ thống | Admin |
| F02.02 | Thêm người dùng | Tạo tài khoản mới cho dược sĩ hoặc quản lý | Admin |
| F02.03 | Cập nhật người dùng | Sửa họ tên, email, vai trò, trạng thái tài khoản | Admin |
| F02.04 | Khóa/mở khóa tài khoản | Thay đổi trạng thái hoạt động của tài khoản | Admin |
| F02.05 | Gán vai trò | Gán vai trò Admin, Dược sĩ hoặc Quản lý | Admin |

### F03 - Quản lý thuốc

| Mã chức năng | Tên chức năng | Mô tả | Actor |
|---|---|---|---|
| F03.01 | Xem danh sách thuốc | Hiển thị danh sách thuốc đang quản lý | Admin, Dược sĩ |
| F03.02 | Tìm kiếm thuốc | Tìm theo tên thuốc, hoạt chất, nhóm thuốc | Admin, Dược sĩ |
| F03.03 | Thêm thuốc | Thêm thuốc mới vào danh mục | Admin |
| F03.04 | Cập nhật thuốc | Sửa thông tin thuốc, hoạt chất, hàm lượng, dạng bào chế | Admin |
| F03.05 | Ngừng sử dụng thuốc | Chuyển thuốc sang trạng thái ngừng bán/ngừng sử dụng | Admin |
| F03.06 | Xem chi tiết thuốc | Xem thông tin hoạt chất, liều dùng, chống chỉ định, tác dụng phụ | Admin, Dược sĩ |

### F04 - Quản lý dữ liệu an toàn thuốc

| Mã chức năng | Tên chức năng | Mô tả | Actor |
|---|---|---|---|
| F04.01 | Quản lý hoạt chất | Thêm, sửa, tra cứu hoạt chất thuốc | Admin |
| F04.02 | Quản lý nhóm thuốc | Quản lý phân loại thuốc theo nhóm điều trị | Admin |
| F04.03 | Quản lý tương tác thuốc | Lưu các cặp thuốc có tương tác và mức độ cảnh báo | Admin |
| F04.04 | Quản lý chống chỉ định | Lưu điều kiện bệnh nền hoặc đối tượng không phù hợp với thuốc | Admin |
| F04.05 | Quản lý cảnh báo theo đối tượng đặc biệt | Cấu hình cảnh báo cho trẻ em, người cao tuổi, phụ nữ có thai, cho con bú | Admin |
| F04.06 | Quản lý thuốc kê đơn | Đánh dấu thuốc yêu cầu đơn thuốc | Admin |

### F05 - Quản lý khách hàng/bệnh nhân

| Mã chức năng | Tên chức năng | Mô tả | Actor |
|---|---|---|---|
| F05.01 | Tìm kiếm khách hàng | Tìm khách hàng theo tên hoặc số điện thoại | Dược sĩ |
| F05.02 | Thêm hồ sơ khách hàng | Tạo hồ sơ khách hàng mới | Dược sĩ |
| F05.03 | Cập nhật hồ sơ khách hàng | Cập nhật tuổi, giới tính, số điện thoại, ghi chú | Dược sĩ |
| F05.04 | Ghi nhận dị ứng thuốc | Nhập hoạt chất hoặc nhóm thuốc khách hàng từng dị ứng | Dược sĩ |
| F05.05 | Ghi nhận bệnh nền | Nhập bệnh nền như cao huyết áp, tiểu đường, suy gan, suy thận | Dược sĩ |
| F05.06 | Ghi nhận thuốc đang sử dụng | Nhập thuốc khách hàng đang dùng để kiểm tra tương tác | Dược sĩ |
| F05.07 | Xem lịch sử mua thuốc | Xem các lần mua thuốc trước đó của khách hàng | Dược sĩ, Quản lý |

### F06 - Tiếp nhận yêu cầu mua thuốc

| Mã chức năng | Tên chức năng | Mô tả | Actor |
|---|---|---|---|
| F06.01 | Chọn luồng có đơn thuốc | Dược sĩ ghi nhận khách hàng có đơn thuốc | Dược sĩ |
| F06.02 | Chọn luồng không có đơn thuốc | Dược sĩ ghi nhận khách hàng mua theo triệu chứng | Dược sĩ |
| F06.03 | Nhập thông tin đơn thuốc | Nhập ngày kê đơn, bác sĩ kê đơn, thuốc trong đơn | Dược sĩ |
| F06.04 | Nhập triệu chứng | Ghi nhận triệu chứng, thời gian mắc bệnh, tình trạng hiện tại | Dược sĩ |
| F06.05 | Kiểm tra đơn thuốc hợp lệ | Kiểm tra thông tin đơn thuốc có đầy đủ và rõ ràng hay không | Dược sĩ |

### F07 - Tạo giỏ hàng thuốc

| Mã chức năng | Tên chức năng | Mô tả | Actor |
|---|---|---|---|
| F07.01 | Thêm thuốc vào giỏ hàng | Dược sĩ chọn thuốc dự định bán | Dược sĩ |
| F07.02 | Cập nhật số lượng | Sửa số lượng thuốc trong giỏ hàng | Dược sĩ |
| F07.03 | Nhập liều dùng | Nhập hướng dẫn dùng thuốc, số lần dùng, thời gian dùng | Dược sĩ |
| F07.04 | Xóa thuốc khỏi giỏ hàng | Loại thuốc khỏi danh sách dự định bán | Dược sĩ |
| F07.05 | Xem tổng quan giỏ hàng | Hiển thị danh sách thuốc, số lượng, liều dùng, cảnh báo liên quan | Dược sĩ |

### F08 - Kiểm tra an toàn bán thuốc

| Mã chức năng | Tên chức năng | Mô tả | Actor |
|---|---|---|---|
| F08.01 | Kiểm tra thiếu dữ liệu | Kiểm tra các trường bắt buộc như tuổi, dị ứng, bệnh nền, thuốc đang dùng | Hệ thống |
| F08.02 | Kiểm tra dị ứng | So khớp thuốc định bán với hoạt chất hoặc nhóm thuốc khách hàng dị ứng | Hệ thống |
| F08.03 | Kiểm tra chống chỉ định | Kiểm tra thuốc với bệnh nền hoặc tình trạng đặc biệt | Hệ thống |
| F08.04 | Kiểm tra tương tác thuốc | Kiểm tra tương tác giữa các thuốc trong giỏ hàng và thuốc đang dùng | Hệ thống |
| F08.05 | Kiểm tra đối tượng đặc biệt | Kiểm tra trẻ em, người cao tuổi, phụ nữ có thai, cho con bú | Hệ thống |
| F08.06 | Kiểm tra trùng hoạt chất/quá liều | Cảnh báo nguy cơ trùng hoạt chất hoặc vượt liều khuyến nghị | Hệ thống |
| F08.07 | Tổng hợp cảnh báo | Tổng hợp cảnh báo thành nhẹ, trung bình, nghiêm trọng | Hệ thống |
| F08.08 | Đưa ra khuyến nghị | Đề xuất cho phép bán, cần xác nhận, đổi thuốc hoặc không bán | Hệ thống |

### F09 - Tạo phiếu bán thuốc

| Mã chức năng | Tên chức năng | Mô tả | Actor |
|---|---|---|---|
| F09.01 | Tạo phiếu bán thuốc | Lưu thông tin khách hàng, dược sĩ, ngày bán và danh sách thuốc | Dược sĩ |
| F09.02 | Xác nhận cảnh báo | Dược sĩ xác nhận đã đọc và tư vấn cảnh báo nhẹ/trung bình | Dược sĩ |
| F09.03 | Hủy giao dịch | Hủy giao dịch khi có cảnh báo nghiêm trọng hoặc khách hàng không mua | Dược sĩ |
| F09.04 | Lưu chi tiết thuốc bán | Lưu thuốc, số lượng, liều dùng, hướng dẫn sử dụng | Hệ thống |
| F09.05 | Lưu kết quả kiểm tra an toàn | Lưu cảnh báo, mức độ, khuyến nghị và quyết định cuối cùng | Hệ thống |

### F10 - Quản lý cảnh báo

| Mã chức năng | Tên chức năng | Mô tả | Actor |
|---|---|---|---|
| F10.01 | Hiển thị cảnh báo | Hiển thị cảnh báo theo màu/mức độ | Hệ thống |
| F10.02 | Xem chi tiết cảnh báo | Xem lý do, thuốc liên quan, khuyến nghị xử lý | Dược sĩ |
| F10.03 | Lưu lịch sử cảnh báo | Lưu các cảnh báo phát sinh trong giao dịch | Hệ thống |
| F10.04 | Tra cứu cảnh báo cũ | Xem lại cảnh báo theo khách hàng hoặc phiếu bán | Dược sĩ, Quản lý |

### F11 - Báo cáo thống kê

| Mã chức năng | Tên chức năng | Mô tả | Actor |
|---|---|---|---|
| F11.01 | Thống kê số phiếu bán | Tổng hợp số lượng phiếu bán theo thời gian | Quản lý |
| F11.02 | Thống kê thuốc bán | Thống kê thuốc bán nhiều, số lượng bán | Quản lý |
| F11.03 | Thống kê cảnh báo | Thống kê số lượng cảnh báo theo mức độ và loại cảnh báo | Quản lý |
| F11.04 | Xem lịch sử bán thuốc | Xem danh sách phiếu bán và chi tiết giao dịch | Quản lý |
| F11.05 | Xuất báo cáo | Xuất dữ liệu báo cáo phục vụ theo dõi và đánh giá | Quản lý |

---

# CNPM-44 - Yêu cầu phi chức năng và phân quyền

## 4.1. Yêu cầu phi chức năng

### 4.1.1. Hiệu năng

| Mã yêu cầu | Nội dung |
|---|---|
| NF01 | Hệ thống phản hồi thao tác tra cứu danh sách trong thời gian chấp nhận được với dữ liệu demo |
| NF02 | Chức năng kiểm tra an toàn thuốc cần trả kết quả nhanh để không làm gián đoạn quá trình bán thuốc |
| NF03 | Hệ thống hỗ trợ tìm kiếm thuốc và khách hàng theo từ khóa |
| NF04 | Các danh sách dữ liệu cần có phân trang hoặc lọc dữ liệu để dễ sử dụng |

### 4.1.2. Bảo mật

| Mã yêu cầu | Nội dung |
|---|---|
| NF05 | Người dùng phải đăng nhập trước khi sử dụng hệ thống |
| NF06 | Mật khẩu không được lưu dạng văn bản thuần |
| NF07 | API cần kiểm tra quyền truy cập theo vai trò người dùng |
| NF08 | Người dùng không có quyền không được truy cập chức năng quản trị |
| NF09 | Dữ liệu khách hàng và lịch sử bán thuốc chỉ được truy cập bởi người có quyền |

### 4.1.3. Tính đúng đắn nghiệp vụ

| Mã yêu cầu | Nội dung |
|---|---|
| NF10 | Hệ thống phải kiểm tra dữ liệu đầu vào trước khi tạo phiếu bán thuốc |
| NF11 | Phiếu bán thuốc có cảnh báo nghiêm trọng không được xác nhận như giao dịch an toàn |
| NF12 | Cảnh báo phát sinh phải được lưu kèm phiếu bán thuốc |
| NF13 | Trạng thái quyết định cuối cùng phải phản ánh đúng kết quả xử lý của dược sĩ |

### 4.1.4. Tính dễ sử dụng

| Mã yêu cầu | Nội dung |
|---|---|
| NF14 | Giao diện cần rõ ràng, dễ thao tác cho dược sĩ trong quá trình bán thuốc |
| NF15 | Cảnh báo cần phân biệt mức độ nhẹ, trung bình, nghiêm trọng |
| NF16 | Các màn hình chính cần có chức năng tìm kiếm, lọc và xem chi tiết |
| NF17 | Màn hình bán thuốc cần hiển thị đồng thời thông tin khách hàng, giỏ hàng và cảnh báo |

### 4.1.5. Khả năng bảo trì và mở rộng

| Mã yêu cầu | Nội dung |
|---|---|
| NF18 | Backend cần tách controller, service, repository/model rõ ràng |
| NF19 | Frontend cần chia component theo từng màn hình chức năng |
| NF20 | Database cần có khóa chính, khóa ngoại và ràng buộc dữ liệu cần thiết |
| NF21 | Hệ thống có thể mở rộng thêm quy tắc cảnh báo mới trong tương lai |

## 4.2. Vai trò người dùng

| Vai trò | Mô tả |
|---|---|
| Admin | Quản trị tài khoản, danh mục thuốc, dữ liệu an toàn và cấu hình hệ thống |
| Dược sĩ | Tiếp nhận khách hàng, tạo phiếu bán thuốc, xem cảnh báo và quyết định bán thuốc |
| Quản lý | Theo dõi lịch sử bán thuốc, xem báo cáo thống kê và giám sát hoạt động |

## 4.3. Ma trận phân quyền

| Chức năng | Admin | Dược sĩ | Quản lý |
|---|---:|---:|---:|
| Đăng nhập/đăng xuất | Có | Có | Có |
| Quản lý người dùng | Có | Không | Không |
| Quản lý thuốc | Có | Xem | Xem |
| Quản lý hoạt chất, nhóm thuốc | Có | Xem | Xem |
| Quản lý tương tác thuốc | Có | Xem | Xem |
| Quản lý chống chỉ định | Có | Xem | Xem |
| Quản lý khách hàng | Có | Có | Xem |
| Ghi nhận dị ứng, bệnh nền | Có | Có | Xem |
| Tạo phiếu bán thuốc | Không | Có | Không |
| Kiểm tra an toàn thuốc | Có | Có | Xem |
| Xác nhận bán khi có cảnh báo | Không | Có | Không |
| Hủy giao dịch bán thuốc | Không | Có | Không |
| Xem lịch sử bán thuốc | Có | Có | Có |
| Xem báo cáo thống kê | Có | Không | Có |
| Cấu hình hệ thống | Có | Không | Không |

## 4.4. Quy định xử lý theo vai trò

- Admin có quyền quản lý dữ liệu nền nhưng không phải actor chính trong quy trình bán thuốc.
- Dược sĩ là actor chính thực hiện nghiệp vụ tiếp nhận, kiểm tra, tư vấn và bán thuốc.
- Quản lý chủ yếu theo dõi dữ liệu, lịch sử bán thuốc và báo cáo thống kê.
- Hệ thống phải kiểm tra quyền trước khi thực hiện các chức năng quan trọng.
- Chức năng xác nhận bán thuốc khi có cảnh báo chỉ dành cho dược sĩ đang trực tiếp bán thuốc.

---

# CNPM-45 - Mô tả use case chính của dược sĩ

## 5.1. Danh sách use case chính

| Mã use case | Tên use case | Actor chính |
|---|---|---|
| UC01 | Đăng nhập hệ thống | Dược sĩ |
| UC02 | Tiếp nhận khách hàng mua thuốc | Dược sĩ |
| UC03 | Tìm hoặc tạo hồ sơ khách hàng | Dược sĩ |
| UC04 | Ghi nhận thông tin an toàn của khách hàng | Dược sĩ |
| UC05 | Tạo giỏ hàng thuốc | Dược sĩ |
| UC06 | Kiểm tra an toàn thuốc | Dược sĩ, Hệ thống |
| UC07 | Xử lý cảnh báo an toàn | Dược sĩ |
| UC08 | Tạo phiếu bán thuốc | Dược sĩ |
| UC09 | Xem lịch sử mua thuốc của khách hàng | Dược sĩ |

## 5.2. UC01 - Đăng nhập hệ thống

| Nội dung | Mô tả |
|---|---|
| Actor chính | Dược sĩ |
| Mục tiêu | Truy cập vào hệ thống để thực hiện nghiệp vụ bán thuốc |
| Tiền điều kiện | Tài khoản dược sĩ đã được tạo và đang hoạt động |
| Hậu điều kiện | Dược sĩ đăng nhập thành công và được chuyển đến màn hình chính |

### Luồng chính

1. Dược sĩ mở hệ thống.
2. Dược sĩ nhập email và mật khẩu.
3. Hệ thống kiểm tra thông tin đăng nhập.
4. Hệ thống xác định vai trò người dùng.
5. Hệ thống chuyển dược sĩ đến màn hình dashboard.

### Luồng thay thế

| Trường hợp | Xử lý |
|---|---|
| Sai email hoặc mật khẩu | Hệ thống thông báo đăng nhập thất bại |
| Tài khoản bị khóa | Hệ thống thông báo tài khoản không được phép truy cập |
| Thiếu thông tin đăng nhập | Hệ thống yêu cầu nhập đầy đủ email và mật khẩu |

## 5.3. UC02 - Tiếp nhận khách hàng mua thuốc

| Nội dung | Mô tả |
|---|---|
| Actor chính | Dược sĩ |
| Actor phụ | Khách hàng |
| Mục tiêu | Ghi nhận nhu cầu mua thuốc và xác định luồng có đơn hoặc không có đơn |
| Tiền điều kiện | Dược sĩ đã đăng nhập |
| Hậu điều kiện | Hệ thống có thông tin ban đầu để chuyển sang bước nhập hồ sơ và kiểm tra an toàn |

### Luồng chính

1. Khách hàng đến nhà thuốc và trình bày nhu cầu.
2. Dược sĩ hỏi khách hàng có đơn thuốc hay không.
3. Nếu có đơn thuốc, dược sĩ kiểm tra thông tin đơn thuốc.
4. Nếu không có đơn thuốc, dược sĩ hỏi triệu chứng và thông tin sức khỏe cần thiết.
5. Dược sĩ chuyển sang bước nhập thông tin khách hàng.

### Luồng thay thế

| Trường hợp | Xử lý |
|---|---|
| Đơn thuốc không hợp lệ | Dược sĩ tư vấn khách hàng liên hệ lại bác sĩ |
| Khách hàng không cung cấp đủ thông tin | Dược sĩ ghi nhận thiếu dữ liệu và cân nhắc không bán thuốc có rủi ro |
| Nhu cầu vượt khả năng tư vấn | Dược sĩ khuyến nghị khách hàng đi khám |

## 5.4. UC03 - Tìm hoặc tạo hồ sơ khách hàng

| Nội dung | Mô tả |
|---|---|
| Actor chính | Dược sĩ |
| Mục tiêu | Xác định khách hàng đã có hồ sơ hay chưa |
| Tiền điều kiện | Dược sĩ đang ở màn hình bán thuốc |
| Hậu điều kiện | Hồ sơ khách hàng được chọn hoặc tạo mới |

### Luồng chính

1. Dược sĩ nhập số điện thoại hoặc tên khách hàng.
2. Hệ thống tìm kiếm hồ sơ khách hàng.
3. Nếu có hồ sơ, dược sĩ chọn hồ sơ phù hợp.
4. Nếu chưa có hồ sơ, dược sĩ tạo hồ sơ mới.
5. Hệ thống hiển thị thông tin khách hàng trên màn hình bán thuốc.

### Luồng thay thế

| Trường hợp | Xử lý |
|---|---|
| Có nhiều khách hàng trùng tên | Hệ thống hiển thị thêm số điện thoại để phân biệt |
| Thiếu số điện thoại | Hệ thống cho phép tạo hồ sơ nhưng đánh dấu thiếu thông tin |
| Khách hàng không muốn lưu hồ sơ | Dược sĩ có thể tạo giao dịch khách vãng lai nếu hệ thống hỗ trợ |

## 5.5. UC04 - Ghi nhận thông tin an toàn của khách hàng

| Nội dung | Mô tả |
|---|---|
| Actor chính | Dược sĩ |
| Mục tiêu | Thu thập thông tin phục vụ kiểm tra an toàn thuốc |
| Tiền điều kiện | Đã chọn hoặc tạo hồ sơ khách hàng |
| Hậu điều kiện | Dữ liệu an toàn của khách hàng được lưu hoặc cập nhật |

### Luồng chính

1. Dược sĩ nhập tuổi, giới tính, cân nặng nếu cần.
2. Dược sĩ nhập tiền sử dị ứng thuốc.
3. Dược sĩ nhập bệnh nền.
4. Dược sĩ nhập thuốc khách hàng đang sử dụng.
5. Dược sĩ ghi nhận tình trạng mang thai hoặc cho con bú nếu cần.
6. Hệ thống lưu thông tin an toàn vào hồ sơ khách hàng.

### Luồng thay thế

| Trường hợp | Xử lý |
|---|---|
| Khách hàng không nhớ dị ứng | Hệ thống ghi nhận chưa rõ tiền sử dị ứng |
| Khách hàng không cung cấp bệnh nền | Hệ thống đánh dấu thiếu dữ liệu an toàn |
| Khách hàng là trẻ em | Hệ thống yêu cầu nhập cân nặng để hỗ trợ kiểm tra liều |

## 5.6. UC05 - Tạo giỏ hàng thuốc

| Nội dung | Mô tả |
|---|---|
| Actor chính | Dược sĩ |
| Mục tiêu | Chọn danh sách thuốc dự định bán cho khách hàng |
| Tiền điều kiện | Đã có thông tin khách hàng |
| Hậu điều kiện | Giỏ hàng thuốc được tạo để kiểm tra an toàn |

### Luồng chính

1. Dược sĩ tìm kiếm thuốc.
2. Dược sĩ chọn thuốc cần bán.
3. Hệ thống tự động hiển thị hoạt chất, hàm lượng và dạng bào chế.
4. Dược sĩ nhập số lượng và liều dùng.
5. Dược sĩ thêm thuốc vào giỏ hàng.
6. Hệ thống hiển thị danh sách thuốc trong giỏ hàng.

### Luồng thay thế

| Trường hợp | Xử lý |
|---|---|
| Thuốc không còn hoạt động | Hệ thống không cho chọn thuốc hoặc hiển thị cảnh báo |
| Nhập số lượng không hợp lệ | Hệ thống yêu cầu nhập lại số lượng |
| Trùng thuốc trong giỏ hàng | Hệ thống cộng dồn số lượng hoặc yêu cầu xác nhận |

## 5.7. UC06 - Kiểm tra an toàn thuốc

| Nội dung | Mô tả |
|---|---|
| Actor chính | Hệ thống |
| Actor kích hoạt | Dược sĩ |
| Mục tiêu | Phát hiện rủi ro an toàn trước khi bán thuốc |
| Tiền điều kiện | Đã có thông tin khách hàng và giỏ hàng thuốc |
| Hậu điều kiện | Danh sách cảnh báo và khuyến nghị xử lý được tạo |

### Luồng chính

1. Dược sĩ bấm nút kiểm tra an toàn.
2. Hệ thống kiểm tra dữ liệu đầu vào.
3. Hệ thống kiểm tra dị ứng thuốc.
4. Hệ thống kiểm tra chống chỉ định theo bệnh nền.
5. Hệ thống kiểm tra tương tác thuốc.
6. Hệ thống kiểm tra đối tượng đặc biệt.
7. Hệ thống kiểm tra trùng hoạt chất và nguy cơ quá liều.
8. Hệ thống tổng hợp kết quả cảnh báo.
9. Hệ thống hiển thị kết quả cho dược sĩ.

### Luồng thay thế

| Trường hợp | Xử lý |
|---|---|
| Thiếu dữ liệu đầu vào | Hệ thống yêu cầu bổ sung thông tin |
| Có cảnh báo nghiêm trọng | Hệ thống khuyến nghị không bán thuốc hiện tại |
| Có cảnh báo nhẹ hoặc trung bình | Hệ thống yêu cầu dược sĩ xác nhận đã tư vấn |
| Không có cảnh báo | Hệ thống cho phép tạo phiếu bán thuốc |

## 5.8. UC07 - Xử lý cảnh báo an toàn

| Nội dung | Mô tả |
|---|---|
| Actor chính | Dược sĩ |
| Mục tiêu | Xử lý kết quả cảnh báo trước khi quyết định bán thuốc |
| Tiền điều kiện | Hệ thống đã kiểm tra an toàn và phát sinh kết quả |
| Hậu điều kiện | Cảnh báo được xác nhận, thuốc được đổi hoặc giao dịch bị hủy |

### Luồng chính

1. Dược sĩ xem danh sách cảnh báo.
2. Dược sĩ đọc mức độ và lý do cảnh báo.
3. Dược sĩ tư vấn lại cho khách hàng.
4. Nếu cảnh báo nhẹ hoặc trung bình, dược sĩ có thể xác nhận tiếp tục bán.
5. Nếu cảnh báo nghiêm trọng, dược sĩ không bán thuốc hiện tại.
6. Dược sĩ đổi thuốc hoặc hủy giao dịch nếu cần.
7. Hệ thống lưu quyết định xử lý.

### Luồng thay thế

| Trường hợp | Xử lý |
|---|---|
| Có thuốc thay thế an toàn | Dược sĩ chọn thuốc thay thế và kiểm tra lại |
| Không có thuốc thay thế | Dược sĩ tư vấn khách hàng đi khám hoặc liên hệ bác sĩ |
| Khách hàng không đồng ý mua | Hệ thống lưu trạng thái hủy giao dịch |

## 5.9. UC08 - Tạo phiếu bán thuốc

| Nội dung | Mô tả |
|---|---|
| Actor chính | Dược sĩ |
| Mục tiêu | Hoàn tất giao dịch bán thuốc |
| Tiền điều kiện | Thuốc đủ điều kiện bán hoặc dược sĩ đã xác nhận cảnh báo được phép bán |
| Hậu điều kiện | Phiếu bán thuốc và lịch sử kiểm tra an toàn được lưu |

### Luồng chính

1. Dược sĩ xác nhận tạo phiếu bán thuốc.
2. Hệ thống lưu thông tin khách hàng.
3. Hệ thống lưu danh sách thuốc bán.
4. Hệ thống lưu dược sĩ thực hiện giao dịch.
5. Hệ thống lưu cảnh báo nếu có.
6. Dược sĩ hướng dẫn khách hàng sử dụng thuốc.
7. Khách hàng thanh toán và nhận thuốc.

### Luồng thay thế

| Trường hợp | Xử lý |
|---|---|
| Có cảnh báo nghiêm trọng chưa xử lý | Hệ thống không cho tạo phiếu bán an toàn |
| Khách hàng hủy mua | Hệ thống ghi nhận giao dịch bị hủy |
| Lỗi lưu dữ liệu | Hệ thống thông báo lỗi và yêu cầu thử lại |

## 5.10. UC09 - Xem lịch sử mua thuốc của khách hàng

| Nội dung | Mô tả |
|---|---|
| Actor chính | Dược sĩ |
| Mục tiêu | Xem lại lịch sử mua thuốc và cảnh báo cũ của khách hàng |
| Tiền điều kiện | Khách hàng đã có hồ sơ trong hệ thống |
| Hậu điều kiện | Dược sĩ có thêm thông tin để tư vấn an toàn |

### Luồng chính

1. Dược sĩ mở hồ sơ khách hàng.
2. Hệ thống hiển thị danh sách lần mua thuốc trước.
3. Dược sĩ chọn một lần mua để xem chi tiết.
4. Hệ thống hiển thị thuốc đã bán, cảnh báo phát sinh và quyết định xử lý.

---

# CNPM-46 - Rà soát tài liệu đặc tả yêu cầu

## 6.1. Mục tiêu rà soát

Đảm bảo tài liệu đặc tả yêu cầu đã đầy đủ, nhất quán và có thể sử dụng làm cơ sở cho thiết kế, lập trình và kiểm thử.

## 6.2. Nội dung rà soát

| STT | Nội dung rà soát | Kết quả mong muốn |
|---:|---|---|
| 1 | Danh sách chức năng | Bao phủ đầy đủ các nghiệp vụ chính của hệ thống |
| 2 | Yêu cầu phi chức năng | Phù hợp với hệ thống web sử dụng Vue.js, ASP.NET Core Web API và SQL Server |
| 3 | Phân quyền | Vai trò Admin, Dược sĩ, Quản lý được mô tả rõ |
| 4 | Use case dược sĩ | Mô tả đúng luồng bán thuốc an toàn |
| 5 | Dữ liệu đầu vào | Có đủ thông tin khách hàng, thuốc, dị ứng, bệnh nền, giỏ hàng |
| 6 | Quy tắc cảnh báo | Phù hợp với nghiệp vụ kiểm tra an toàn đã khảo sát |
| 7 | Tính liên kết với Jira | Các chức năng có thể ánh xạ sang các task lập trình tiếp theo |
| 8 | Tính khả thi | Phù hợp với phạm vi đồ án môn học |

## 6.3. Kết quả rà soát

| Hạng mục | Trạng thái | Ghi chú |
|---|---|---|
| Chức năng hệ thống | Đạt | Đã xác định đầy đủ các nhóm chức năng chính |
| Phi chức năng | Đạt | Bao gồm hiệu năng, bảo mật, dễ sử dụng, bảo trì |
| Phân quyền | Đạt | Có ma trận quyền cho Admin, Dược sĩ, Quản lý |
| Use case dược sĩ | Đạt | Mô tả các use case chính của quy trình bán thuốc |
| Khả năng triển khai | Đạt | Có thể triển khai bằng Vue.js, ASP.NET Core Web API và SQL Server |
| Cơ sở cho lập trình | Đạt | Có thể dùng làm đầu vào cho các task code tiếp theo |

## 6.4. Kết luận

Tài liệu đặc tả yêu cầu đã mô tả đầy đủ các chức năng, yêu cầu phi chức năng, vai trò người dùng và use case chính của hệ thống. Nội dung tài liệu bám sát đề tài “Hệ thống web hỗ trợ dược sĩ ra quyết định bán thuốc an toàn” và phù hợp để sử dụng cho các bước thiết kế giao diện, thiết kế API, thiết kế cơ sở dữ liệu, lập trình chức năng và kiểm thử hệ thống.
