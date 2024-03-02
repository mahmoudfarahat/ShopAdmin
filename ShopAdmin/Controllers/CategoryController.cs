using Microsoft.AspNetCore.Mvc;
using ShopAdmin.Models;
using ShopAdmin.Models.Repository;
using ShopAdmin.ViewModels;

namespace ShopAdmin.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            CategoryListViewModel model = new() {
                Categories = (await categoryRepository.GetAllCategoriesAsync()).ToList()
                };
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var selectedCategory = await categoryRepository.GetCategoryByIdAsync(id.Value);

            return View(selectedCategory);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add([Bind("Name,Description,DateAdded")] Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await  categoryRepository.AddCategoryAsync(category);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Adding the category failed, please try again! Error: {ex.Message}");
            }

            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var selectedCategory = await  categoryRepository.GetCategoryByIdAsync(id.Value);
            return View(selectedCategory);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    await categoryRepository.UpdateCategoryAsync(category);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Updating the category failed, please try again! Error: {ex.Message}");
            }



            return View(category);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var selectedCategory = await categoryRepository.GetCategoryByIdAsync(id);
            return View(selectedCategory);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? categoryId)
        {
            if (categoryId == null)
            {
                ViewData["ErrorMessage"] = "Deleting the category failed, invalid Id!";
                return View();  
            }
            try
            {
                await categoryRepository.DeleteCategoryAsync(categoryId.Value);
                TempData["CategoryDeleted"] = "Category Deleted successfully";

                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Deleting the category failed, please try again! Error: {ex.Message}";
              
            }

            var selectedCategory = await categoryRepository.GetCategoryByIdAsync(categoryId.Value);
            return View(selectedCategory);
        }
        public async Task<IActionResult> BulkEdit()
        {
            List<CategoryBulkEditViewModel> categoryBulkEditViewModels = new List<CategoryBulkEditViewModel>();

            var allCategories = await  categoryRepository.GetAllCategoriesAsync();
            foreach (var category in allCategories)
            {
                categoryBulkEditViewModels.Add(new CategoryBulkEditViewModel
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name
                });
            }

            return View(categoryBulkEditViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> BulkEdit(List<CategoryBulkEditViewModel> categoryBulkEditViewModels)
        {
            List<Category> categories = new List<Category>();

            foreach (var categoryBulkEditViewModel in categoryBulkEditViewModels)
            {
                categories.Add(new Category() { CategoryId = categoryBulkEditViewModel.CategoryId, Name = categoryBulkEditViewModel.Name });
            }

            await  categoryRepository.UpdateCategoryNamesAsync(categories);

            return RedirectToAction(nameof(Index));
        }
    }
}
