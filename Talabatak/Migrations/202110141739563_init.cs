namespace Talabatak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        Visiable = c.Boolean(nullable: false),
                        ImageUrl = c.String(),
                        SortingNumber = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Stores",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        PhoneNumber = c.String(),
                        SeenCount = c.Int(nullable: false),
                        DescriptionAr = c.String(),
                        DescriptionEn = c.String(),
                        AddressAr = c.String(),
                        AddressEn = c.String(),
                        LogoImageUrl = c.String(),
                        CoverImageUrl = c.String(),
                        IsClosingManual = c.Boolean(nullable: false),
                        Is24HourOpen = c.Boolean(nullable: false),
                        OpenFrom = c.Time(precision: 7),
                        OpenTo = c.Time(precision: 7),
                        IsOpen = c.Boolean(nullable: false),
                        Latitude = c.Double(),
                        Longitude = c.Double(),
                        DeliveryBySystem = c.Boolean(nullable: false),
                        StoreOrdersDeliveryOpenFarePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StoreOrdersDeliveryOpenFareKilometers = c.Double(nullable: false),
                        StoreOrdersDeliveryEveryKilometerPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CategoryId = c.Long(nullable: false),
                        CityId = c.Long(nullable: false),
                        IsAccepted = c.Boolean(),
                        IsHidden = c.Boolean(nullable: false),
                        IsBlocked = c.Boolean(nullable: false),
                        AcceptedOn = c.DateTime(),
                        RejectedOn = c.DateTime(),
                        Profit = c.Double(nullable: false),
                        RejectionReason = c.String(),
                        Rate = c.Double(nullable: false),
                        SortingNumber = c.Int(nullable: false),
                        Wallet = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ResponsiblePersonEmail = c.String(),
                        ResponsiblePersonPhoneNumber = c.String(),
                        ResponsiblePersonName = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.Cities", t => t.CityId, cascadeDelete: true)
                .Index(t => t.CategoryId)
                .Index(t => t.CityId);
            
            CreateTable(
                "dbo.Cities",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CountryId = c.Long(nullable: false),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Countries", t => t.CountryId, cascadeDelete: true)
                .Index(t => t.CountryId);
            
            CreateTable(
                "dbo.Countries",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        ImageUrl = c.String(),
                        TimeZoneId = c.String(),
                        PhoneCode = c.String(),
                        CurrencyAr = c.String(),
                        CurrencyEn = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OtlobAy7agaOrder",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CityId = c.Long(nullable: false),
                        OrderStatus = c.Int(nullable: false),
                        Code = c.String(),
                        Details = c.String(),
                        ImageUrl = c.String(),
                        DeliveryProfit = c.Double(nullable: false),
                        DeliveryFees = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SubTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsPaid = c.Boolean(nullable: false),
                        UserId = c.String(maxLength: 128),
                        DriverId = c.Long(),
                        EstimatedDeliverDistance = c.Int(nullable: false),
                        EstimatedDeliverTimeInSeconds = c.Int(nullable: false),
                        UserAddressId = c.Long(),
                        PaymentMethod = c.Int(nullable: false),
                        IsRefundRequired = c.Boolean(nullable: false),
                        FinishCode = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Drivers", t => t.DriverId)
                .ForeignKey("dbo.UserAddresses", t => t.UserAddressId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.Cities", t => t.CityId, cascadeDelete: true)
                .Index(t => t.CityId)
                .Index(t => t.UserId)
                .Index(t => t.DriverId)
                .Index(t => t.UserAddressId);
            
            CreateTable(
                "dbo.OtlobAy7agaOrderChat",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        FromUserId = c.String(nullable: false, maxLength: 128),
                        ToUserId = c.String(nullable: false, maxLength: 128),
                        Message = c.String(),
                        OrderId = c.Long(nullable: false),
                        AttachmentName = c.String(),
                        AttachmentFileType = c.Int(),
                        AttachmentUrl = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ToUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.FromUserId)
                .ForeignKey("dbo.OtlobAy7agaOrder", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.FromUserId)
                .Index(t => t.ToUserId)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        ImageUrl = c.String(),
                        VerificationCode = c.Int(nullable: false),
                        ForgotPasswordGUID = c.String(),
                        Latitude = c.Double(),
                        Longitude = c.Double(),
                        Wallet = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CityId = c.Long(),
                        RegisterationType = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        PaymentUniqueKey = c.String(),
                        AmountToBePaid = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Cities", t => t.CityId)
                .Index(t => t.CityId)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Complaints",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Message = c.String(),
                        UserId = c.String(maxLength: 128),
                        IsViewed = c.Boolean(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.DriverReviews",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        DriverId = c.Long(nullable: false),
                        UserId = c.String(maxLength: 128),
                        Rate = c.Int(nullable: false),
                        Review = c.String(),
                        StoreOrderId = c.Long(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StoreOrders", t => t.StoreOrderId)
                .ForeignKey("dbo.Drivers", t => t.DriverId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.DriverId)
                .Index(t => t.UserId)
                .Index(t => t.StoreOrderId);
            
            CreateTable(
                "dbo.Drivers",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        VehicleTypeId = c.Long(),
                        VehicleNumber = c.String(),
                        VehicleColor = c.String(),
                        PersonalPhoto = c.String(),
                        IdentityPhoto = c.String(),
                        Profit = c.Double(nullable: false),
                        LicensePhoto = c.String(),
                        VehicleLicensePhoto = c.String(),
                        RejectionReason = c.String(),
                        AcceptedOn = c.DateTime(),
                        IsBlocked = c.Boolean(nullable: false),
                        IsOnline = c.Boolean(nullable: false),
                        IsAccepted = c.Boolean(),
                        IsAvailable = c.Boolean(nullable: false),
                        Rate = c.Double(nullable: false),
                        NumberOfCompletedTrips = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.VehicleTypes", t => t.VehicleTypeId)
                .Index(t => t.UserId)
                .Index(t => t.VehicleTypeId);
            
            CreateTable(
                "dbo.StoreOrderDrivers",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        OrderId = c.Long(nullable: false),
                        DriverId = c.Long(nullable: false),
                        IsAccepted = c.Boolean(),
                        AcceptedOn = c.DateTime(),
                        RejectedOn = c.DateTime(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Drivers", t => t.DriverId, cascadeDelete: true)
                .ForeignKey("dbo.StoreOrders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId)
                .Index(t => t.DriverId);
            
            CreateTable(
                "dbo.StoreOrders",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        PaymentUniqueKey = c.String(),
                        DeliveryFees = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SubTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Code = c.String(),
                        StoreProfit = c.Double(nullable: false),
                        DeliveryProfit = c.Double(nullable: false),
                        PaymentMethod = c.Int(nullable: false),
                        IsRefundRequired = c.Boolean(nullable: false),
                        IsPaid = c.Boolean(nullable: false),
                        Status = c.Int(nullable: false),
                        IsDeliveryFeesUpdated = c.Boolean(nullable: false),
                        UserAddressId = c.Long(),
                        DriverId = c.Long(),
                        DeliveredOn = c.DateTime(),
                        UserId = c.String(maxLength: 128),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Drivers", t => t.DriverId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.UserAddresses", t => t.UserAddressId)
                .Index(t => t.UserAddressId)
                .Index(t => t.DriverId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.StoreOrderChats",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        FromUserId = c.String(nullable: false, maxLength: 128),
                        ToUserId = c.String(nullable: false, maxLength: 128),
                        Message = c.String(),
                        OrderId = c.Long(nullable: false),
                        AttachmentName = c.String(),
                        AttachmentFileType = c.Int(),
                        AttachmentUrl = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StoreOrders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ToUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.FromUserId)
                .Index(t => t.FromUserId)
                .Index(t => t.ToUserId)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.StoreOrderItems",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ProductId = c.Long(nullable: false),
                        SizeId = c.Long(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Quantity = c.Int(nullable: false),
                        SubTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OrderId = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StoreOrders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.ProductSizes", t => t.SizeId)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId)
                .Index(t => t.SizeId)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        DescriptionAr = c.String(),
                        DescriptionEn = c.String(),
                        IsMultipleSize = c.Boolean(nullable: false),
                        SingleOriginalPrice = c.Decimal(precision: 18, scale: 2),
                        SingleOfferPrice = c.Decimal(precision: 18, scale: 2),
                        CategoryId = c.Long(nullable: false),
                        Rate = c.Double(nullable: false),
                        Currency = c.String(),
                        CurrencyAr = c.String(),
                        Inventory = c.Double(nullable: false),
                        SellCounter = c.Int(nullable: false),
                        SortingNumber = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProductCategories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.ProductCategories",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        StoreId = c.Long(nullable: false),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        ImageUrl = c.String(),
                        SortingNumber = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Stores", t => t.StoreId, cascadeDelete: true)
                .Index(t => t.StoreId);
            
            CreateTable(
                "dbo.ProductImages",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ImageUrl = c.String(),
                        ProductId = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.ProductSizes",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ProductId = c.Long(nullable: false),
                        SizeAr = c.String(),
                        SizeEn = c.String(),
                        OriginalPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OfferPrice = c.Decimal(precision: 18, scale: 2),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.PaymentHistories",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        IsStoreOrder = c.Boolean(nullable: false),
                        StoreOrderId = c.Long(),
                        OtlobAy7agaOrderId = c.Long(),
                        TransactionId = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OtlobAy7agaOrder", t => t.OtlobAy7agaOrderId)
                .ForeignKey("dbo.StoreOrders", t => t.StoreOrderId)
                .Index(t => t.StoreOrderId)
                .Index(t => t.OtlobAy7agaOrderId);
            
            CreateTable(
                "dbo.StoreReviews",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        StoreId = c.Long(nullable: false),
                        UserId = c.String(maxLength: 128),
                        Rate = c.Int(nullable: false),
                        Review = c.String(),
                        StoreOrderId = c.Long(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Stores", t => t.StoreId, cascadeDelete: true)
                .ForeignKey("dbo.StoreOrders", t => t.StoreOrderId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.StoreId)
                .Index(t => t.UserId)
                .Index(t => t.StoreOrderId);
            
            CreateTable(
                "dbo.UserAddresses",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        Latitude = c.Double(),
                        Longitude = c.Double(),
                        Address = c.String(),
                        AddressInDetails = c.String(),
                        Floor = c.String(),
                        BuildingNumber = c.String(),
                        Apartment = c.String(),
                        PhoneNumber = c.String(),
                        UserId = c.String(maxLength: 128),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserWallets",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        TransactionAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TransactionType = c.Int(nullable: false),
                        StoreOrderId = c.Long(),
                        OtlobOrderId = c.Long(),
                        AttachmentUrl = c.String(),
                        TransactionWay = c.String(),
                        TransactionId = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OtlobAy7agaOrder", t => t.OtlobOrderId)
                .ForeignKey("dbo.StoreOrders", t => t.StoreOrderId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.StoreOrderId)
                .Index(t => t.OtlobOrderId);
            
            CreateTable(
                "dbo.OtlobAy7agaOrderDriver",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        OrderId = c.Long(nullable: false),
                        DriverId = c.Long(nullable: false),
                        IsAccepted = c.Boolean(),
                        AcceptedOn = c.DateTime(),
                        RejectedOn = c.DateTime(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Drivers", t => t.DriverId, cascadeDelete: true)
                .ForeignKey("dbo.OtlobAy7agaOrder", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId)
                .Index(t => t.DriverId);
            
            CreateTable(
                "dbo.VehicleTypes",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.JobOrders",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        FinishCode = c.Int(nullable: false),
                        Code = c.String(),
                        UserId = c.String(maxLength: 128),
                        Name = c.String(),
                        PhoneNumber = c.String(),
                        Address = c.String(),
                        WorkerProfit = c.Double(nullable: false),
                        AddressInDetails = c.String(),
                        Latitude = c.Double(),
                        Longitude = c.Double(),
                        Building = c.String(),
                        Floor = c.String(),
                        Apartment = c.String(),
                        Description = c.String(),
                        Status = c.Int(nullable: false),
                        IsUserReviewed = c.Boolean(nullable: false),
                        UserReviewRate = c.Int(),
                        UserReviewDescription = c.String(),
                        UserReviewDate = c.DateTime(),
                        TotalPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        WorkerId = c.Long(nullable: false),
                        JobId = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Jobs", t => t.JobId, cascadeDelete: true)
                .ForeignKey("dbo.Workers", t => t.WorkerId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.WorkerId)
                .Index(t => t.JobId);
            
            CreateTable(
                "dbo.JobOrderChats",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        FromUserId = c.String(nullable: false, maxLength: 128),
                        ToUserId = c.String(nullable: false, maxLength: 128),
                        Message = c.String(),
                        OrderId = c.Long(nullable: false),
                        AttachmentName = c.String(),
                        AttachmentFileType = c.Int(),
                        AttachmentUrl = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.JobOrders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ToUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.FromUserId)
                .Index(t => t.FromUserId)
                .Index(t => t.ToUserId)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.JobOrderImages",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ImageUrl = c.String(),
                        OrderId = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.JobOrders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.Jobs",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        ImageUrl = c.String(),
                        NumberOfOrders = c.Int(nullable: false),
                        SortingNumber = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.JobWorkers",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        JobId = c.Long(nullable: false),
                        WorkerId = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Jobs", t => t.JobId, cascadeDelete: true)
                .ForeignKey("dbo.Workers", t => t.WorkerId, cascadeDelete: true)
                .Index(t => t.JobId)
                .Index(t => t.WorkerId);
            
            CreateTable(
                "dbo.Workers",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        DescriptionAr = c.String(),
                        DescriptionEn = c.String(),
                        IdentityPhoto = c.String(),
                        RejectionReason = c.String(),
                        AcceptedOn = c.DateTime(),
                        IsBlocked = c.Boolean(nullable: false),
                        IsAccepted = c.Boolean(),
                        Rate = c.Double(nullable: false),
                        Profit = c.Double(nullable: false),
                        NumberOfOrders = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Title = c.String(),
                        Body = c.String(),
                        IsSeen = c.Boolean(nullable: false),
                        IsWorker = c.Boolean(nullable: false),
                        RequestId = c.Long(nullable: false),
                        NotificationType = c.Int(nullable: false),
                        NotificationLink = c.String(),
                        UserId = c.String(maxLength: 128),
                        StoreId = c.Long(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Stores", t => t.StoreId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.StoreId);
            
            CreateTable(
                "dbo.PushTokens",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Token = c.String(),
                        OS = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        IsWorker = c.Boolean(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.StoreUsers",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        StoreId = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Stores", t => t.StoreId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.StoreId);
            
            CreateTable(
                "dbo.SecondSliders",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        SortingNumber = c.Int(nullable: false),
                        StoreId = c.Long(nullable: false),
                        ImagePath = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Stores", t => t.StoreId, cascadeDelete: true)
                .Index(t => t.StoreId);
            
            CreateTable(
                "dbo.Sliders",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        SortingNumber = c.Int(nullable: false),
                        StoreId = c.Long(nullable: false),
                        ImagePath = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Stores", t => t.StoreId, cascadeDelete: true)
                .Index(t => t.StoreId);
            
            CreateTable(
                "dbo.MetaDatas",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Email = c.String(),
                        Facebook = c.String(),
                        WhatsApp = c.String(),
                        Twitter = c.String(),
                        LinkedIn = c.String(),
                        Instagram = c.String(),
                        Address = c.String(),
                        Phone = c.String(),
                        Latitude = c.Double(),
                        Longitude = c.Double(),
                        Fax = c.String(),
                        StoreOrdersDeliveryOpenFarePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StoreOrdersDeliveryOpenFareKilometers = c.Double(nullable: false),
                        StoreOrdersDeliveryEveryKilometerPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OtlobAy7agaDeliveryOpenFarePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OtlobAy7agaDeliveryOpenFareKilometers = c.Double(nullable: false),
                        OtlobAy7agaDeliveryEveryKilometerPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OtlobAy7agaDeliveryStaticExtraFees = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NumberOfAvailableOrdersPerDriver = c.Int(nullable: false),
                        NumberOfMinutesBeforeCancellingOrderWithoutDriver = c.Int(nullable: false),
                        WalletMinimumAmountToCharge = c.Decimal(nullable: false, precision: 18, scale: 2),
                        WalletMaximumAmountToCharge = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MobileVersions",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Version = c.String(),
                        IsUpdateRequired = c.Boolean(nullable: false),
                        OS = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.TermsAndConditions",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Terms_Conditions = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Sliders", "StoreId", "dbo.Stores");
            DropForeignKey("dbo.SecondSliders", "StoreId", "dbo.Stores");
            DropForeignKey("dbo.Stores", "CityId", "dbo.Cities");
            DropForeignKey("dbo.OtlobAy7agaOrder", "CityId", "dbo.Cities");
            DropForeignKey("dbo.OtlobAy7agaOrderChat", "OrderId", "dbo.OtlobAy7agaOrder");
            DropForeignKey("dbo.StoreUsers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.StoreUsers", "StoreId", "dbo.Stores");
            DropForeignKey("dbo.StoreOrderChats", "FromUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OtlobAy7agaOrderChat", "FromUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.JobOrderChats", "FromUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.StoreOrderChats", "ToUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OtlobAy7agaOrderChat", "ToUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.JobOrderChats", "ToUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PushTokens", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OtlobAy7agaOrder", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Notifications", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Notifications", "StoreId", "dbo.Stores");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.JobOrders", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Workers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.JobWorkers", "WorkerId", "dbo.Workers");
            DropForeignKey("dbo.JobOrders", "WorkerId", "dbo.Workers");
            DropForeignKey("dbo.JobWorkers", "JobId", "dbo.Jobs");
            DropForeignKey("dbo.JobOrders", "JobId", "dbo.Jobs");
            DropForeignKey("dbo.JobOrderImages", "OrderId", "dbo.JobOrders");
            DropForeignKey("dbo.JobOrderChats", "OrderId", "dbo.JobOrders");
            DropForeignKey("dbo.DriverReviews", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Drivers", "VehicleTypeId", "dbo.VehicleTypes");
            DropForeignKey("dbo.Drivers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OtlobAy7agaOrderDriver", "OrderId", "dbo.OtlobAy7agaOrder");
            DropForeignKey("dbo.OtlobAy7agaOrderDriver", "DriverId", "dbo.Drivers");
            DropForeignKey("dbo.DriverReviews", "DriverId", "dbo.Drivers");
            DropForeignKey("dbo.UserWallets", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserWallets", "StoreOrderId", "dbo.StoreOrders");
            DropForeignKey("dbo.UserWallets", "OtlobOrderId", "dbo.OtlobAy7agaOrder");
            DropForeignKey("dbo.UserAddresses", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.StoreOrders", "UserAddressId", "dbo.UserAddresses");
            DropForeignKey("dbo.OtlobAy7agaOrder", "UserAddressId", "dbo.UserAddresses");
            DropForeignKey("dbo.StoreOrders", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.StoreReviews", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.StoreReviews", "StoreOrderId", "dbo.StoreOrders");
            DropForeignKey("dbo.StoreReviews", "StoreId", "dbo.Stores");
            DropForeignKey("dbo.PaymentHistories", "StoreOrderId", "dbo.StoreOrders");
            DropForeignKey("dbo.PaymentHistories", "OtlobAy7agaOrderId", "dbo.OtlobAy7agaOrder");
            DropForeignKey("dbo.StoreOrderItems", "ProductId", "dbo.Products");
            DropForeignKey("dbo.StoreOrderItems", "SizeId", "dbo.ProductSizes");
            DropForeignKey("dbo.ProductSizes", "ProductId", "dbo.Products");
            DropForeignKey("dbo.ProductImages", "ProductId", "dbo.Products");
            DropForeignKey("dbo.ProductCategories", "StoreId", "dbo.Stores");
            DropForeignKey("dbo.Products", "CategoryId", "dbo.ProductCategories");
            DropForeignKey("dbo.StoreOrderItems", "OrderId", "dbo.StoreOrders");
            DropForeignKey("dbo.StoreOrderDrivers", "OrderId", "dbo.StoreOrders");
            DropForeignKey("dbo.DriverReviews", "StoreOrderId", "dbo.StoreOrders");
            DropForeignKey("dbo.StoreOrders", "DriverId", "dbo.Drivers");
            DropForeignKey("dbo.StoreOrderChats", "OrderId", "dbo.StoreOrders");
            DropForeignKey("dbo.StoreOrderDrivers", "DriverId", "dbo.Drivers");
            DropForeignKey("dbo.OtlobAy7agaOrder", "DriverId", "dbo.Drivers");
            DropForeignKey("dbo.Complaints", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "CityId", "dbo.Cities");
            DropForeignKey("dbo.Cities", "CountryId", "dbo.Countries");
            DropForeignKey("dbo.Stores", "CategoryId", "dbo.Categories");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Sliders", new[] { "StoreId" });
            DropIndex("dbo.SecondSliders", new[] { "StoreId" });
            DropIndex("dbo.StoreUsers", new[] { "StoreId" });
            DropIndex("dbo.StoreUsers", new[] { "UserId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.PushTokens", new[] { "UserId" });
            DropIndex("dbo.Notifications", new[] { "StoreId" });
            DropIndex("dbo.Notifications", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.Workers", new[] { "UserId" });
            DropIndex("dbo.JobWorkers", new[] { "WorkerId" });
            DropIndex("dbo.JobWorkers", new[] { "JobId" });
            DropIndex("dbo.JobOrderImages", new[] { "OrderId" });
            DropIndex("dbo.JobOrderChats", new[] { "OrderId" });
            DropIndex("dbo.JobOrderChats", new[] { "ToUserId" });
            DropIndex("dbo.JobOrderChats", new[] { "FromUserId" });
            DropIndex("dbo.JobOrders", new[] { "JobId" });
            DropIndex("dbo.JobOrders", new[] { "WorkerId" });
            DropIndex("dbo.JobOrders", new[] { "UserId" });
            DropIndex("dbo.OtlobAy7agaOrderDriver", new[] { "DriverId" });
            DropIndex("dbo.OtlobAy7agaOrderDriver", new[] { "OrderId" });
            DropIndex("dbo.UserWallets", new[] { "OtlobOrderId" });
            DropIndex("dbo.UserWallets", new[] { "StoreOrderId" });
            DropIndex("dbo.UserWallets", new[] { "UserId" });
            DropIndex("dbo.UserAddresses", new[] { "UserId" });
            DropIndex("dbo.StoreReviews", new[] { "StoreOrderId" });
            DropIndex("dbo.StoreReviews", new[] { "UserId" });
            DropIndex("dbo.StoreReviews", new[] { "StoreId" });
            DropIndex("dbo.PaymentHistories", new[] { "OtlobAy7agaOrderId" });
            DropIndex("dbo.PaymentHistories", new[] { "StoreOrderId" });
            DropIndex("dbo.ProductSizes", new[] { "ProductId" });
            DropIndex("dbo.ProductImages", new[] { "ProductId" });
            DropIndex("dbo.ProductCategories", new[] { "StoreId" });
            DropIndex("dbo.Products", new[] { "CategoryId" });
            DropIndex("dbo.StoreOrderItems", new[] { "OrderId" });
            DropIndex("dbo.StoreOrderItems", new[] { "SizeId" });
            DropIndex("dbo.StoreOrderItems", new[] { "ProductId" });
            DropIndex("dbo.StoreOrderChats", new[] { "OrderId" });
            DropIndex("dbo.StoreOrderChats", new[] { "ToUserId" });
            DropIndex("dbo.StoreOrderChats", new[] { "FromUserId" });
            DropIndex("dbo.StoreOrders", new[] { "UserId" });
            DropIndex("dbo.StoreOrders", new[] { "DriverId" });
            DropIndex("dbo.StoreOrders", new[] { "UserAddressId" });
            DropIndex("dbo.StoreOrderDrivers", new[] { "DriverId" });
            DropIndex("dbo.StoreOrderDrivers", new[] { "OrderId" });
            DropIndex("dbo.Drivers", new[] { "VehicleTypeId" });
            DropIndex("dbo.Drivers", new[] { "UserId" });
            DropIndex("dbo.DriverReviews", new[] { "StoreOrderId" });
            DropIndex("dbo.DriverReviews", new[] { "UserId" });
            DropIndex("dbo.DriverReviews", new[] { "DriverId" });
            DropIndex("dbo.Complaints", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUsers", new[] { "CityId" });
            DropIndex("dbo.OtlobAy7agaOrderChat", new[] { "OrderId" });
            DropIndex("dbo.OtlobAy7agaOrderChat", new[] { "ToUserId" });
            DropIndex("dbo.OtlobAy7agaOrderChat", new[] { "FromUserId" });
            DropIndex("dbo.OtlobAy7agaOrder", new[] { "UserAddressId" });
            DropIndex("dbo.OtlobAy7agaOrder", new[] { "DriverId" });
            DropIndex("dbo.OtlobAy7agaOrder", new[] { "UserId" });
            DropIndex("dbo.OtlobAy7agaOrder", new[] { "CityId" });
            DropIndex("dbo.Cities", new[] { "CountryId" });
            DropIndex("dbo.Stores", new[] { "CityId" });
            DropIndex("dbo.Stores", new[] { "CategoryId" });
            DropTable("dbo.TermsAndConditions");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.MobileVersions");
            DropTable("dbo.MetaDatas");
            DropTable("dbo.Sliders");
            DropTable("dbo.SecondSliders");
            DropTable("dbo.StoreUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.PushTokens");
            DropTable("dbo.Notifications");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.Workers");
            DropTable("dbo.JobWorkers");
            DropTable("dbo.Jobs");
            DropTable("dbo.JobOrderImages");
            DropTable("dbo.JobOrderChats");
            DropTable("dbo.JobOrders");
            DropTable("dbo.VehicleTypes");
            DropTable("dbo.OtlobAy7agaOrderDriver");
            DropTable("dbo.UserWallets");
            DropTable("dbo.UserAddresses");
            DropTable("dbo.StoreReviews");
            DropTable("dbo.PaymentHistories");
            DropTable("dbo.ProductSizes");
            DropTable("dbo.ProductImages");
            DropTable("dbo.ProductCategories");
            DropTable("dbo.Products");
            DropTable("dbo.StoreOrderItems");
            DropTable("dbo.StoreOrderChats");
            DropTable("dbo.StoreOrders");
            DropTable("dbo.StoreOrderDrivers");
            DropTable("dbo.Drivers");
            DropTable("dbo.DriverReviews");
            DropTable("dbo.Complaints");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.OtlobAy7agaOrderChat");
            DropTable("dbo.OtlobAy7agaOrder");
            DropTable("dbo.Countries");
            DropTable("dbo.Cities");
            DropTable("dbo.Stores");
            DropTable("dbo.Categories");
        }
    }
}
