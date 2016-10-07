/******************************************************************************
Copyright (c) 2016 Koray Kiyakoglu

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

#include "pch.h"
#include "DirectXTexture.h"
#include "Framework.h"

#include "DDSTextureLoader.h"

using namespace Swarm2D::UniversalWindowsPlatform::DirectX;
using namespace Platform;

DirectXTexture::DirectXTexture(Framework* framework)
{
	_framework = framework;
	_texture = nullptr;
	_textureView = nullptr;

	_width = 0;
	_height = 0;
}

void DirectXTexture::Load(const Platform::Array<unsigned char>^ data)
{
	auto deviceResources = _framework->GetDeviceResources();
	auto device = deviceResources->GetD3DDevice();
	auto deviceContext = deviceResources->GetD3DDeviceContext();

	UINT width = 2048;
	UINT height = 2048;

	::DirectX::CreateDDSTextureFromMemory(device, deviceContext, data->Data, data->Length, &_texture, &_textureView, &width, &height);

	_width = width;
	_height = height;
}

void DirectXTexture::Delete()
{
	if (_texture != nullptr)
	{
		_texture->Release();
	}
}

ID3D11ShaderResourceView* DirectXTexture::GetView()
{
	return _textureView;
}

int DirectXTexture::GetWidth()
{
	return _width;
}

int DirectXTexture::GetHeight()
{
	return _height;
}