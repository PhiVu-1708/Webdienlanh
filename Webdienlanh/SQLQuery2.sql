USE [master]
GO

-- 1. XÓA DATABASE CŨ NẾU ĐÃ TỒN TẠI ĐỂ LÀM LẠI SẠCH SẼ
IF DB_ID('DienLanhDB') IS NOT NULL
BEGIN
    ALTER DATABASE [DienLanhDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [DienLanhDB];
END
GO

-- 2. TẠO DATABASE MỚI
CREATE DATABASE [DienLanhDB]
GO
USE [DienLanhDB]
GO

-- ==========================================
-- 3. TẠO CÁC BẢNG ĐỘC LẬP (KHÔNG CÓ KHÓA NGOẠI)
-- ==========================================

-- Bảng Khách Hàng (Cũ)
CREATE TABLE [dbo].[Customers](
    [CustomerId] [int] IDENTITY(1,1) PRIMARY KEY,
    [FullName] [nvarchar](100) NOT NULL,
    [PhoneNumber] [varchar](15) NOT NULL,
    [Address] [nvarchar](255) NULL
)
GO

-- Bảng Dịch Vụ (Cũ)
CREATE TABLE [dbo].[Services](
    [ServiceId] [int] IDENTITY(1,1) PRIMARY KEY,
    [ServiceName] [nvarchar](100) NOT NULL,
    [BasePrice] [decimal](18, 2) NOT NULL,
    [IconClass] [varchar](50) NULL,
    [IsActive] [bit] DEFAULT 1
)
GO

-- Bảng Tài Khoản Đăng Nhập (Đã thêm tài khoản phivu)
CREATE TABLE [dbo].[UserAccounts](
    [Id] [int] IDENTITY(1,1) PRIMARY KEY,
    [Username] [nvarchar](max) NOT NULL,
    [Password] [nvarchar](max) NOT NULL,
    [Role] [nvarchar](max) NOT NULL,
    [FullName] [nvarchar](max) NOT NULL
)
GO

-- Bảng Hồ Sơ Kỹ Thuật Viên (Hiển thị ra cho khách xem)
CREATE TABLE [dbo].[Technicians](
    [Id] [int] IDENTITY(1,1) PRIMARY KEY,
    [Username] [nvarchar](max) NOT NULL,
    [Password] [nvarchar](max) NOT NULL,
    [FullName] [nvarchar](max) NOT NULL,
    [Phone] [nvarchar](max) NOT NULL,
    [CreatedAt] [datetime2](7) NOT NULL,
    [WalletBalance] [decimal](18, 2) NOT NULL DEFAULT ((10000000))
)
GO

-- ==========================================
-- 4. TẠO CÁC BẢNG CÓ KHÓA NGOẠI (PHỤ THUỘC)
-- ==========================================

-- Bảng Đặt Lịch (Cũ)
CREATE TABLE [dbo].[Bookings](
    [BookingId] [int] IDENTITY(1,1) PRIMARY KEY,
    [CustomerId] [int] NOT NULL FOREIGN KEY REFERENCES [dbo].[Customers]([CustomerId]),
    [BookingDate] [datetime] DEFAULT GETDATE(),
    [ServiceDate] [datetime] NOT NULL,
    [TotalAmount] [decimal](18, 2) DEFAULT 0,
    [Status] [nvarchar](50) DEFAULT N'Chờ xử lý',
    [Notes] [nvarchar](max) NULL
)
GO

-- Bảng Chi Tiết Đặt Lịch (Cũ)
CREATE TABLE [dbo].[BookingDetails](
    [DetailId] [int] IDENTITY(1,1) PRIMARY KEY,
    [BookingId] [int] NOT NULL FOREIGN KEY REFERENCES [dbo].[Bookings]([BookingId]) ON DELETE CASCADE,
    [ServiceId] [int] NOT NULL FOREIGN KEY REFERENCES [dbo].[Services]([ServiceId]),
    [Price] [decimal](18, 2) NOT NULL
)
GO

-- Bảng Đơn Hàng Mới (Có chức năng nối ID Kỹ Thuật Viên)
CREATE TABLE [dbo].[BookingOrders](
    [Id] [int] IDENTITY(1,1) PRIMARY KEY,
    [CustomerName] [nvarchar](max) NOT NULL,
    [Phone] [nvarchar](max) NOT NULL,
    [Address] [nvarchar](max) NOT NULL,
    [ServiceDate] [datetime2](7) NOT NULL,
    [Notes] [nvarchar](max) NULL,
    [SelectedServices] [nvarchar](max) NOT NULL,
    [CreatedAt] [datetime2](7) NOT NULL,
    [Status] [int] NOT NULL DEFAULT 0,
    [PaymentMethod] [nvarchar](50) NOT NULL DEFAULT N'Tiền mặt',
    [TechnicianId] [int] NULL FOREIGN KEY REFERENCES [dbo].[Technicians]([Id]) 
)
GO

-- BẢNG MỚI BỔ SUNG: Bảng Tin Nhắn Chat Trực Tuyến
CREATE TABLE [dbo].[Messages](
    [Id] [int] IDENTITY(1,1) PRIMARY KEY,
    [BookingOrderId] [int] NOT NULL FOREIGN KEY REFERENCES [dbo].[BookingOrders]([Id]) ON DELETE CASCADE,
    [SenderRole] [nvarchar](20) NOT NULL, -- 'Customer' hoặc 'Technician'
    [MessageText] [nvarchar](max) NOT NULL,
    [SentAt] [datetime2](7) NOT NULL DEFAULT GETDATE()
)
GO

-- Bảng Lịch Sử Migration
CREATE TABLE [dbo].[__EFMigrationsHistory](
    [MigrationId] [nvarchar](150) PRIMARY KEY,
    [ProductVersion] [nvarchar](32) NOT NULL
)
GO

-- ==========================================
-- 5. CHÈN DỮ LIỆU MẪU VÀO HỆ THỐNG
-- ==========================================

INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) 
VALUES (N'20260904165045_InitialCreate', N'8.0.0'), (N'20260905090426_AddUserTable', N'8.0.0'), (N'20260928044008_AddTechnicianToBookingOrder', N'8.0.0')
GO

SET IDENTITY_INSERT [dbo].[Services] ON 
INSERT [dbo].[Services] ([ServiceId], [ServiceName], [BasePrice], [IconClass], [IsActive]) VALUES (1, N'Vệ Sinh Máy Lạnh', 150000.00, N'bi-wind', 1)
INSERT [dbo].[Services] ([ServiceId], [ServiceName], [BasePrice], [IconClass], [IsActive]) VALUES (2, N'Bơm Gas Máy Lạnh', 250000.00, N'bi-speedometer2', 1)
INSERT [dbo].[Services] ([ServiceId], [ServiceName], [BasePrice], [IconClass], [IsActive]) VALUES (3, N'Sửa Chữa Tận Nhà', 0.00, N'bi-tools', 1)
INSERT [dbo].[Services] ([ServiceId], [ServiceName], [BasePrice], [IconClass], [IsActive]) VALUES (4, N'Lắp Đặt & Di Dời', 350000.00, N'bi-box-seam', 1)
SET IDENTITY_INSERT [dbo].[Services] OFF
GO

-- TẠO TÀI KHOẢN ĐĂNG NHẬP
SET IDENTITY_INSERT [dbo].[UserAccounts] ON 
INSERT [dbo].[UserAccounts] ([Id], [Username], [Password], [Role], [FullName]) VALUES (1, N'admin', N'123', N'admin', N'Quản trị viên')
INSERT [dbo].[UserAccounts] ([Id], [Username], [Password], [Role], [FullName]) VALUES (2, N'phivu', N'123', N'tho', N'Phạm Hồ Phi Vũ')
SET IDENTITY_INSERT [dbo].[UserAccounts] OFF
GO

-- TẠO HỒ SƠ THỢ
SET IDENTITY_INSERT [dbo].[Technicians] ON
INSERT [dbo].[Technicians] ([Id], [Username], [Password], [FullName], [Phone], [CreatedAt], [WalletBalance]) 
VALUES (2, N'phivu', N'123', N'Phạm Hồ Phi Vũ', N'0866450439', GETDATE(), 10000000)
SET IDENTITY_INSERT [dbo].[Technicians] OFF
GO

-- TẠO SẴN 1 ĐƠN HÀNG CHỜ NHẬN
SET IDENTITY_INSERT [dbo].[BookingOrders] ON 
INSERT [dbo].[BookingOrders] ([Id], [CustomerName], [Phone], [Address], [ServiceDate], [Notes], [SelectedServices], [CreatedAt], [Status], [PaymentMethod], [TechnicianId]) 
VALUES (1, N'Chị Mai', N'0899677266', N'242 Nguyễn Đức Thuận', GETDATE(), N'Máy lạnh không mát', N'Sửa Chữa Tận Nhà', GETDATE(), 0, N'Tiền mặt', NULL)
SET IDENTITY_INSERT [dbo].[BookingOrders] OFF
GO