// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handle HTTP API requests related to admin category management. 
// ***********************************************************************

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Requests;
using omnicart_api.Services;

namespace omnicart_api.Controllers.Admin;

[Route("api/admin/categories")]
[ApiController]
[ServiceFilter(typeof(ValidateModelAttribute))]
[Authorize(Roles = "admin")]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _categoryService;

    /// <summary>
    /// Initializes the CategoryController with MongoDbService dependency.
    /// </summary>
    /// <param name="categoryService">The MongoDB service</param>
    public CategoryController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }


    /// <summary>
    ///  Handles GET requests to retrieve a specific category by ID.
    /// </summary>
    /// <param name="id">The ObjectId of the category</param>
    /// <returns>The category object if found</returns>
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<AppResponse<Category>>> Get(string id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);

        if (category == null)
        {
            return NotFound(new AppResponse<Category>
            {
                Success = false,
                Message = "Category not found",
                ErrorCode = 404
            });
        }

        return Ok(new AppResponse<Category>
        {
            Success = true,
            Data = category,
            Message = "Category retrieved successfully"
        });
    }

    /// <summary>
    /// Handles POST requests to create a new category in the MongoDB collection.
    /// </summary>
    /// <param name="newCategory">The new category object</param>
    /// <returns>CreatedAtAction result with the new category</returns>
    [HttpPost]
    public async Task<ActionResult<AppResponse<Category>>> Post(Category newCategory)
    {
        await _categoryService.CreateCategoryAsync(newCategory);

        return CreatedAtAction(nameof(Get), new { id = newCategory.Id }, new AppResponse<Category>
        {
            Success = true,
            Data = newCategory,
            Message = "Category created successfully"
        });
    }

    /// <summary>
    /// Handles PUT requests to update an existing category
    /// </summary>
    /// <param name="id">The ObjectId of the category</param>
    /// <param name="updatedCategory"></param>
    /// <returns> Category result if successful </returns>
    [HttpPut("{id:length(24)}")]
    public async Task<ActionResult<AppResponse<Category>>> Update(string id, CategoryDto updatedCategory)
    {
        var existingCategory = await _categoryService.GetCategoryByIdAsync(id);

        if (existingCategory == null)
        {
            return NotFound(new AppResponse<Category>
            {
                Success = false,
                Message = "Category not found",
                ErrorCode = 404
            });
        }

        existingCategory.Name = updatedCategory.Name;
        existingCategory.Image = updatedCategory.Image;
        existingCategory.IsActive = updatedCategory.IsActive;

        await _categoryService.UpdateCategoryAsync(id, existingCategory);

        return Ok(new AppResponse<Category>
        {
            Success = true,
            Data = existingCategory,
            Message = "Category updated successfully"
        });
    }

    /// <summary>
    /// Handles DELETE requests to remove a category
    /// </summary>
    /// <param name="id">The ObjectId of the category</param>
    /// <returns>NoContent result if successful</returns>
    [HttpDelete("{id:length(24)}")]
    public async Task<ActionResult<AppResponse<Category>>> Delete(string id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);

        if (category == null)
        {
            return NotFound(new AppResponse<string>
            {
                Success = false,
                Message = "Category not found",
                ErrorCode = 404
            });
        }

        await _categoryService.DeleteCategoryAsync(id);

        return Ok(new AppResponse<Category>
        {
            Success = true,
            Data = category,
            Message = "Category deleted successfully"
        });
    }

    [HttpPatch("{id:length(24)}/status")]
    public async Task<ActionResult<AppResponse<Category>>> ChangeActiveStatus(string id, [FromBody] CategoryStatusDto body)
    {
        bool isActive = body.IsActive;

        var existingCategory = await _categoryService.GetCategoryByIdAsync(id);

        if (existingCategory == null)
            return NotFound(new AppResponse<Category> { Success = false, Message = "category not found" });

        await _categoryService.SetCategoryStatusAsync(id, isActive);

        existingCategory.IsActive = isActive;

        return Ok(new AppResponse<Category> { Success = true, Data = existingCategory, Message = $"category status updated to {isActive}" });
    }
}