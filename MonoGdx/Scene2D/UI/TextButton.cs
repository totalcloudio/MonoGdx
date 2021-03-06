﻿/**
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using MonoGdx.TableLayout;

namespace MonoGdx.Scene2D.UI
{
    public class TextButton : Button
    {
        private readonly Label _label;
        private TextButtonStyle _style;

        public TextButton (string text, Skin skin)
            : this(text, skin.Get<TextButtonStyle>())
        {
            Skin = skin;
        }

        public TextButton (string text, Skin skin, string styleName)
            : this(text, skin.Get<TextButtonStyle>(styleName))
        {
            Skin = skin;
        }

        public TextButton (string text, TextButtonStyle style)
        {
            Style = style;

            _label = new Label(text, new LabelStyle(style.Font, style.FontColor));
            _label.SetAlignment(Alignment.Center);

            Add(_label).Configure.Expand().Fill();
            Width = PrefWidth;
            Height = PrefHeight;
        }

        public new TextButtonStyle Style
        {
            get { return StyleCore as TextButtonStyle; }
            set { StyleCore = value; }
        }

        protected override ButtonStyle StyleCore
        {
            get { return _style; }
            set
            {
                if (!(value is TextButtonStyle))
                    throw new ArgumentException("Style must be a TextButtonStyle");

                base.StyleCore = value;
                _style = value as TextButtonStyle;

                if (_label != null) {
                    LabelStyle labelStyle = _label.Style;
                    labelStyle.Font = _style.Font;
                    labelStyle.FontColor = _style.FontColor;
                    _label.Style = labelStyle;
                }
            }
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            Color? fontColor;
            if (IsDisabled && _style.DisabledFontColor != null)
                fontColor = _style.DisabledFontColor;
            else if (IsPressed && _style.DownFontColor != null)
                fontColor = _style.DownFontColor;
            else if (IsChecked && _style.CheckedFontColor != null)
                fontColor = IsOver ? (_style.CheckedOverFontColor ?? _style.CheckedFontColor) : _style.CheckedFontColor;
            else if (IsOver && _style.OverFontColor != null)
                fontColor = _style.OverFontColor;
            else
                fontColor = _style.FontColor;

            if (fontColor != null)
                _label.Style.FontColor = fontColor;

            base.Draw(spriteBatch, parentAlpha);
        }

        public Label Label
        {
            get { return _label; }
        }

        public Cell LabelCell
        {
            get { return GetCell(_label); }
        }

        public string Text
        {
            get { return _label.Text; }
            set { _label.Text = value; }
        }
    }

    public class TextButtonStyle : ButtonStyle
    {
        public TextButtonStyle ()
        { }

        public TextButtonStyle (ISceneDrawable up, ISceneDrawable down, ISceneDrawable chkd, BitmapFont font)
            : base(up, down, chkd)
        {
            Font = font;
        }

        public TextButtonStyle (TextButtonStyle style)
            : base(style)
        {
            Font = style.Font;
            FontColor = style.FontColor;
            DownFontColor = style.DownFontColor;
            OverFontColor = style.OverFontColor;
            CheckedFontColor = style.CheckedFontColor;
            CheckedOverFontColor = style.CheckedOverFontColor;
            DisabledFontColor = style.DisabledFontColor;
        }

        public BitmapFont Font { get; set; }
        public Color? FontColor { get; set; }
        public Color? DownFontColor { get; set; }
        public Color? OverFontColor { get; set; }
        public Color? CheckedFontColor { get; set; }
        public Color? CheckedOverFontColor { get; set; }
        public Color? DisabledFontColor { get; set; }
    }
}
