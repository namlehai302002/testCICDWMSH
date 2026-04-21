using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Controllers;

namespace WMS.Tests;

public class AuthorizationMatrixTests
{
    [Theory]
    [InlineData(typeof(UsersController), nameof(UsersController.Index), new[] { "Admin" })]
    [InlineData(typeof(UsersController), nameof(UsersController.Create), new[] { "Admin" })]
    [InlineData(typeof(UsersController), nameof(UsersController.ResetPassword), new[] { "Admin" })]
    [InlineData(typeof(UsersController), nameof(UsersController.Delete), new[] { "Admin" })]
    [InlineData(typeof(CategoriesController), nameof(CategoriesController.Index), new[] { "Admin", "Manager" })]
    [InlineData(typeof(CategoriesController), nameof(CategoriesController.Create), new[] { "Admin", "Manager" })]
    [InlineData(typeof(CategoriesController), nameof(CategoriesController.Edit), new[] { "Admin", "Manager" })]
    [InlineData(typeof(CategoriesController), nameof(CategoriesController.Delete), new[] { "Admin" })]
    [InlineData(typeof(UnitsController), nameof(UnitsController.Index), new[] { "Admin", "Manager" })]
    [InlineData(typeof(UnitsController), nameof(UnitsController.Create), new[] { "Admin", "Manager" })]
    [InlineData(typeof(UnitsController), nameof(UnitsController.Delete), new[] { "Admin", "Manager" })]
    [InlineData(typeof(PartnersController), nameof(PartnersController.Index), new[] { "Admin", "Manager" })]
    [InlineData(typeof(PartnersController), nameof(PartnersController.Create), new[] { "Admin", "Manager" })]
    [InlineData(typeof(PartnersController), nameof(PartnersController.Edit), new[] { "Admin", "Manager" })]
    [InlineData(typeof(PartnersController), nameof(PartnersController.Delete), new[] { "Admin" })]
    [InlineData(typeof(WarehousesController), nameof(WarehousesController.Edit), new[] { "Admin", "Manager" })]
    [InlineData(typeof(WarehousesController), nameof(WarehousesController.Create), new[] { "Admin", "Manager" })]
    [InlineData(typeof(WarehousesController), nameof(WarehousesController.Delete), new[] { "Admin", "Manager" })]
    [InlineData(typeof(WarehousesController), nameof(WarehousesController.CreateZone), new[] { "Admin", "Manager" })]
    [InlineData(typeof(WarehousesController), nameof(WarehousesController.CreateZoneWithLocations), new[] { "Admin", "Manager" })]
    [InlineData(typeof(WarehousesController), nameof(WarehousesController.CreateLocation), new[] { "Admin", "Manager" })]
    [InlineData(typeof(WarehousesController), nameof(WarehousesController.FixData), new[] { "Admin" })]
    [InlineData(typeof(ItemsController), nameof(ItemsController.Create), new[] { "Admin", "Manager" })]
    [InlineData(typeof(ItemsController), nameof(ItemsController.Edit), new[] { "Admin", "Manager" })]
    [InlineData(typeof(ItemsController), nameof(ItemsController.Delete), new[] { "Admin" })]
    [InlineData(typeof(OperationsController), nameof(OperationsController.Waves), new[] { "Admin", "Manager", "Staff" })]
    [InlineData(typeof(OperationsController), nameof(OperationsController.PickTasks), new[] { "Admin", "Manager", "Staff" })]
    [InlineData(typeof(OperationsController), nameof(OperationsController.AssignTask), new[] { "Admin", "Manager" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.StockCount), new[] { "Admin", "Manager", "Staff" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.StockCountSaveDraft), new[] { "Admin", "Manager", "Staff" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.StockCountApproveDraft), new[] { "Admin", "Manager" })]
    [InlineData(typeof(VouchersController), nameof(VouchersController.Cancel), new[] { "Admin", "Manager" })]
    [InlineData(typeof(VouchersController), nameof(VouchersController.Create), new[] { "Admin", "Manager", "Staff" })]
    [InlineData(typeof(VouchersController), nameof(VouchersController.ConfirmForPicking), new[] { "Admin", "Manager" })]
    [InlineData(typeof(VouchersController), nameof(VouchersController.ConfirmPickTask), new[] { "Admin", "Manager", "Staff" })]
    [InlineData(typeof(VouchersController), nameof(VouchersController.PostReservedOutbound), new[] { "Admin", "Manager" })]
    [InlineData(typeof(VouchersController), nameof(VouchersController.Approve), new[] { "Admin", "Manager" })]
    [InlineData(typeof(VouchersController), nameof(VouchersController.UpdateInboundDefect), new[] { "Admin", "Manager" })]
    [InlineData(typeof(VouchersController), nameof(VouchersController.ReplenishDefect), new[] { "Admin", "Manager" })]
    [InlineData(typeof(VouchersController), nameof(VouchersController.AnalyzeReceipt), new[] { "Admin", "Manager", "Staff" })]
    [InlineData(typeof(VouchersController), nameof(VouchersController.DownloadImportTemplate), new[] { "Admin", "Manager", "Staff" })]
    [InlineData(typeof(VouchersController), nameof(VouchersController.DownloadDemoImport100), new[] { "Admin", "Manager", "Staff" })]
    [InlineData(typeof(VouchersController), nameof(VouchersController.ImportLinesExcel), new[] { "Admin", "Manager", "Staff" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.StockCountUnlockApproved), new[] { "Admin" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.PeriodLocks), new[] { "Admin", "Manager" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.SetPeriodLock), new[] { "Admin", "Manager" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.ClearPeriodLock), new[] { "Admin", "Manager" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.StockSnapshot), new[] { "Admin", "Manager" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.GenerateStockSnapshot), new[] { "Admin", "Manager" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.ExportStockSnapshot), new[] { "Admin", "Manager" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.AuditTrail), new[] { "Admin" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.Alerts), new[] { "Admin" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.RefreshExpiryAlerts), new[] { "Admin" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.ResolveAlert), new[] { "Admin" })]
    [InlineData(typeof(ReportsController), nameof(ReportsController.OpsKpi), new[] { "Admin", "Manager" })]
    public void CriticalActions_ShouldMatchExpectedRoleMatrix(Type controllerType, string actionName, string[] expectedRoles)
    {
        var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => string.Equals(m.Name, actionName, StringComparison.Ordinal))
            .Where(IsActionMethod)
            .ToList();

        Assert.NotEmpty(methods);
        foreach (var method in methods)
        {
            var actualRoles = GetEffectiveRoles(controllerType, method);
            Assert.Equal(expectedRoles.OrderBy(x => x), actualRoles.OrderBy(x => x));
        }
    }

    [Theory]
    [InlineData(typeof(HomeController), nameof(HomeController.Index))]
    [InlineData(typeof(ItemsController), nameof(ItemsController.Index))]
    [InlineData(typeof(ItemsController), nameof(ItemsController.Details))]
    [InlineData(typeof(WarehousesController), nameof(WarehousesController.Index))]
    [InlineData(typeof(WarehousesController), nameof(WarehousesController.Details))]
    [InlineData(typeof(WarehousesController), nameof(WarehousesController.InventoryMap))]
    [InlineData(typeof(VouchersController), nameof(VouchersController.Index))]
    [InlineData(typeof(VouchersController), nameof(VouchersController.Details))]
    [InlineData(typeof(ReportsController), nameof(ReportsController.Inventory))]
    [InlineData(typeof(ReportsController), nameof(ReportsController.StockMovement))]
    public void ReadOnlyActions_ShouldRequireAuthenticationAtMinimum(Type controllerType, string actionName)
    {
        var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => string.Equals(m.Name, actionName, StringComparison.Ordinal))
            .Where(IsActionMethod)
            .ToList();

        Assert.NotEmpty(methods);
        foreach (var method in methods)
        {
            var isAnonymous = method.GetCustomAttribute<AllowAnonymousAttribute>() != null
                || controllerType.GetCustomAttribute<AllowAnonymousAttribute>() != null;
            Assert.False(isAnonymous);
        }
    }

    [Fact]
    public void DangerousSystemActions_ShouldRequireAdminAndAntiForgery()
    {
        var controllerType = typeof(SystemController);
        var actionNames = new[]
        {
            nameof(SystemController.SeedData),
            nameof(SystemController.MergeLocationsPerLevel),
            nameof(SystemController.ResetDatabase)
        };

        foreach (var actionName in actionNames)
        {
            var method = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(m => m.Name == actionName && IsActionMethod(m));

            var roles = GetEffectiveRoles(controllerType, method);
            Assert.Equal(new[] { "Admin" }, roles);
            Assert.NotNull(method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
        }
    }

    [Theory]
    [InlineData(typeof(VouchersController), nameof(VouchersController.Cancel))]
    [InlineData(typeof(VouchersController), nameof(VouchersController.Approve))]
    [InlineData(typeof(VouchersController), nameof(VouchersController.PostReservedOutbound))]
    [InlineData(typeof(ReportsController), nameof(ReportsController.StockCountApproveDraft))]
    [InlineData(typeof(ReportsController), nameof(ReportsController.StockCountUnlockApproved))]
    [InlineData(typeof(UsersController), nameof(UsersController.Delete))]
    public void SensitivePostActions_ShouldUseAntiForgery(Type controllerType, string actionName)
    {
        var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == actionName && IsActionMethod(m))
            .ToList();

        Assert.NotEmpty(methods);
        foreach (var method in methods)
        {
            Assert.NotNull(method.GetCustomAttribute<HttpPostAttribute>());
            Assert.NotNull(method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
        }
    }

    private static bool IsActionMethod(MethodInfo method)
    {
        if (method.IsSpecialName) return false;
        if (method.GetCustomAttribute<NonActionAttribute>() != null) return false;
        return typeof(IActionResult).IsAssignableFrom(method.ReturnType)
            || (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)
                && typeof(IActionResult).IsAssignableFrom(method.ReturnType.GenericTypeArguments[0]));
    }

    private static string[] GetEffectiveRoles(Type controllerType, MethodInfo method)
    {
        var allowAnonymous = method.GetCustomAttribute<AllowAnonymousAttribute>() != null
            || controllerType.GetCustomAttribute<AllowAnonymousAttribute>() != null;
        if (allowAnonymous) return Array.Empty<string>();

        var methodAuth = method.GetCustomAttributes<AuthorizeAttribute>(true).ToList();
        if (methodAuth.Any(a => !string.IsNullOrWhiteSpace(a.Roles)))
            return ParseRoles(methodAuth);

        var classAuth = controllerType.GetCustomAttributes<AuthorizeAttribute>(true).ToList();
        return ParseRoles(classAuth);
    }

    private static string[] ParseRoles(IEnumerable<AuthorizeAttribute> attributes)
    {
        return attributes
            .SelectMany(a => (a.Roles ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x)
            .ToArray();
    }
}