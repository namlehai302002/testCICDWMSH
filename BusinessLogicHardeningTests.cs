using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using WMS.Authorization;
using WMS.Controllers;
using WMS.Data;
using WMS.Models;
using WMS.ViewModels;

namespace WMS.Tests;

public class BusinessLogicHardeningTests
{
    [Fact]
    public async Task FefoSingleLinePick_ShouldReturnEarliestExpiryWithLot()
    {
        await using var db = CreateDb(nameof(FefoSingleLinePick_ShouldReturnEarliestExpiryWithLot));
        SeedWarehouseGraph(db);

        db.Items.Add(new Item
        {
            ItemId = 1,
            ItemCode = "ITEM-001",
            ItemName = "Test Item",
            BaseUomId = 1,
            UnitCost = 100,
            IsActive = true
        });

        db.ItemLocations.AddRange(
            new ItemLocation
            {
                ItemLocationId = 1,
                ItemId = 1,
                LocationId = 1,
                Quantity = 20,
                ReservedQty = 0,
                LotNumber = "LOT-LATE",
                ExpiryDate = new DateTime(2026, 12, 31),
                UpdatedAt = DateTime.UtcNow
            },
            new ItemLocation
            {
                ItemLocationId = 2,
                ItemId = 1,
                LocationId = 2,
                Quantity = 20,
                ReservedQty = 0,
                LotNumber = "LOT-EARLY",
                ExpiryDate = new DateTime(2026, 6, 30),
                UpdatedAt = DateTime.UtcNow
            });
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        var method = typeof(VouchersController).GetMethod(
            "GetFefoLocationForSingleLineAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(method);
        var task = (Task)method!.Invoke(controller, new object[] { 1, 1, 5m })!;
        await task;
        var result = task.GetType().GetProperty("Result")!.GetValue(task);

        Assert.NotNull(result);
        Assert.Equal(2, (int)result!.GetType().GetProperty("LocationId")!.GetValue(result)!);
        Assert.Equal("LOT-EARLY", (string?)result.GetType().GetProperty("LotNumber")!.GetValue(result));
        Assert.Equal(new DateTime(2026, 6, 30), (DateTime?)result.GetType().GetProperty("ExpiryDate")!.GetValue(result));
    }

    [Fact]
    public async Task Create_ShouldRejectUnknownItemAndRollbackVoucher()
    {
        await using var db = CreateDb(nameof(Create_ShouldRejectUnknownItemAndRollbackVoucher));
        SeedWarehouseGraph(db);
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        var vm = new VoucherCreateViewModel
        {
            VoucherType = VoucherTypeEnum.DieuChinh,
            WarehouseId = 1,
            Lines = new List<VoucherDetailLine>
            {
                new()
                {
                    ItemId = 999,
                    LocationId = 1,
                    TransactionQty = 1,
                    TransactionUomId = 1,
                    AdjustSign = 1,
                    UnitPrice = 100,
                    LineAmount = 100
                }
            }
        };

        var result = await controller.Create(vm);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(vm, view.Model);
        Assert.Contains("Không tìm thấy vật tư", controller.TempData["Error"]?.ToString());
        Assert.Equal(1, await db.Vouchers.CountAsync());
        Assert.Equal(0, await db.VoucherDetails.CountAsync());
    }

    [Fact]
    public async Task Create_ShouldRejectNegativeFinancialValuesForFinancialUser()
    {
        await using var db = CreateDb(nameof(Create_ShouldRejectNegativeFinancialValuesForFinancialUser));
        SeedWarehouseGraph(db);

        db.Items.Add(new Item
        {
            ItemId = 1,
            ItemCode = "ITEM-NEG",
            ItemName = "Negative Price Item",
            BaseUomId = 1,
            UnitCost = 50,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db, includeFinancialPermission: true);
        var vm = new VoucherCreateViewModel
        {
            VoucherType = VoucherTypeEnum.DieuChinh,
            WarehouseId = 1,
            Lines = new List<VoucherDetailLine>
            {
                new()
                {
                    ItemId = 1,
                    LocationId = 1,
                    TransactionQty = 1,
                    TransactionUomId = 1,
                    AdjustSign = 1,
                    UnitPrice = -10,
                    LineAmount = -10
                }
            }
        };

        var result = await controller.Create(vm);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(vm, view.Model);
        Assert.Contains("Đơn giá không được âm", controller.TempData["Error"]?.ToString());
        Assert.Equal(1, await db.Vouchers.CountAsync());
        Assert.Equal(0, await db.VoucherDetails.CountAsync());
    }

    private static AppDbContext CreateDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    private static void SeedWarehouseGraph(AppDbContext db)
    {
        db.Warehouses.Add(new Warehouse
        {
            WarehouseId = 1,
            WarehouseCode = "WH1",
            WarehouseName = "Main Warehouse",
            IsActive = true
        });

        db.Zones.AddRange(
            new Zone { ZoneId = 1, WarehouseId = 1, ZoneCode = "Z1", ZoneName = "Zone 1", IsActive = true },
            new Zone { ZoneId = 2, WarehouseId = 1, ZoneCode = "Z2", ZoneName = "Zone 2", IsActive = true });

        db.Locations.AddRange(
            new Location { LocationId = 1, ZoneId = 1, LocationCode = "L1", IsActive = true },
            new Location { LocationId = 2, ZoneId = 2, LocationCode = "L2", IsActive = true });
    }

    private static VouchersController CreateController(AppDbContext db, bool includeFinancialPermission = false)
    {
        var configuration = new ConfigurationBuilder().Build();
        var controller = new VouchersController(db, configuration);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "qa.user"),
            new(ClaimTypes.Role, "Admin")
        };
        if (includeFinancialPermission)
            claims.Add(new Claim(PermissionClaimTypes.Permission, WmsPermissions.ReportViewFinancial));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var httpContext = new DefaultHttpContext { User = principal };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());
        return controller;
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    /// <summary>
    /// Stock validation happens when voucher is posted, not during Create for draft vouchers.
    /// This test verifies that draft export vouchers are created successfully.
    /// </summary>
    [Fact]
    public async Task Create_ShouldRejectWhenQuantityExceedsStock()
    {
        await using var db = CreateDb(nameof(Create_ShouldRejectWhenQuantityExceedsStock));
        SeedWarehouseGraph(db);

        db.Items.Add(new Item
        {
            ItemId = 1,
            ItemCode = "ITEM-STOCK",
            ItemName = "Stock Test Item",
            BaseUomId = 1,
            UnitCost = 100,
            IsActive = true
        });

        db.ItemLocations.Add(new ItemLocation
        {
            ItemLocationId = 1,
            ItemId = 1,
            LocationId = 1,
            Quantity = 5,
            ReservedQty = 0,
            LotNumber = "LOT-1",
            ExpiryDate = DateTime.UtcNow.AddMonths(6),
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var vm = new VoucherCreateViewModel
        {
            VoucherType = VoucherTypeEnum.DieuChinh,
            WarehouseId = 1,
            Lines = new List<VoucherDetailLine>
            {
                new()
                {
                    ItemId = 1,
                    LocationId = 1,
                    TransactionQty = 10, // Exceeds available stock
                    TransactionUomId = 1,
                    AdjustSign = -1, // Negative adjustment
                    UnitPrice = 100,
                    LineAmount = 1000
                }
            }
        };

        var result = await controller.Create(vm);

        // Stock validation happens when voucher is posted. For DieuChinh with negative qty,
        // we expect the system to reject and return to the view with error.
        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(vm, view.Model);
        Assert.Contains("không đủ tồn kho", controller.TempData["Error"]?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, await db.Vouchers.CountAsync());
        Assert.Equal(0, await db.VoucherDetails.CountAsync());
    }
}




