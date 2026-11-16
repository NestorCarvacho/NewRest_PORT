using Microsoft.AspNetCore.Mvc;
using SGH.Web.Ui;

namespace SGH.Web.ViewComponents;

public sealed class ModuloEscritorioViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ModuloEscritorioVm modelo) => View(modelo);
}