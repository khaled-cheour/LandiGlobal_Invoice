using LandiGlobalTemplate.Models;
using LandiGlobalTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace LandiGlobalTemplate.Controllers
{
    public class ExcelTransformController : Controller
    {
        private readonly ExcelTransformService _transformService;
        private readonly ILogger<ExcelTransformController> _logger;

        public ExcelTransformController(ExcelTransformService transformService, ILogger<ExcelTransformController> logger)
        {
            _transformService = transformService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new ExcelTransformViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Upload(ExcelTransformViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError("File", "Veuillez sélectionner un fichier Excel.");
                return View("Index", model);
            }

            try
            {
                var result = await _transformService.TransformAsync(model.ImportType, model.File, model.MappingFile);
                model.Result = result;

                HttpContext.Session.SetString("ExcelTransformBytes", Convert.ToBase64String(result.FileBytes));
                HttpContext.Session.SetString("ExcelTransformFileName", $"Transformed_{result.ImportTypeLabel}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

                return View("Result", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la transformation Excel");
                ModelState.AddModelError("", $"Erreur lors du traitement du fichier : {ex.Message}");
                return View("Index", model);
            }
        }

        public IActionResult Download()
        {
            var encoded = HttpContext.Session.GetString("ExcelTransformBytes");
            if (string.IsNullOrEmpty(encoded))
            {
                return RedirectToAction("Index");
            }

            var bytes = Convert.FromBase64String(encoded);
            var fileName = HttpContext.Session.GetString("ExcelTransformFileName") ?? $"Transformed_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
