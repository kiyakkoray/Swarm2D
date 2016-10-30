/******************************************************************************
Copyright (c) 2015 Koray Kiyakoglu

http://www.swarm2d.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Swarm2D.Library;

namespace Swarm2D.Engine.View
{
    public class TextMesh : Mesh
    {
        public float FontHeight { get; set; }

        protected Font LabelFont;

        private float _scaleValue;

        private float _lastFontHeight;
        private float _lastWidth;
        private float _lastHeight;
        private string _lastText;
        private int _textMeshCharacterCount;
        private bool _singleLine;

        private bool _meshNeedUpdate;

        private bool _renderCursor;
        private int _renderCursorPosition;

        const float TextureSize = 512.0f; //TODO:read it from texture
        const float InverseTextureSize = 1.0f / TextureSize;
        const float ExtraPadding = 0.0f;

        public TextMesh(Font labelFont)
        {
            LabelFont = labelFont;

            Vertices = null;
            TextureCoordinates = null;
            _lastText = "";
            FontHeight = 16;
            _lastFontHeight = FontHeight;
            _lastWidth = 0;
            _lastHeight = 0;
            _singleLine = false;
            _renderCursor = false;
            _renderCursorPosition = 0;

            _meshNeedUpdate = true;
        }

        public void Update(float newWidth, float newHeight, string newText)
        {
            if (TextMeshOutdated(newWidth, newHeight, newText))
            {
                RecalculateTextMesh(newWidth, newHeight, newText);
            }
        }

        public int GetCursorPositionNear(float x, float y)
        {
            return 0;
        }

        public void SetSingleLine(bool singleLine)
        {
            if (_singleLine != singleLine)
            {
                _meshNeedUpdate = true;
                _singleLine = singleLine;
            }
        }

        public bool GetSingleLine()
        {
            return _singleLine;
        }

        public void SetRenderCursor(bool renderCursor)
        {
            if (_renderCursor != renderCursor)
            {
                _meshNeedUpdate = true;
                _renderCursor = renderCursor;
            }
        }

        public bool GetRenderCursor()
        {
            return _renderCursor;
        }

        public void SetRenderCursorPosition(int renderCursorPosition)
        {
            if (_renderCursorPosition != renderCursorPosition)
            {
                _renderCursorPosition = renderCursorPosition;
                _meshNeedUpdate = true;
            }
        }

        public int GetRenderCursorPosition()
        {
            return _renderCursorPosition;
        }

        private void AddCharacterToMesh(float x, float y, FontCharacter fontCharacter)
        {
            float u0 = fontCharacter.X * InverseTextureSize;
            float v0 = fontCharacter.Y * InverseTextureSize;
            float u1 = u0 + fontCharacter.Width * InverseTextureSize;
            float v1 = v0 + fontCharacter.Height * InverseTextureSize;
            //v0 = /*1.0f - */v0;
            //v1 = /*1.0f - */v1;

            float width = fontCharacter.Width * _scaleValue;
            float height = fontCharacter.Height * _scaleValue;

            TextureCoordinates[8 * _textMeshCharacterCount + 0] = u0;
            TextureCoordinates[8 * _textMeshCharacterCount + 1] = v0;

            TextureCoordinates[8 * _textMeshCharacterCount + 2] = u1;
            TextureCoordinates[8 * _textMeshCharacterCount + 3] = v0;

            TextureCoordinates[8 * _textMeshCharacterCount + 4] = u1;
            TextureCoordinates[8 * _textMeshCharacterCount + 5] = v1;

            TextureCoordinates[8 * _textMeshCharacterCount + 6] = u0;
            TextureCoordinates[8 * _textMeshCharacterCount + 7] = v1;

            Vertices[8 * _textMeshCharacterCount + 0] = x;
            Vertices[8 * _textMeshCharacterCount + 1] = y;

            Vertices[8 * _textMeshCharacterCount + 2] = x + width;
            Vertices[8 * _textMeshCharacterCount + 3] = y;

            Vertices[8 * _textMeshCharacterCount + 4] = x + width;
            Vertices[8 * _textMeshCharacterCount + 5] = y + height;

            Vertices[8 * _textMeshCharacterCount + 6] = x;
            Vertices[8 * _textMeshCharacterCount + 7] = y + height;

            _textMeshCharacterCount++;
        }

        private void RecalculateTextMesh(float newWidth, float newHeight, string newText)
        {
            if (newText == null)
            {
                newText = "";
            }

            _lastWidth = newWidth;
            _lastHeight = newHeight;

            if (_lastText.Length < newText.Length)
            {
                Vertices = null;
                TextureCoordinates = null;
            }

            if (Vertices == null)
            {
                Vertices = new float[newText.Length * 8 * 2];
            }

            if (TextureCoordinates == null)
            {
                TextureCoordinates = new float[newText.Length * 8 * 2];
            }

            _textMeshCharacterCount = 0;

            _lastFontHeight = FontHeight;
            _lastText = newText;

            _scaleValue = FontHeight / LabelFont.LineHeight;
            _scaleValue = 1.0f;

            double currentWidth = 8.0f;
            double currentHeight = 8 + LabelFont.Base * _scaleValue - LabelFont.LineHeight * _scaleValue;

            for (int i = 0; i < newText.Length; i++)
            {
                char character = newText[i];
                int characterData = character;

                if (character == '\n')
                {
                    currentWidth = 8.0f;
                    currentHeight += LabelFont.Base * (double)_scaleValue;
                }
                else if (LabelFont.Characters.ContainsKey(characterData))
                {
                    FontCharacter fontCharacter = LabelFont.Characters[characterData];

                    if (fontCharacter.Width * _scaleValue + currentWidth > newWidth && !_singleLine)
                    {
                        currentWidth = 0;
                        currentHeight += LabelFont.Base * (double)_scaleValue;
                    }

                    {
                        float x = (float)(currentWidth + fontCharacter.XOffset * (double)_scaleValue);
                        float y = (float)(currentHeight + fontCharacter.YOffset * (double)_scaleValue);

                        AddCharacterToMesh(x, y, fontCharacter);
                    }

                    if (_renderCursor && _renderCursorPosition == i)
                    {
                        FontCharacter cursorCharacter = LabelFont.Characters['|'];

                        float x = (float)(currentWidth + cursorCharacter.XOffset * (double)_scaleValue);
                        float y = (float)(currentHeight + cursorCharacter.YOffset * (double)_scaleValue);

                        AddCharacterToMesh(x, y, cursorCharacter);
                    }

                    currentWidth += ((double)fontCharacter.XAdvance + ExtraPadding) * _scaleValue;
                }
            }

            VertexCount = _textMeshCharacterCount * 4;

            _meshNeedUpdate = false;
        }

        private bool TextMeshOutdated(float newWidth, float newHeight, string newText)
        {
            return _meshNeedUpdate || Vertices == null || TextureCoordinates == null || _lastWidth != newWidth || _lastHeight != newHeight || _lastFontHeight != FontHeight || _lastText != newText;
        }
    }
}
