// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handle HTTP API requests related to public categories. 
// ***********************************************************************

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Requests;
using omnicart_api.Services;

namespace omnicart_api.Controllers;

[Route("api/categories")]
[ApiController]
[ServiceFilter(typeof(ValidateModelAttribute))]
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
    /// Handles GET requests to retrieve all categories
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<AppResponse<List<Category>>>> Get()
    {
        try
        {
            var categories = await _categoryService.GetCategoriesAsync();
            var response = new AppResponse<List<Category>>
            {
                Success = true,
                Data = categories,
                Message = "Categories retrieved successfully"
            };
            return response;
        }
        catch (Exception ex)
        {
            var response = new AppResponse<List<Category>>
            {
                Success = false,
                Data = [],
                Message = "An error occurred while retrieving categories",
                Error = ex.Message,
                ErrorCode = 500
            };

            return StatusCode(500, response);
        }
    }

}
