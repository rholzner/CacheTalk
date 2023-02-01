using _08.OutPutCustomStore.Epi.Models.ViewModels;

namespace _08.OutPutCustomStore.Epi.Business;

/// <summary>
/// Defines a method which may be invoked by PageContextActionFilter allowing controllers
/// to modify common layout properties of the view model.
/// </summary>
internal interface IModifyLayout
{
    void ModifyLayout(LayoutModel layoutModel);
}
