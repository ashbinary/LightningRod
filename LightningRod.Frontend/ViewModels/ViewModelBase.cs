using System.Runtime.CompilerServices;
using ReactiveUI;

namespace LightningRod.Frontend.ViewModels;

public class ViewModelBase : ReactiveObject
{
    protected void SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
    {
        this.RaiseAndSetIfChanged(ref backingField, value, propertyName);
    }
}
