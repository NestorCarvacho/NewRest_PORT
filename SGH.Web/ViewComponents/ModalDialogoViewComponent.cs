using Microsoft.AspNetCore.Mvc;
using SGH.Web.Ui;

namespace SGH.Web.ViewComponents;

public sealed class ModalDialogoViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ModalDialogoVm modelo) => View(modelo);
}