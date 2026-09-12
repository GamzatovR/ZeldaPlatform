using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Features.Admin.Shop.Commands.CreateCategory;
using ZeldaArena.Application.Features.Admin.Shop.Commands.CreateProduct;
using ZeldaArena.Application.Features.Admin.Shop.Commands.DeleteCategory;
using ZeldaArena.Application.Features.Admin.Shop.Commands.DeleteProduct;
using ZeldaArena.Application.Features.Admin.Shop.Commands.RenameCategory;
using ZeldaArena.Application.Features.Admin.Shop.Commands.UpdateProduct;
using ZeldaArena.Application.Features.Admin.Shop.Queries.GetCategoriesForAdmin;
using ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductForEdit;
using ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductsForAdmin;
using ZeldaArena.Web.Areas.Admin.Models.Shop;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Extensions;

namespace ZeldaArena.Web.Areas.Admin.Controllers;

[Route("admin/products")]
[Authorize(Policy = PolicyNames.AdminOnly)]
public sealed class ProductsController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetProductsForAdminQuery filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(filter, cancellationToken);
        var categories = await sender.Send(new GetCategoriesForAdminQuery(), cancellationToken);

        return View(new ProductIndexViewModel { Filter = filter, Result = result, Categories = categories });
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken) =>
        View(new ProductCreateViewModel
        {
            Form = new ProductFormViewModel(),
            Categories = await sender.Send(new GetCategoriesForAdminQuery(), cancellationToken),
        });

    [HttpPost("create")]
    [RequestSizeLimit(ImageUploadRules.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ImageUploadRules.MaxRequestBytes)]
    public async Task<IActionResult> Create(
        [Bind(Prefix = nameof(ProductCreateViewModel.Form))] ProductFormViewModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (ModelState.IsValid)
        {
            await using var image = model.Image?.OpenReadStream();

            var result = await sender.Send(
                new CreateProductCommand
                {
                    Sku = model.Sku,
                    Name = model.Name,
                    CategoryId = model.CategoryId,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    Description = model.Description,
                    Image = model.Image.ToFileUpload(image),
                },
                cancellationToken);

            if (result.IsSuccess)
            {
                ReportSuccess(localizer["admin.product.created"].Value);

                return RedirectToAction(nameof(Edit), new { id = result.Value });
            }

            ModelState.AddResultError(result, localizer);
        }

        return View(new ProductCreateViewModel
        {
            Form = model,
            Categories = await sender.Send(new GetCategoriesForAdminQuery(), cancellationToken),
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var product = await sender.Send(new GetProductForEditQuery(id), cancellationToken);

        return product is null
            ? NotFound()
            : View(new ProductEditViewModel
            {
                Product = product,
                Form = ProductFormViewModel.From(product),
                Categories = await sender.Send(new GetCategoriesForAdminQuery(), cancellationToken),
            });
    }

    [HttpPost("{id:guid}")]
    [RequestSizeLimit(ImageUploadRules.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ImageUploadRules.MaxRequestBytes)]
    public async Task<IActionResult> Edit(
        Guid id,
        [Bind(Prefix = nameof(ProductEditViewModel.Form))] ProductFormViewModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        // Артикул задаётся один раз при создании, форма правки его не отправляет:
        // иначе Required на пустом поле молча отказывал бы в сохранении.
        ModelState.Remove($"{nameof(ProductEditViewModel.Form)}.{nameof(ProductFormViewModel.Sku)}");

        if (ModelState.IsValid)
        {
            await using var image = model.Image?.OpenReadStream();

            var result = await sender.Send(
                new UpdateProductCommand
                {
                    Id = id,
                    Name = model.Name,
                    CategoryId = model.CategoryId,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    Image = model.Image.ToFileUpload(image),
                    RemoveImage = model.RemoveImage,
                },
                cancellationToken);

            if (result.IsSuccess)
            {
                ReportSuccess(localizer["admin.saved"].Value);

                return RedirectToAction(nameof(Edit), new { id });
            }

            ModelState.AddResultError(result, localizer);
        }

        var product = await sender.Send(new GetProductForEditQuery(id), cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        model.CurrentImagePath = product.ImagePath;

        return View(new ProductEditViewModel
        {
            Product = product,
            Form = model,
            Categories = await sender.Send(new GetCategoriesForAdminQuery(), cancellationToken),
        });
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteProductCommand(id), cancellationToken);
        Report(result, localizer["admin.product.deleted"].Value, localizer);

        return result.IsSuccess
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Edit), new { id });
    }

    // ─── Категории ───────────────────────────────────────────────────────────
    // Плоский справочник, поэтому живёт на той же странице, что и товары.

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(string name, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateCategoryCommand(name ?? string.Empty), cancellationToken);
        Report(result, localizer["admin.category.created"].Value, localizer);

        return RedirectToAction(nameof(Index), null, null, "categories");
    }

    [HttpPost("categories/{id:guid}")]
    public async Task<IActionResult> RenameCategory(Guid id, string name, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RenameCategoryCommand(id, name ?? string.Empty), cancellationToken);
        Report(result, localizer["admin.saved"].Value, localizer);

        return RedirectToAction(nameof(Index), null, null, "categories");
    }

    [HttpPost("categories/{id:guid}/delete")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteCategoryCommand(id), cancellationToken);
        Report(result, localizer["admin.category.deleted"].Value, localizer);

        return RedirectToAction(nameof(Index), null, null, "categories");
    }
}