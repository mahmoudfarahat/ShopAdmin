using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopAdmin.Models;
using ShopAdmin.Models.Repository;
using ShopAdmin.Utilities;
using ShopAdmin.ViewModels;

namespace ShopAdmin.Controllers
{
    public class PieController : Controller
    {
        private readonly IPieRepository pieRepository;
        private readonly ICategoryRepository categoryRepository;

        public PieController(IPieRepository pieRepository , ICategoryRepository categoryRepository)
        {
            this.pieRepository = pieRepository;
            this.categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var pies = await pieRepository.GetAllPiesAsync();
            return View(pies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var pie = await pieRepository.GetPieByIdAsync(id);
            return View(pie);
        }


        public async Task<IActionResult> Add()
        {
            try
            {
                IEnumerable<Category>? allCategories = await  categoryRepository.GetAllCategoriesAsync();
                IEnumerable<SelectListItem> selectListItems = new SelectList(allCategories, "CategoryId", "Name", null);

                PieAddViewModel pieAddViewModel = new() { Categories = selectListItems };
                return View(pieAddViewModel);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"There was an error: {ex.Message}";
            }
            return View(new PieAddViewModel());

        }


        [HttpPost]
        public async Task<IActionResult> Add(PieAddViewModel pieAddViewModel)
        {
            if (ModelState.IsValid)
            {
                Pie pie = new()
                {
                    CategoryId = pieAddViewModel.Pie.CategoryId,
                    ShortDescription = pieAddViewModel.Pie.ShortDescription,
                    LongDescription = pieAddViewModel.Pie.LongDescription,
                    Price = pieAddViewModel.Pie.Price,
                    AllergyInformation = pieAddViewModel.Pie.AllergyInformation,
                    ImageThumbnailUrl = pieAddViewModel.Pie.ImageThumbnailUrl,
                    ImageUrl = pieAddViewModel.Pie.ImageUrl,
                    InStock = pieAddViewModel.Pie.InStock,
                    IsPieOfTheWeek = pieAddViewModel.Pie.IsPieOfTheWeek,
                    Name = pieAddViewModel.Pie.Name
                };

                await pieRepository.AddPieAsync(pie);
                return RedirectToAction(nameof(Index));
            }
            var allCategories = await categoryRepository.GetAllCategoriesAsync();

            IEnumerable<SelectListItem> selectListItems = new SelectList(allCategories, "CategoryId", "Name", null);

            pieAddViewModel.Categories = selectListItems;

            return View(pieAddViewModel);

        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allCategories = await  categoryRepository.GetAllCategoriesAsync();

            IEnumerable<SelectListItem> selectListItems = new SelectList(allCategories, "CategoryId", "Name", null);

            var selectedPie = await  pieRepository.GetPieByIdAsync(id.Value);

            PieEditViewModel pieEditViewModel = new() { Categories = selectListItems, Pie = selectedPie };
            return View(pieEditViewModel);
        }


        //[HttpPost]
        //public async Task<IActionResult> Edit(Pie pie)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            await  pieRepository.UpdatePieAsync(pie);
        //            return RedirectToAction(nameof(Index));
        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", $"Updating the category failed, please try again! Error: {ex.Message}");
        //    }

        //    var allCategories = await  categoryRepository.GetAllCategoriesAsync();

        //    IEnumerable<SelectListItem> selectListItems = new SelectList(allCategories, "CategoryId", "Name", null);

        //    PieEditViewModel pieEditViewModel = new() { Categories = selectListItems, Pie = pie };

        //    return View(pieEditViewModel);
        //}

        public async Task<IActionResult> Delete(int id)
        {
            var selectedCategory = await pieRepository.GetPieByIdAsync(id);

            return View(selectedCategory);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? pieId)
        {
            if (pieId == null)
            {
                ViewData["ErrorMessage"] = "Deleting the pie failed, invalid ID!";
                return View();
            }

            try
            {
                await pieRepository.DeletePieAsync(pieId.Value);
                TempData["PieDeleted"] = "Pie deleted successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Deleting the pie failed, please try again! Error: {ex.Message}";
            }

            var selectedPie = await pieRepository.GetPieByIdAsync(pieId.Value);
            return View(selectedPie);
        }
        private int pageSize = 5;
        public async Task<IActionResult> IndexPaging(int? pageNumber)
        {
            var pies = await pieRepository.GetPiesPagedAsync(pageNumber, pageSize);
            pageNumber ??= 1;
            var count = await pieRepository.GetAllPiesCountAsync();

            return View(new PagedList<Pie>(pies.ToList(), count, pageNumber.Value, pageSize));
        }

        public async Task<IActionResult> IndexPagingSorting(string sortBy, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortBy;

            ViewData["IdSortParam"] = String.IsNullOrEmpty(sortBy) || sortBy == "id_desc" ? "id" : "id_desc";
            ViewData["NameSortParam"] = sortBy == "name" ? "name_desc" : "name";
            ViewData["PriceSortParam"] = sortBy == "price" ? "price_desc" : "price";

            pageNumber ??= 1;

            var pies = await  pieRepository.GetPiesSortedAndPagedAsync(sortBy, pageNumber, pageSize);

            var count = await  pieRepository.GetAllPiesCountAsync();

            return View(new PagedList<Pie>(pies.ToList(), count, pageNumber.Value, pageSize));
        }
        public async Task<IActionResult> Search(string? searchQuery, int? searchCategory)
        {
            var allCategories = await  categoryRepository.GetAllCategoriesAsync();

            IEnumerable<SelectListItem> selectListItems = new SelectList(allCategories, "CategoryId", "Name", null);

            if (searchQuery != null)
            {
                var pies = await  pieRepository.SearchPies(searchQuery, searchCategory);

                return View(new PieSearchViewModel()
                {
                    Pies = pies,
                    SearchCategory = searchCategory,
                    Categories = selectListItems,
                    SearchQuery = searchQuery
                });
            }
            return View(new PieSearchViewModel()
            {
                Pies = new List<Pie>(),
                SearchCategory = null,
                Categories = selectListItems,
                SearchQuery = string.Empty
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Pie pie)
        {
            Pie pieToUpdate = await  pieRepository.GetPieByIdAsync(pie.PieId);

            try
            {
                if (pieToUpdate == null)
                {
                    ModelState.AddModelError(string.Empty, "The pie you want to update doesn't exist or was already deleted by someone else.");
                }

                if (ModelState.IsValid)
                {
                    await  pieRepository.UpdatePieAsync(pie);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var exceptionPie = ex.Entries.Single();
                var entityValues = (Pie)exceptionPie.Entity;
                var databasePie = exceptionPie.GetDatabaseValues();

                if (databasePie == null)
                {
                    ModelState.AddModelError(string.Empty, "The pie was already deleted by another user.");
                }
                else
                {
                    var databaseValues = (Pie)databasePie.ToObject();

                    if (databaseValues.Name != entityValues.Name)
                    {
                        ModelState.AddModelError("Pie.Name", $"Current value: {databaseValues.Name}");
                    }
                    if (databaseValues.Price != entityValues.Price)
                    {
                        ModelState.AddModelError("Pie.Price", $"Current value: {databaseValues.Price:c}");
                    }
                    if (databaseValues.ShortDescription != entityValues.ShortDescription)
                    {
                        ModelState.AddModelError("Pie.ShortDescription", $"Current value: {databaseValues.ShortDescription}");
                    }
                    if (databaseValues.LongDescription != entityValues.LongDescription)
                    {
                        ModelState.AddModelError("Pie.LongDescription", $"Current value: {databaseValues.LongDescription}");
                    }
                    if (databaseValues.AllergyInformation != entityValues.AllergyInformation)
                    {
                        ModelState.AddModelError("Pie.AllergyInformation", $"Current value: {databaseValues.AllergyInformation}");
                    }
                    if (databaseValues.ImageThumbnailUrl != entityValues.ImageThumbnailUrl)
                    {
                        ModelState.AddModelError("Pie.ImageThumbnailUrl", $"Current value: {databaseValues.ImageThumbnailUrl}");
                    }
                    if (databaseValues.ImageUrl != entityValues.ImageUrl)
                    {
                        ModelState.AddModelError("Pie.ImageUrl", $"Current value: {databaseValues.ImageUrl}");
                    }
                    if (databaseValues.IsPieOfTheWeek != entityValues.IsPieOfTheWeek)
                    {
                        ModelState.AddModelError("Pie.IsPieOfTheWeek", $"Current value: {databaseValues.IsPieOfTheWeek}");
                    }
                    if (databaseValues.InStock != entityValues.InStock)
                    {
                        ModelState.AddModelError("Pie.InStock", $"Current value: {databaseValues.InStock}");
                    }
                    if (databaseValues.CategoryId != entityValues.CategoryId)
                    {
                        ModelState.AddModelError("Pie.CategoryId", $"Current value: {databaseValues.CategoryId}");
                    }

                    ModelState.AddModelError(string.Empty, "The pie was modified already by another user. The database values are now shown. Hit Save again to store these values.");
                    pieToUpdate.RowVersion = databaseValues.RowVersion;

                    ModelState.Remove("Pie.RowVersion");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Updating the category failed, please try again! Error: {ex.Message}");
            }

            var allCategories = await   categoryRepository.GetAllCategoriesAsync();

            IEnumerable<SelectListItem> selectListItems = new SelectList(allCategories, "CategoryId", "Name", null);

            PieEditViewModel pieEditViewModel = new() { Categories = selectListItems, Pie = pieToUpdate };

            return View(pieEditViewModel);
        }
    }

}
