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
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.UI
{
    public class Window : Table
    {
        private static StringSequence _workingSequence;

        private WindowStyle _style;
        private string _title;
        private BitmapFontCache _titleCache;
        private Vector2 _dragOffset;
        private bool _dragging;
        private Alignment _titleAlignment = Alignment.Center;

        public Window (string title, Skin skin)
            : this(title, skin.Get<WindowStyle>())
        {
            Skin = skin;
        }

        public Window (string title, Skin skin, string styleName)
            : this(title, skin.Get<WindowStyle>(styleName))
        {
            Skin = skin;
        }

        public Window (string title, WindowStyle style)
        {
            if (title == null)
                throw new ArgumentNullException("title");
            _title = title;

            IsMovable = true;
            KeepWithinStage = true;

            Touchable = Touchable.Enabled;
            SetClip(true);

            Style = style;
            Width = 150;
            Height = 150;
            Title = title;
        }

        protected override void OnPreviewTouchDown (TouchEventArgs e)
        {
            ToFront();
            base.OnPreviewTouchDown(e);
        }

        protected override void OnTouchDown (TouchEventArgs e)
        {
            if (e.Button == 0) {
                Vector2 position = e.GetPosition(this);

                _dragging = IsMovable && (Height - position.Y) <= PadTop && position.Y < Height && position.X > 0 && position.X < Width;
                _dragOffset = position;
            }

            if (_dragging || IsModal)
                e.Handled = true;

            base.OnTouchDown(e);
        }

        protected override void OnTouchUp (TouchEventArgs e)
        {
            base.OnTouchUp(e);
            if (_dragging)
                _dragging = false;
        }

        protected override void OnTouchDrag (TouchEventArgs e)
        {
            base.OnTouchDrag(e);
            Vector2 position = e.GetPosition(this);
            if (_dragging)
                Translate(position.X - _dragOffset.X, position.Y - _dragOffset.Y);
        }

        protected override void OnMouseMove (MouseEventArgs e)
        {
            if (IsModal)
                e.Handled = true;
            base.OnMouseMove(e);
        }

        protected override void OnScroll (ScrollEventArgs e)
        {
            if (IsModal)
                e.Handled = true;
            base.OnScroll(e);
        }

        protected override void OnKeyDown (KeyEventArgs e)
        {
            if (IsModal)
                e.Handled = true;
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp (KeyEventArgs e)
        {
            if (IsModal)
                e.Handled = true;
            base.OnKeyUp(e);
        }

        public WindowStyle Style
        {
            get { return _style; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Style");
                _style = value;

                SetBackground(Style.Background);
                _titleCache = new BitmapFontCache(_style.TitleFont);
                _titleCache.Color = Style.TitleFontColor;

                if (_title != null)
                    Title = _title;

                InvalidateHierarchy();
            }
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            Stage stage = Stage;
            if (KeepWithinStage && Parent == stage.Root) {
                float parentWidth = stage.Width;
                float parentHeight = stage.Height;
                if (X < 0)
                    X = 0;
                if (Right > parentWidth)
                    X = parentWidth - Width;
                if (Y < 0)
                    Y = 0;
                if (Top > parentHeight)
                    Y = parentHeight - Height;
            }

            base.Draw(spriteBatch, parentAlpha);
        }

        protected override void DrawBackground (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            if (_style.StageBackground != null) {
                spriteBatch.Color = Color.MultiplyAlpha(parentAlpha);

                Stage stage = Stage;
                Vector2 localPos = StageToLocalCoordinates(Vector2.Zero);
                Vector2 localSize = StageToLocalCoordinates(new Vector2(stage.Width, stage.Height));

                _style.StageBackground.Draw(spriteBatch, X + localPos.X, Y + localPos.Y, X + localSize.X, Y + localSize.Y);
            }

            base.DrawBackground(spriteBatch, parentAlpha);

            float x = X;
            float y = Y + Height;
            TextBounds bounds = _titleCache.Bounds;

            if ((_titleAlignment & Alignment.Left) != 0)
                x += PadLeft;
            else if ((_titleAlignment & Alignment.Right) != 0)
                x += Width - bounds.Width - PadRight;
            else
                x += (Width - bounds.Width) / 2;

            if ((_titleAlignment & Alignment.Top) == 0) {
                if ((_titleAlignment & Alignment.Bottom) != 0)
                    y -= PadTop - bounds.Height;
                else
                    y -= (PadTop - bounds.Height) / 2;
            }

            _titleCache.Color = Color.Multiply(_style.TitleFontColor);
            _titleCache.SetPosition((int)x, (int)y);
            _titleCache.Draw(spriteBatch, parentAlpha);
        }

        public override Actor Hit (float x, float y, bool touchable)
        {
            Actor hit = base.Hit(x, y, touchable);
            if (hit == null && IsModal && (!touchable || Touchable == Touchable.Enabled))
                return this;
            return hit;
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                _workingSequence.Value = value;
                _titleCache.SetMultiLineText(_workingSequence, 0, 0);
            }
        }

        public Alignment TitleAlignment
        {
            get { return _titleAlignment; }
            set { _titleAlignment = value; }
        }

        public bool IsMovable { get; set; }
        public bool IsModal { get; set; }
        public bool KeepWithinStage { get; set; }

        public bool IsDragging
        {
            get { return _dragging; }
        }

        public override float PrefWidth
        {
            get { return Math.Max(base.PrefWidth, _titleCache.Bounds.Width + PadLeft + PadRight); }
        }
    }

    public class WindowStyle
    {
        public WindowStyle ()
        {
            TitleFontColor = Color.White;
        }

        public WindowStyle (BitmapFont titleFont, Color titleFontColor, ISceneDrawable background)
        {
            Background = background;
            TitleFont = titleFont;
            TitleFontColor = titleFontColor;
        }

        public WindowStyle (WindowStyle style)
        {
            Background = style.Background;
            TitleFont = style.TitleFont;
            TitleFontColor = style.TitleFontColor;
        }

        public ISceneDrawable Background { get; set; }
        public BitmapFont TitleFont { get; set; }
        public Color TitleFontColor { get; set; }
        public ISceneDrawable StageBackground { get; set; }
    }
}
