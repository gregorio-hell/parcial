using Microsoft.AspNetCore.Mvc;
using parcial.Services;
using parcial.ViewModels;

namespace parcial.ViewComponents;

/// <summary>
/// ViewComponent para mostrar el enlace "Volver al curso" del último curso visitado desde sesión
/// </summary>
public class UltimoCursoViewComponent : ViewComponent
{
    private readonly ISesionService _sesionService;

    public UltimoCursoViewComponent(ISesionService sesionService)
    {
        _sesionService = sesionService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Solo mostrar si el usuario está autenticado
        if (!HttpContext.User.Identity?.IsAuthenticated == true)
        {
            return Content(string.Empty);
        }

        var ultimoCurso = await _sesionService.ObtenerUltimoCursoVisitadoAsync();
        
        if (ultimoCurso != null)
        {
            var viewModel = new UltimoCursoVisitadoViewModel
            {
                CursoId = ultimoCurso.Value.Id,
                NombreCurso = ultimoCurso.Value.Nombre,
                CodigoCurso = ultimoCurso.Value.Codigo
            };
            
            return View(viewModel);
        }

        return Content(string.Empty);
    }
}