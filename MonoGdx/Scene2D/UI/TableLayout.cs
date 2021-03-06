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
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGdx.Scene2D.Utils;
using MonoGdx.TableLayout;
using TLToolkit = MonoGdx.TableLayout.Toolkit;

namespace MonoGdx.Scene2D.UI
{
    public class TableLayout : BaseTableLayout<Actor, Table, TableLayout, TableToolkit>
    {
        [TODO]
        public TableLayout ()
            : base(TLToolkit.Instance as TableToolkit)
        {
            DebugRects = new List<TableToolkit.DebugRect>();
        }

        public bool IsRound { get; set; }

        internal List<TableToolkit.DebugRect> DebugRects { get; private set; }

        public void Layout ()
        {
            Table table = Table;
            float width = table.Width;
            float height = table.Height;

            base.Layout(0, 0, width, height);

            List<Cell> cells = Cells;
            if (IsRound) {
                foreach (Cell c in cells) {
                    if (c.Ignore == true)
                        continue;

                    float widgetWidth = (float)Math.Round(c.WidgetWidth);
                    float widgetHeight = (float)Math.Round(c.WidgetHeight);
                    float widgetX = (float)Math.Round(c.WidgetX);
                    float widgetY = height - (float)Math.Round(c.WidgetY) - widgetHeight;

                    c.WidgetX = widgetX;
                    c.WidgetY = widgetY;
                    c.WidgetWidth = widgetWidth;
                    c.WidgetHeight = widgetHeight;

                    Actor actor = c.Widget as Actor;
                    if (actor != null) {
                        actor.X = widgetX;
                        actor.Y = widgetY;

                        if (actor.Width != widgetWidth || actor.Height != widgetHeight) {
                            actor.Width = widgetWidth;
                            actor.Height = widgetHeight;
                            if (actor is ILayout)
                                (actor as ILayout).Invalidate();
                        }
                    }
                }
            }
            else {
                foreach (Cell c in cells) {
                    if (c.Ignore == true)
                        continue;

                    float widgetWidth = c.WidgetWidth;
                    float widgetHeight = c.WidgetHeight;
                    float widgetX = c.WidgetX;
                    float widgetY = height - c.WidgetY - widgetHeight;

                    c.WidgetX = widgetX;
                    c.WidgetY = widgetY;
                    c.WidgetWidth = widgetWidth;
                    c.WidgetHeight = widgetHeight;

                    Actor actor = c.Widget as Actor;
                    if (actor != null) {
                        actor.X = widgetX;
                        actor.Y = widgetY;

                        if (actor.Width != widgetWidth || actor.Height != widgetHeight) {
                            actor.Width = widgetWidth;
                            actor.Height = widgetHeight;
                            if (actor is ILayout)
                                (actor as ILayout).Invalidate();
                        }
                    }
                }
            }

            // Validate children separately from sizing actors to ensure actors without a cell are validated.
            foreach (Actor child in table.Children) {
                if (child is ILayout)
                    (child as ILayout).Validate();
            }
        }

        public override void InvalidateHierarchy ()
        {
            base.Invalidate();
            Table.InvalidateHierarchy();
        }

        private Vector2 ToStageCoordinates (Actor actor, Vector2 point)
        {
            if (actor == null)
                return point;

            point.X += actor.X;
            point.Y += actor.Y;
            return ToStageCoordinates(actor.Parent, point);
        }

        [TODO]
        public void DrawDebug (SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }
    }
}
