using Microsoft.AspNetCore.Mvc;
using SGH.Web.Ui;

namespace SGH.Web.ViewComponents;

public sealed class TablaListadoViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(TablaListadoVm modelo) => View(modelo);
}