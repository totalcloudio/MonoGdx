﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGdx.Geometry;
/**
 * Copyright 2011-2013 See AUTHORS file.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using MonoGdx.Scene2D.Utils;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.UI
{
    public class Slider : Widget
    {
        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<float>), typeof(Slider));

        private SliderStyle _style;
        private float _min;
        private float _max;
        private float _stepSize;
        private float _value;
        private float _animateFromValue;
        private float _sliderPos;
        private bool _vertical;
        private int _draggingPointer = -1;
        private float _animateDuration;
        private float _animateTime;
        private Interpolation _animateInterpolation = Interpolation.Linear;
        
        public Slider (float min, float max, float stepSize, bool vertical, Skin skin)
            : this(min, max, stepSize, vertical, skin.Get<SliderStyle>("default-" + (vertical ? "vertical" : "horizontal")))
        { }

        public Slider (float min, float max, float stepSize, bool vertical, Skin skin, string styleName)
            : this(min, max, stepSize, vertical, skin.Get<SliderStyle>(styleName))
        { }

        public Slider (float min, float max, float stepSize, bool vertical, SliderStyle style)
        {
            if (min > max)
                throw new ArgumentException("min must be > max: " + min + " > " + max);
            if (stepSize < 0)
                throw new ArgumentOutOfRangeException("stepSize must be > 0: " + stepSize);

            Style = style;
            _min = min;
            _max = max;
            _stepSize = stepSize;
            _vertical = vertical;
            _value = min;

            Width = PrefWidth;
            Height = PrefHeight;
        }

        protected override void OnTouchDown (TouchEventArgs e)
        {
            try {
                if (IsDisabled)
                    return;
                if (_draggingPointer != -1)
                    return;

                Vector2 position = e.GetPosition(this);

                _draggingPointer = e.Pointer;
                CalculatePositionAndValue(position.X, position.Y);

                CaptureTouch(e.Pointer);
                e.Handled = true;
            }
            finally {
                base.OnTouchDown(e);
            }
        }

        protected override void OnTouchDrag (TouchEventArgs e)
        {
            base.OnTouchDrag(e);

            Vector2 position = e.GetPosition(this);
            CalculatePositionAndValue(position.X, position.Y);
        }

        protected override void OnTouchUp (TouchEventArgs e)
        {
            base.OnTouchUp(e);

            if (e.Pointer != _draggingPointer)
                return;

            _draggingPointer = -1;
            ReleaseTouchCapture(e.Pointer);

            Vector2 position = e.GetPosition(this);

            if (!CalculatePositionAndValue(position.X, position.Y)) {
                // TODO: Raise event for end of dragging control.

                //ChangeEvent changeEvent = Pools<ChangeEvent>.Obtain();
                //Fire(changeEvent);
                //Pools<ChangeEvent>.Release(changeEvent);
            }
        }

        public SliderStyle Style
        {
            get { return _style; }
            set 
            { 
                if (value == null)
                    throw new ArgumentException("Style");
                _style = value;
                InvalidateHierarchy();
            }
        }

        public override void Act (float delta)
        {
            base.Act(delta);
            _animateTime -= delta;
        }

        public override void Draw (Graphics.G2D.GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            SliderStyle style = Style;
            bool disabled = IsDisabled;

            ISceneDrawable knob = (disabled && style.DisabledKnob != null) ? style.DisabledKnob : style.Knob;
            ISceneDrawable bg = (disabled && style.DisabledBackground != null) ? style.DisabledBackground : style.Background;
            ISceneDrawable knobBefore = (disabled && style.DisabledKnobBefore != null) ? style.DisabledKnobBefore : style.KnobBefore;
            ISceneDrawable knobAfter = (disabled && style.DisabledKnobAfter != null) ? style.DisabledKnobAfter : style.KnobAfter;

            Color color = Color;
            float x = X;
            float y = Y;
            float width = Width;
            float height = Height;
            float knobHeight = (knob == null) ? 0 : knob.MinHeight;
            float knobWidth = (knob == null) ? 0 : knob.MinWidth;
            float value = VisualValue;

            spriteBatch.Color = color.MultiplyAlpha(parentAlpha);

            if (_vertical) {
                bg.Draw(spriteBatch, x + (int)((width - bg.MinWidth) * .5f), y, bg.MinWidth, height);

                float sliderPosHeight = height - (bg.TopHeight + bg.BottomHeight);
                if (_min != _max) {
                    _sliderPos = (value - _min) / (_max - _min) * (sliderPosHeight - knobHeight);
                    _sliderPos = Math.Max(0, _sliderPos);
                    _sliderPos = Math.Min(sliderPosHeight - knobHeight, _sliderPos) + bg.BottomHeight;
                }

                float knobHeightHalf = knobHeight * .5f;
                if (knobBefore != null)
                    knobBefore.Draw(spriteBatch, x + (int)((width - knobBefore.MinWidth) * .5f), y, 
                        knobBefore.MinWidth, (int)(_sliderPos + knobHeightHalf));
                if (knobAfter != null)
                    knobAfter.Draw(spriteBatch, x + (int)((width - knobAfter.MinWidth) * .5f), y + (int)(_sliderPos + knobHeightHalf), 
                        knobAfter.MinWidth, height - (int)(_sliderPos + knobHeightHalf));
                if (knob != null)
                    knob.Draw(spriteBatch, x + (int)((width - knobWidth) * .5f), (int)(y + _sliderPos), knobWidth, knobHeight);
            }
            else {
                bg.Draw(spriteBatch, x, y + (int)((height - bg.MinHeight) * .5f), width, bg.MinHeight);

                float sliderPosWidth = width - (bg.LeftWidth + bg.RightWidth);
                if (_min != _max) {
                    _sliderPos = (value - _min) / (_max - _min) * (sliderPosWidth - knobWidth);
                    _sliderPos = Math.Max(0, _sliderPos);
                    _sliderPos = Math.Min(sliderPosWidth - knobWidth, _sliderPos) + bg.LeftWidth;
                }

                float knobWidthHalf = knobWidth * .5f;
                if (knobBefore != null)
                    knobBefore.Draw(spriteBatch, x, y + (int)((height - knobBefore.MinHeight) * .5f), 
                        (int)(_sliderPos + knobWidthHalf), knobBefore.MinHeight);
                if (knobAfter != null)
                    knobAfter.Draw(spriteBatch, x + (int)(_sliderPos + knobWidthHalf), y + (int)((height - knobAfter.MinWidth) * .5f),
                        width - (int)(_sliderPos + knobWidthHalf), knobAfter.MinHeight);
                if (knob != null)
                    knob.Draw(spriteBatch, (int)(x + _sliderPos), (int)(y + (height - knobHeight) * .5f), knobWidth, knobHeight);
            }
        }
        
        bool CalculatePositionAndValue (float x, float y)
        {
            ISceneDrawable knob = (IsDisabled && _style.DisabledKnob != null) ? _style.DisabledKnob : _style.Knob;
            ISceneDrawable bg = (IsDisabled && _style.DisabledBackground != null) ? _style.DisabledBackground : _style.Background;

            float value;
            float oldPosition = _sliderPos;

            if (_vertical) {
                float height = Height - bg.TopHeight - bg.BottomHeight;
                float knobHeight = (knob == null) ? 0 : knob.MinHeight;
                _sliderPos = y - bg.BottomHeight - knobHeight * .5f;
                value = _min + (_max - _min) * (_sliderPos / (height - knobHeight));
                _sliderPos = Math.Max(0, _sliderPos);
                _sliderPos = Math.Min(height - knobHeight, _sliderPos);
            }
            else {
                float width = Width - bg.LeftWidth - bg.RightWidth;
                float knobWidth = (knob == null) ? 0 : knob.MinWidth;
                _sliderPos = x - bg.LeftWidth - knobWidth * .5f;
                value = _min + (_max - _min) * (_sliderPos / (width - knobWidth));
                _sliderPos = Math.Max(0, _sliderPos);
                _sliderPos = Math.Min(width - knobWidth, _sliderPos);
            }

            float oldValue = value;
            bool valueSet = SetValue(value);
            if (value == oldValue)
                _sliderPos = oldPosition;

            return valueSet;
        }

        public bool IsDragging
        {
            get { return _draggingPointer != -1; }
        }

        public float Value
        {
            get { return _value; }
            set { SetValue(value); }
        }

        public bool SetValue (float value)
        {
            value = Snap(Clamp((float)Math.Round(value / _stepSize) * _stepSize));
            float oldValue = _value;
            if (value == oldValue)
                return false;

            float oldVisualValue = VisualValue;
            _value = value;

            bool cancelled = OnValueChanged(oldValue, value);
            if (cancelled)
                _value = oldValue;
            else if (_animateDuration > 0) {
                _animateFromValue = oldVisualValue;
                _animateTime = _animateDuration;
            }

            return !cancelled;
        }

        public float VisualValue
        {
            get 
            { 
                if (_animateTime > 0)
                    return _animateInterpolation.Apply(_animateFromValue, _value, 1 - _animateTime / _animateDuration);
                return _value;
            }
        }

        protected float Clamp (float value)
        {
            return MathHelper.Clamp(value, _min, _max);
        }

        public float MinValue
        {
            get { return _min; }
        }

        public float MaxValue
        {
            get { return _max; }
        }

        public void SetRange (float min, float max)
        {
            if (min > max)
                throw new ArgumentException("min must be <= max");
            _min = min;
            _max = max;

            if (_value < min)
                SetValue(min);
            else if (_value > max)
                SetValue(max);
        }

        public float StepSize
        {
            get { return _stepSize; }
            set 
            { 
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("steps must be > 0: " + value);
                _stepSize = value;
            }
        }

        public override float PrefWidth
        {
            get
            {
                if (_vertical) {
                    ISceneDrawable knob = (IsDisabled && _style.DisabledKnob != null) ? _style.DisabledKnob : _style.Knob;
                    ISceneDrawable bg = (IsDisabled && _style.DisabledBackground != null) ? _style.DisabledBackground : _style.Background;
                    return Math.Max(knob == null ? 0 : knob.MinWidth, bg.MinWidth);
                }
                else
                    return 140;
            }
        }

        public override float PrefHeight
        {
            get
            {
                if (_vertical)
                    return 140;
                else {
                    ISceneDrawable knob = (IsDisabled && _style.DisabledKnob != null) ? _style.DisabledKnob : _style.Knob;
                    ISceneDrawable bg = (IsDisabled && _style.DisabledBackground != null) ? _style.DisabledBackground : _style.Background;
                    return Math.Max(knob == null ? 0 : knob.MinHeight, bg.MinHeight);
                }
            }
        }

        public float AnimationDuration
        {
            get { return _animateDuration; }
            set { _animateDuration = value; }
        }

        public Interpolation AnimationInterpolation
        {
            get { return _animateInterpolation; }
            set { 
                if (value == null)
                    throw new ArgumentNullException("AnimationInterpolation");
                _animateInterpolation = value;
            }
        }

        public float[] SnapValues { get; set; }

        public float SnapThreshold { get; set; }

        private float Snap (float value)
        {
            if (SnapValues == null)
                return value;
            foreach (float v in SnapValues) {
                if (Math.Abs(value - v) <= SnapThreshold)
                    return v;
            }
            return value;
        }

        public bool IsDisabled { get; set; }

        public event RoutedPropertyChangedEventHandler<float> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        protected virtual bool OnValueChanged (float oldValue, float newValue)
        {
            return RaiseEvent(new RoutedPropertyChangedEventArgs<float>(oldValue, newValue) {
                RoutedEvent = ValueChangedEvent,
                OriginalSource = this,
                Source = this,
            });
        }
    }

    public class SliderStyle
    {
        public SliderStyle ()
        { }

        public SliderStyle (ISceneDrawable background, ISceneDrawable knob)
        {
            Background = background;
            Knob = knob;
        }

        public SliderStyle (SliderStyle style)
        {
            Background = style.Background;
            Knob = style.Knob;
        }

        public ISceneDrawable Background { get; set; }
        public ISceneDrawable DisabledBackground { get; set; }
        public ISceneDrawable Knob { get; set; }
        public ISceneDrawable DisabledKnob { get; set; }

        public ISceneDrawable KnobBefore { get; set; }
        public ISceneDrawable KnobAfter { get; set; }
        public ISceneDrawable DisabledKnobBefore { get; set; }
        public ISceneDrawable DisabledKnobAfter { get; set; }
    }
}
