# GCTWeb - Hướng dẫn Setup với Docker

## Yêu cầu
- .NET 8.0 SDK
- Docker Desktop (hoặc Docker Engine trên Linux)

## Các bước Setup lần đầu

### 1. Chạy MySQL Container

```powershell
docker run -d --name gctweb-mysql `
  -e MYSQL_ROOT_PASSWORD=root `
  -e MYSQL_DATABASE=GCTWeb `
  -p 3306:3306 `
  mysql:8.0
```

**Tham số:**
- `--name gctweb-mysql`: Đặt tên container để dễ quản lý
- `-e MYSQL_ROOT_PASSWORD=root`: Password cho user root
- `-e MYSQL_DATABASE=GCTWeb`: Tự động tạo database GCTWeb
- `-p 3306:3306`: Map port 3306 từ container ra host
- `-d`: Chạy container ở background

### 2. Kiểm tra Container đang chạy

```powershell
docker ps
```

Bạn sẽ thấy container `gctweb-mysql` với status `Up`.

### 3. Restore Dependencies

```powershell
dotnet restore
```

### 4. Cài đặt EF Core Tools (nếu chưa có)

```powershell
dotnet tool install --global dotnet-ef --version 8.0.11
```

### 5. Chạy Migrations để tạo Database Schema

```powershell
dotnet ef database update
```

Lệnh này sẽ:
- Tạo tất cả các bảng (Products, Orders, Users, Cart, etc.)
- Seed dữ liệu ban đầu (Roles: Admin, Staff, Customer)
- Tạo tài khoản admin mặc định

### 6. Chạy ứng dụng

```powershell
dotnet run --launch-profile https
```

Hoặc chỉ dùng HTTP:

```powershell
dotnet run --launch-profile http
```

### 7. Truy cập ứng dụng

- HTTPS: `https://localhost:7060`
- HTTP: `http://localhost:5190`

**Tài khoản Admin mặc định:**
- Email: `admin@gct.com`
- Password: `Admin@123`

## Quản lý Container hàng ngày

### Dừng MySQL Container

```powershell
docker stop gctweb-mysql
```

### Khởi động lại MySQL Container

```powershell
docker start gctweb-mysql
```

### Xem logs của Container

```powershell
docker logs gctweb-mysql
```

### Xem logs real-time

```powershell
docker logs -f gctweb-mysql
```

### Truy cập MySQL Shell

```powershell
docker exec -it gctweb-mysql mysql -uroot -proot
```

Sau đó bạn có thể chạy SQL commands:

```sql
USE GCTWeb;
SHOW TABLES;
SELECT * FROM AspNetUsers;
```

### Xóa Container (nếu muốn reset hoàn toàn)

```powershell
# Dừng container
docker stop gctweb-mysql

# Xóa container (dữ liệu sẽ mất)
docker rm gctweb-mysql

# Tạo lại container mới
docker run -d --name gctweb-mysql `
  -e MYSQL_ROOT_PASSWORD=root `
  -e MYSQL_DATABASE=GCTWeb `
  -p 3306:3306 `
  mysql:8.0

# Đợi vài giây rồi chạy lại migrations
Start-Sleep -Seconds 15
dotnet ef database update
```

## Persistence Data với Docker Volume (Khuyến nghị)

Để dữ liệu không bị mất khi xóa container, sử dụng volume:

```powershell
# Tạo volume
docker volume create gctweb-mysql-data

# Chạy container với volume
docker run -d --name gctweb-mysql `
  -e MYSQL_ROOT_PASSWORD=root `
  -e MYSQL_DATABASE=GCTWeb `
  -p 3306:3306 `
  -v gctweb-mysql-data:/var/lib/mysql `
  mysql:8.0
```

Giờ dữ liệu sẽ được lưu trong volume `gctweb-mysql-data` và không bị mất khi xóa container.

## Docker Compose (Tùy chọn)

Tạo file `docker-compose.yml`:

```yaml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    container_name: gctweb-mysql
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: GCTWeb
    ports:
      - "3306:3306"
    volumes:
      - gctweb-mysql-data:/var/lib/mysql
    restart: unless-stopped

volumes:
  gctweb-mysql-data:
```

Sử dụng:

```powershell
# Khởi động
docker-compose up -d

# Dừng
docker-compose down

# Xem logs
docker-compose logs -f
```

## Troubleshooting

### Port 3306 đã được sử dụng

Nếu bạn có MySQL cài sẵn trên máy:
- Dừng MySQL service local: `net stop MySQL80` (Windows)
- Hoặc đổi port mapping: `-p 3307:3306` và cập nhật connection string thành `server=localhost;port=3307;...`

### Container không start được

```powershell
# Xem logs lỗi
docker logs gctweb-mysql

# Xóa và tạo lại
docker rm -f gctweb-mysql
docker run -d --name gctweb-mysql -e MYSQL_ROOT_PASSWORD=root -e MYSQL_DATABASE=GCTWeb -p 3306:3306 mysql:8.0
```

### Kết nối database bị lỗi

Đảm bảo:
1. Container đang chạy: `docker ps`
2. Đợi MySQL khởi động hoàn toàn (15-20 giây sau khi start)
3. Connection string trong `appsettings.json` đúng:
   ```json
   "DefaultConnection": "server=localhost;database=GCTWeb;user=root;password=root;Allow User Variables=True"
   ```

## Backup & Restore Database

### Backup

```powershell
docker exec gctweb-mysql mysqldump -uroot -proot GCTWeb > backup.sql
```

### Restore

```powershell
Get-Content backup.sql | docker exec -i gctweb-mysql mysql -uroot -proot GCTWeb
```
