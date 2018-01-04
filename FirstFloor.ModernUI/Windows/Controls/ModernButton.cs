﻿// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FirstFloor.ModernUI.Windows.Controls
{
    public class ModernButton : Button
    {
        public static readonly DependencyProperty EllipseDiameterProperty = DependencyProperty.Register("EllipseDiameter", typeof(double), typeof(ModernButton),
            new PropertyMetadata(18D));

        public static readonly DependencyProperty EllipseStrokeThicknessProperty = DependencyProperty.Register("EllipseStrokeThickness", typeof(double),
            typeof(ModernButton), new PropertyMetadata(1D));

        public static readonly DependencyProperty IconDataProperty = DependencyProperty.Register("IconData", typeof(Geometry), typeof(ModernButton));

        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register("IconHeight", typeof(double), typeof(ModernButton),
            new PropertyMetadata(10D));

        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register("IconWidth", typeof(double), typeof(ModernButton),
            new PropertyMetadata(10D));

        public ModernButton()
        {
            DefaultStyleKey = typeof(ModernButton);
        }

        public double EllipseDiameter { get { return (double) GetValue(EllipseDiameterProperty); } set { SetValue(EllipseDiameterProperty, value); } }

        public double EllipseStrokeThickness
        {
            get { return (double) GetValue(EllipseStrokeThicknessProperty); }
            set { SetValue(EllipseStrokeThicknessProperty, value); }
        }

        public Geometry IconData { get { return (Geometry) GetValue(IconDataProperty); } set { SetValue(IconDataProperty, value); } }

        public double IconHeight { get { return (double) GetValue(IconHeightProperty); } set { SetValue(IconHeightProperty, value); } }

        public double IconWidth { get { return (double) GetValue(IconWidthProperty); } set { SetValue(IconWidthProperty, value); } }
    }
}