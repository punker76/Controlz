namespace ControlzEx.Theming
{
    using System;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using ControlzEx.Internal;

    public class WindowBackdropManager : DependencyObject
    {
        private WindowBackdropManager()
        {
        }

        public static readonly DependencyProperty BackdropTypeProperty = DependencyProperty.RegisterAttached("BackdropType", typeof(WindowBackdropType), typeof(WindowBackdropManager), new PropertyMetadata(WindowBackdropType.None, OnBackdropTypeChanged));

        public static void SetBackdropType(Window element, WindowBackdropType value)
        {
            element.SetValue(BackdropTypeProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static WindowBackdropType GetBackdropType(Window element)
        {
            return (WindowBackdropType)element.GetValue(BackdropTypeProperty);
        }

        private static void OnBackdropTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateBackdrop((Window)d);
        }

        // ReSharper disable once InconsistentNaming
        private static readonly DependencyPropertyKey CurrentBackdropTypePropertyKey = DependencyProperty.RegisterAttachedReadOnly("CurrentBackdropType", typeof(WindowBackdropType), typeof(WindowBackdropManager), new PropertyMetadata(WindowBackdropType.None));

        public static readonly DependencyProperty CurrentBackdropTypeProperty = CurrentBackdropTypePropertyKey.DependencyProperty;

        private static void SetCurrentBackdropType(Window element, WindowBackdropType value)
        {
            element.SetValue(CurrentBackdropTypePropertyKey, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static WindowBackdropType GetCurrentBackdropType(Window element)
        {
            return (WindowBackdropType)element.GetValue(CurrentBackdropTypeProperty);
        }

        public static bool UpdateBackdrop(Window target)
        {
            return UpdateBackdrop(target, GetBackdropType(target));
        }

        public static bool UpdateBackdrop(Window target, WindowBackdropType windowBackdropType)
        {
            if (windowBackdropType == GetCurrentBackdropType(target))
            {
                return true;
            }

            if (PresentationSource.FromVisual(target) is HwndSource hwndSource)
            {
                if (hwndSource.CompositionTarget is { } compositionTarget)
                {
                    compositionTarget.BackgroundColor = windowBackdropType != WindowBackdropType.None
                        ? Colors.Transparent
                        : Color.FromRgb(0, 0, 0); // same value as in HwndTarget
                }

                if (target is { AllowsTransparency: true })
                {
                    SetCurrentBackdropType(target, WindowBackdropType.None);
                    return false;
                }

                if (FeatureSupport.IsWindowBackdropSupported is false)
                {
                    SetCurrentBackdropType(target, WindowBackdropType.None);
                    return false;
                }

                var handle = hwndSource.Handle;

                var result = UpdateBackdrop(handle, windowBackdropType);

                SetCurrentBackdropType(target, result
                                           ? windowBackdropType
                                           : WindowBackdropType.None);

                return result;
            }

            return false;
        }

        public static bool UpdateBackdrop(IntPtr handle, WindowBackdropType windowBackdropType)
        {
            if (FeatureSupport.IsWindowBackdropSupported is false)
            {
                return false;
            }

            return DwmHelper.SetBackdropType(handle, windowBackdropType);
        }
    }
}