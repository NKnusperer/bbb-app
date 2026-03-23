using Android.Runtime;
using Avalonia.Android;

namespace BlackBoxBuddy.Android;

[Application]
public class AndroidApp : AvaloniaAndroidApplication<App>
{
    protected AndroidApp(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
}
