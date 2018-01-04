// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Data;

using FirstFloor.ModernUI.Shell.Standard;

namespace FirstFloor.ModernUI.Shell
{
    public enum ResizeGripDirection
    {
        None,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left,
        Caption,
    }

    [Flags]
    public enum SacrificialEdge
    {
        None = 0,
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
        Office = Left | Right | Bottom,
    }

    public class WindowChrome : Freezable
    {
        private static List<_SystemParameterBoundProperty> _BoundProperties;

        public WindowChrome()
        {
            _BoundProperties = new List<_SystemParameterBoundProperty>
            {
                new _SystemParameterBoundProperty {DependencyProperty = CornerRadiusProperty, SystemParameterPropertyName = "WindowCornerRadius"},
                new _SystemParameterBoundProperty {DependencyProperty = CaptionHeightProperty, SystemParameterPropertyName = "WindowCaptionHeight"},
                new _SystemParameterBoundProperty
                {
                    DependencyProperty = ResizeBorderThicknessProperty,
                    SystemParameterPropertyName = "WindowResizeBorderThickness"
                },
                new _SystemParameterBoundProperty
                {
                    DependencyProperty = GlassFrameThicknessProperty,
                    SystemParameterPropertyName = "WindowNonClientFrameThickness"
                },
            };

            foreach(var bp in _BoundProperties)
            {
                Assert.IsNotNull(bp.DependencyProperty);
                BindingOperations.SetBinding(this, bp.DependencyProperty,
                    new Binding
                    {
                        Source = SystemParameters2.Current,
                        Path = new PropertyPath(bp.SystemParameterPropertyName),
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    });
            }
        }

        public static Thickness GlassFrameCompleteThickness { get { return new Thickness(-1); } }

        #region Attached Properties

        public static readonly DependencyProperty WindowChromeProperty = DependencyProperty.RegisterAttached("WindowChrome", typeof(WindowChrome),
            typeof(WindowChrome), new PropertyMetadata(null, _OnChromeChanged));
        public static readonly DependencyProperty IsHitTestVisibleInChromeProperty = DependencyProperty.RegisterAttached("IsHitTestVisibleInChrome",
            typeof(bool), typeof(WindowChrome), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty ResizeGripDirectionProperty = DependencyProperty.RegisterAttached("ResizeGripDirection",
            typeof(ResizeGripDirection), typeof(WindowChrome),
            new FrameworkPropertyMetadata(ResizeGripDirection.None, FrameworkPropertyMetadataOptions.Inherits));

        private static void _OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(DesignerProperties.GetIsInDesignMode(d))
            {
                return;
            }
            var window = (Window) d;
            var newChrome = (WindowChrome) e.NewValue;
            Assert.IsNotNull(window);

            var chromeWorker = WindowChromeWorker.GetWindowChromeWorker(window);
            if(chromeWorker == null)
            {
                chromeWorker = new WindowChromeWorker();
                WindowChromeWorker.SetWindowChromeWorker(window, chromeWorker);
            }
            chromeWorker.SetWindowChrome(newChrome);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static WindowChrome GetWindowChrome(Window window)
        {
            Verify.IsNotNull(window, "window");
            return (WindowChrome) window.GetValue(WindowChromeProperty);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void SetWindowChrome(Window window, WindowChrome chrome)
        {
            Verify.IsNotNull(window, "window");
            window.SetValue(WindowChromeProperty, chrome);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static bool GetIsHitTestVisibleInChrome(IInputElement inputElement)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            var dobj = inputElement as DependencyObject;
            if(dobj == null)
            {
                throw new ArgumentException(@"The element must be a DependencyObject", "inputElement");
            }
            return (bool) dobj.GetValue(IsHitTestVisibleInChromeProperty);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void SetIsHitTestVisibleInChrome(IInputElement inputElement, bool hitTestVisible)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            var dobj = inputElement as DependencyObject;
            if(dobj == null)
            {
                throw new ArgumentException(@"The element must be a DependencyObject", "inputElement");
            }
            dobj.SetValue(IsHitTestVisibleInChromeProperty, hitTestVisible);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static ResizeGripDirection GetResizeGripDirection(IInputElement inputElement)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            var dobj = inputElement as DependencyObject;
            if(dobj == null)
            {
                throw new ArgumentException(@"The element must be a DependencyObject", "inputElement");
            }
            return (ResizeGripDirection) dobj.GetValue(ResizeGripDirectionProperty);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void SetResizeGripDirection(IInputElement inputElement, ResizeGripDirection direction)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            var dobj = inputElement as DependencyObject;
            if(dobj == null)
            {
                throw new ArgumentException(@"The element must be a DependencyObject", "inputElement");
            }
            dobj.SetValue(ResizeGripDirectionProperty, direction);
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty CaptionHeightProperty = DependencyProperty.Register("CaptionHeight", typeof(double), typeof(WindowChrome),
            new PropertyMetadata(0d, (d, e) => ((WindowChrome) d)._OnPropertyChangedThatRequiresRepaint()), value => (double) value >= 0d);
        public static readonly DependencyProperty ResizeBorderThicknessProperty = DependencyProperty.Register("ResizeBorderThickness", typeof(Thickness),
            typeof(WindowChrome), new PropertyMetadata(default(Thickness)), value => ((Thickness) value).IsNonNegative());
        public static readonly DependencyProperty GlassFrameThicknessProperty = DependencyProperty.Register("GlassFrameThickness", typeof(Thickness),
            typeof(WindowChrome),
            new PropertyMetadata(default(Thickness), (d, e) => ((WindowChrome) d)._OnPropertyChangedThatRequiresRepaint(),
                (d, o) => _CoerceGlassFrameThickness((Thickness) o)));
        public static readonly DependencyProperty UseAeroCaptionButtonsProperty = DependencyProperty.Register("UseAeroCaptionButtons", typeof(bool),
            typeof(WindowChrome), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(WindowChrome),
            new PropertyMetadata(default(CornerRadius), (d, e) => ((WindowChrome) d)._OnPropertyChangedThatRequiresRepaint()),
            value => ((CornerRadius) value).IsValid());
        public static readonly DependencyProperty SacrificialEdgeProperty = DependencyProperty.Register("SacrificialEdge", typeof(SacrificialEdge),
            typeof(WindowChrome), new PropertyMetadata(SacrificialEdge.None, (d, e) => ((WindowChrome) d)._OnPropertyChangedThatRequiresRepaint()),
            _IsValidSacrificialEdge);
        private static readonly SacrificialEdge SacrificialEdge_All = SacrificialEdge.Bottom | SacrificialEdge.Top | SacrificialEdge.Left
                                                                      | SacrificialEdge.Right;

        public double CaptionHeight { get { return (double) GetValue(CaptionHeightProperty); } set { SetValue(CaptionHeightProperty, value); } }

        public Thickness ResizeBorderThickness
        {
            get { return (Thickness) GetValue(ResizeBorderThicknessProperty); }
            set { SetValue(ResizeBorderThicknessProperty, value); }
        }

        public Thickness GlassFrameThickness
        {
            get { return (Thickness) GetValue(GlassFrameThicknessProperty); }
            set { SetValue(GlassFrameThicknessProperty, value); }
        }

        public bool UseAeroCaptionButtons
        {
            get { return (bool) GetValue(UseAeroCaptionButtonsProperty); }
            set { SetValue(UseAeroCaptionButtonsProperty, value); }
        }

        public CornerRadius CornerRadius { get { return (CornerRadius) GetValue(CornerRadiusProperty); } set { SetValue(CornerRadiusProperty, value); } }

        public SacrificialEdge SacrificialEdge
        {
            get { return (SacrificialEdge) GetValue(SacrificialEdgeProperty); }
            set { SetValue(SacrificialEdgeProperty, value); }
        }

        private static object _CoerceGlassFrameThickness(Thickness thickness)
        {
            if(!thickness.IsNonNegative())
            {
                return GlassFrameCompleteThickness;
            }
            return thickness;
        }

        private static bool _IsValidSacrificialEdge(object value)
        {
            var se = SacrificialEdge.None;
            try
            {
                se = (SacrificialEdge) value;
            }
            catch(InvalidCastException)
            {
                return false;
            }
            if(se == SacrificialEdge.None)
            {
                return true;
            }

            if((se | SacrificialEdge_All) != SacrificialEdge_All)
            {
                return false;
            }

            if(se == SacrificialEdge_All)
            {
                return false;
            }
            return true;
        }

        #endregion

        protected override Freezable CreateInstanceCore()
        {
            return new WindowChrome();
        }

        private void _OnPropertyChangedThatRequiresRepaint()
        {
            var handler = PropertyChangedThatRequiresRepaint;
            if(handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        internal event EventHandler PropertyChangedThatRequiresRepaint;

        private struct _SystemParameterBoundProperty
        {
            public string SystemParameterPropertyName { get; set; }
            public DependencyProperty DependencyProperty { get; set; }
        }
    }
}