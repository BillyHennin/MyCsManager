// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace FirstFloor.ModernUI.Windows.Controls
{
    public class RelativeAnimatingContentControl : ContentControl
    {
        private const double SimpleDoubleComparisonEpsilon = 0.000009;

        private double _knownHeight;

        private double _knownWidth;

        private List<AnimationValueAdapter> _specialAnimations;

        public RelativeAnimatingContentControl()
        {
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(e != null && e.NewSize.Height > 0 && e.NewSize.Width > 0)
            {
                _knownWidth = e.NewSize.Width;
                _knownHeight = e.NewSize.Height;

                UpdateAnyAnimationValues();
            }
        }

        private void UpdateAnyAnimationValues()
        {
            if(_knownHeight > 0 && _knownWidth > 0)
            {
                if(_specialAnimations == null)
                {
                    _specialAnimations = new List<AnimationValueAdapter>();

                    foreach(VisualStateGroup group in VisualStateManager.GetVisualStateGroups(this))
                    {
                        if(group == null)
                        {
                            continue;
                        }
                        foreach(VisualState state in group.States)
                        {
                            if(state != null)
                            {
                                var sb = state.Storyboard;

                                if(sb != null)
                                {
                                    foreach(var timeline in sb.Children)
                                    {
                                        var da = timeline as DoubleAnimation;
                                        var dakeys = timeline as DoubleAnimationUsingKeyFrames;
                                        if(da != null)
                                        {
                                            ProcessDoubleAnimation(da);
                                        }
                                        else if(dakeys != null)
                                        {
                                            ProcessDoubleAnimationWithKeys(dakeys);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                UpdateKnownAnimations();

                foreach(VisualStateGroup group in VisualStateManager.GetVisualStateGroups(this))
                {
                    if(group == null)
                    {
                        continue;
                    }
                    foreach(VisualState state in group.States)
                    {
                        if(state != null)
                        {
                            var sb = state.Storyboard;

                            if(sb != null)
                            {
                                sb.Begin(this);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateKnownAnimations()
        {
            foreach(var adapter in _specialAnimations)
            {
                adapter.UpdateWithNewDimension(_knownWidth, _knownHeight);
            }
        }

        private void ProcessDoubleAnimationWithKeys(DoubleAnimationUsingKeyFrames da)
        {
            foreach(DoubleKeyFrame frame in da.KeyFrames)
            {
                var d = DoubleAnimationFrameAdapter.GetDimensionFromIdentifyingValue(frame.Value);
                if(d.HasValue)
                {
                    _specialAnimations.Add(new DoubleAnimationFrameAdapter(d.Value, frame));
                }
            }
        }

        private void ProcessDoubleAnimation(DoubleAnimation da)
        {
            if(da.To.HasValue)
            {
                var d = DoubleAnimationToAdapter.GetDimensionFromIdentifyingValue(da.To.Value);
                if(d.HasValue)
                {
                    _specialAnimations.Add(new DoubleAnimationToAdapter(d.Value, da));
                }
            }

            if(da.From.HasValue)
            {
                if(da.To != null)
                {
                    var d = DoubleAnimationFromAdapter.GetDimensionFromIdentifyingValue(da.To.Value);
                    if(d.HasValue)
                    {
                        _specialAnimations.Add(new DoubleAnimationFromAdapter(d.Value, da));
                    }
                }
            }
        }

        private abstract class AnimationValueAdapter
        {
            protected AnimationValueAdapter(DoubleAnimationDimension dimension)
            {
                Dimension = dimension;
            }

            protected DoubleAnimationDimension Dimension { get; private set; }

            public abstract void UpdateWithNewDimension(double width, double height);
        }

        private enum DoubleAnimationDimension
        {
            Width,
            Height,
        }

        private class DoubleAnimationFrameAdapter : GeneralAnimationValueAdapter<DoubleKeyFrame>
        {
            public DoubleAnimationFrameAdapter(DoubleAnimationDimension dimension, DoubleKeyFrame frame) : base(dimension, frame) {}

            protected override double GetValue()
            {
                return Instance.Value;
            }

            protected override void SetValue(double newValue)
            {
                Instance.Value = newValue;
            }
        }

        private class DoubleAnimationFromAdapter : GeneralAnimationValueAdapter<DoubleAnimation>
        {
            public DoubleAnimationFromAdapter(DoubleAnimationDimension dimension, DoubleAnimation instance) : base(dimension, instance) {}

            protected override double GetValue()
            {
                if(Instance.From != null)
                {
                    return (double) Instance.From;
                }
                return 0;
            }

            protected override void SetValue(double newValue)
            {
                Instance.From = newValue;
            }
        }

        private class DoubleAnimationToAdapter : GeneralAnimationValueAdapter<DoubleAnimation>
        {
            public DoubleAnimationToAdapter(DoubleAnimationDimension dimension, DoubleAnimation instance) : base(dimension, instance) {}

            protected override double GetValue()
            {
                if(Instance.To != null)
                {
                    return (double) Instance.To;
                }
                return 0;
            }

            protected override void SetValue(double newValue)
            {
                Instance.To = newValue;
            }
        }

        private abstract class GeneralAnimationValueAdapter<T> : AnimationValueAdapter
        {
            private readonly double _ratio;

            protected GeneralAnimationValueAdapter(DoubleAnimationDimension d, T instance) : base(d)
            {
                Instance = instance;

                InitialValue = StripIdentifyingValueOff(GetValue());
                _ratio = InitialValue / 100;
            }

            protected T Instance { get; private set; }

            private double InitialValue { get; set; }

            protected abstract double GetValue();

            protected abstract void SetValue(double newValue);

            private double StripIdentifyingValueOff(double number)
            {
                return Dimension == DoubleAnimationDimension.Width ? number - .1 : number - .2;
            }

            public static DoubleAnimationDimension? GetDimensionFromIdentifyingValue(double number)
            {
                var floor = Math.Floor(number);
                var remainder = number - floor;

                if(remainder >= .1 - SimpleDoubleComparisonEpsilon && remainder <= .1 + SimpleDoubleComparisonEpsilon)
                {
                    return DoubleAnimationDimension.Width;
                }
                if(remainder >= .2 - SimpleDoubleComparisonEpsilon && remainder <= .2 + SimpleDoubleComparisonEpsilon)
                {
                    return DoubleAnimationDimension.Height;
                }
                return null;
            }

            public override void UpdateWithNewDimension(double width, double height)
            {
                var size = Dimension == DoubleAnimationDimension.Width ? width : height;
                UpdateValue(size);
            }

            private void UpdateValue(double sizeToUse)
            {
                SetValue(sizeToUse * _ratio);
            }
        }
    }
}