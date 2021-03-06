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

namespace MonoGdx.Scene2D.Actions
{
    /// <summary>
    /// Moves an actor from its current size to a relative size.
    /// </summary>
    public class SizeByAction : RelativeTemporalAction
    {
        public float AmountWidth { get; set; }
        public float AmountHeight { get; set; }

        public void SetAmount (float width, float height)
        {
            AmountWidth = width;
            AmountHeight = height;
        }

        protected override void UpdateRelative (float percentDelta)
        {
            Actor.Size(AmountWidth * percentDelta, AmountHeight * percentDelta);
        }
    }
}
