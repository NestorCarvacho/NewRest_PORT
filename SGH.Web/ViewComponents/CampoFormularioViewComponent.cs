using Microsoft.AspNetCore.Mvc;
using SGH.Web.Ui;

namespace SGH.Web.ViewComponents;

public sealed class CampoFormularioViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(CampoFormularioVm modelo) => View(modelo);
}