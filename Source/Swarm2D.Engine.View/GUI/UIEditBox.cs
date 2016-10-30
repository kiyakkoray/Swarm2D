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

namespace Swarm2D.Engine.View.GUI
{
    public class UIEditBox : UIFrame
    {
        public event Action TextChanged;

        private UIEditBoxRenderState _renderState;

        private bool _isEditing;

        private Font _editBoxFont;
        private TextMesh _textMesh;

        private int _cursorPosition;

        private Sprite _normalSprite;
        private Sprite _mouseDownSprite;
        private Sprite _mouseOverSprite;

        public override void Initialize(List<UIPositionParameter> positionParameters)
        {
            base.Initialize(positionParameters);

            _renderState = UIEditBoxRenderState.Normal;

            Widget.MouseEnter += new UIMouseEvent(OnMouseEnterHandler);
            Widget.MouseLeave += new UIMouseEvent(OnMouseLeaveHandler);
            Widget.MouseDown += new UIMouseEvent(OnMouseDownHandler);
            Widget.MouseUp += new UIMouseEvent(OnMouseUpHandler);

            _normalSprite = Manager.Skin.ButtonNormalSprite;
            _mouseDownSprite = Manager.Skin.ButtonMouseDownSprite;
            _mouseOverSprite = Manager.Skin.ButtonMouseOverSprite;

            _editBoxFont = Manager.Font;

            _textMesh = new TextMesh(_editBoxFont);
            _textMesh.SetSingleLine(true);

            _cursorPosition = 0;
            _isEditing = false;

            ScissorTestOnRender = true;
        }

        public void OnMouseUpHandler(UIWidget sender, MouseEventArgs e)
        {
            if (_isEditing)
            {
                _renderState = UIEditBoxRenderState.MouseDown;
            }
            else
            {
                _renderState = UIEditBoxRenderState.MouseOver;
            }
        }

        public void OnMouseDownHandler(UIWidget sender, MouseEventArgs e)
        {
            _renderState = UIEditBoxRenderState.MouseDown;
        }

        public void OnMouseLeaveHandler(UIWidget sender, MouseEventArgs e)
        {
            if (_isEditing)
            {
                _renderState = UIEditBoxRenderState.MouseDown;
            }
            else
            {
                _renderState = UIEditBoxRenderState.Normal;
            }
        }

        public void OnMouseEnterHandler(UIWidget sender, MouseEventArgs e)
        {
            if (_isEditing)
            {
                _renderState = UIEditBoxRenderState.MouseDown;
            }
            else
            {
                _renderState = UIEditBoxRenderState.MouseOver;
            }
        }

        public bool IsEditing()
        {
            return _isEditing;
        }

        protected override void Render(RenderContext renderContext)
        {
            Sprite currentSprite = null;

            switch (_renderState)
            {
                case UIEditBoxRenderState.Normal:
                    currentSprite = _normalSprite;
                    break;
                case UIEditBoxRenderState.MouseDown:
                    currentSprite = _mouseDownSprite;
                    break;
                case UIEditBoxRenderState.MouseOver:
                    currentSprite = _mouseOverSprite;
                    break;
            }

            currentSprite = _mouseDownSprite;

            renderContext.AddDrawSpriteJob(X, Y, currentSprite, 1.0f, Width, Height);

            _textMesh.Update(X, Y, Text);
            renderContext.AddDrawMeshJob(X, Y, _textMesh, new SimpleMaterial(_editBoxFont.FontTexture));
        }

        protected internal override void ObjectUpdate()
        {
            base.ObjectUpdate();

            if (_cursorPosition < 0)
            {
                _cursorPosition = 0;
            }

            if (Manager.CurrentFocusObject == Widget)
            {
                if (!_isEditing)
                {
                    _cursorPosition = 0;
                    _isEditing = true;
                    _renderState = UIEditBoxRenderState.MouseDown;
                    _textMesh.SetRenderCursor(true);
                }
            }
            else
            {
                _textMesh.SetRenderCursor(false);
                _isEditing = false;
                _renderState = UIEditBoxRenderState.Normal;
            }

            if (_isEditing)
            {
                _textMesh.SetRenderCursorPosition(_cursorPosition);

                if (IOSystem.GetKeyDown(KeyCode.KeyLeftArrow))
                {
                    HandleKeyDown(KeyCode.KeyLeftArrow);
                }

                if (IOSystem.GetKeyDown(KeyCode.KeyRightArrow))
                {
                    HandleKeyDown(KeyCode.KeyRightArrow);
                }

                if (IOSystem.GetKeyDown(KeyCode.KeySpace))
                {
                    HandleKeyDown(KeyCode.KeySpace);
                }

                if (IOSystem.GetKeyDown(KeyCode.KeyBackspace))
                {
                    HandleKeyDown(KeyCode.KeyBackspace);
                }

                if (IOSystem.GetKeyDown(KeyCode.KeyDot))
                {
                    HandleKeyDown(KeyCode.KeyDot);
                }

                for (int i = (int)KeyCode.Key0; i <= (int)KeyCode.Key9; i++)
                {
                    if (IOSystem.GetKeyDown((KeyCode)i))
                    {
                        HandleKeyDown((KeyCode)i);
                    }
                }

                for (int i = (int)KeyCode.KeyA; i <= (int)KeyCode.KeyZ; i++)
                {
                    if (IOSystem.GetKeyDown((KeyCode)i))
                    {
                        HandleKeyDown((KeyCode)i);
                    }
                }
            }
        }

        protected internal override void OnTextChange()
        {
            if (TextChanged != null)
            {
                TextChanged();
            }
        }

        private void HandleKeyDown(KeyCode keyCode)
        {
            string currentText = Text;

            if (keyCode == KeyCode.KeyLeftArrow)
            {
                if (_cursorPosition > 0)
                {
                    _cursorPosition--;
                }
            }
            else if (keyCode == KeyCode.KeyRightArrow)
            {
                if (_cursorPosition < currentText.Length)
                {
                    _cursorPosition++;
                }
            }
            else if (keyCode == KeyCode.KeyBackspace)
            {
                if (_cursorPosition > 0)
                {
                    currentText = currentText.Remove(_cursorPosition - 1, 1);
                    //currentText.RemoveCharacter(_cursorPosition - 1);
                    _cursorPosition--;
                }
            }
            else if (keyCode == KeyCode.KeyDot)
            {
                currentText = currentText.Insert(_cursorPosition, ".");
                //currentText.AddCharacter('.', _cursorPosition);
                _cursorPosition++;
            }
            else
            {
                char c = (char)keyCode;

                if (IOSystem.GetKey(KeyCode.KeyShift))
                {
                    c = Char.ToUpper(c);
                }
                else
                {
                    c = Char.ToLower(c);
                }

                currentText = currentText.Insert(_cursorPosition, c.ToString());
                //currentText.AddCharacter((wchar_t)keyCode, _cursorPosition);
                _cursorPosition++;
            }

            Text = currentText;
        }
    }

    public enum UIEditBoxRenderState
    {
        Normal = 0,
        MouseDown = 1,
        MouseOver = 2,
    }
}
