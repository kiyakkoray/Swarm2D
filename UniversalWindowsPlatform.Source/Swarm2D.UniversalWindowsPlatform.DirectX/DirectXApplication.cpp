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
#include "DirectXApplication.h"
#include "Framework.h"
#include "DirectXTexture.h"

using namespace Swarm2D::UniversalWindowsPlatform::DirectX;
using namespace Platform;

void StartGame(IDirectXDomain^ directXDomain);

Framework* DirectXApplication::_framework = nullptr;

void DirectXApplication::Start(IDirectXDomain^ directXDomain)
{
	StartGame(directXDomain);
}

void DirectXApplication::Initialize(Framework* framework)
{
	_framework = framework;
}

void DirectXApplication::BeginFrame()
{
	_framework->BeginFrame();
}

void DirectXApplication::SwapBuffers()
{
	_framework->SwapBuffers();
}

int DirectXApplication::Width()
{
	return _framework->Width();
}

int DirectXApplication::Height()
{
	return _framework->Height();
}

void DirectXApplication::SetViewMatrix(float m00, float m01, float m02, float m03, float m10, float m11, float m12, float m13, float m20, float m21, float m22, float m23, float m30, float m31, float m32, float m33)
{
	::DirectX::XMFLOAT4X4 matrix;

	matrix.m[0][0] = m00;
	matrix.m[0][1] = m01;
	matrix.m[0][2] = m02;
	matrix.m[0][3] = m03;

	matrix.m[1][0] = m10;
	matrix.m[1][1] = m11;
	matrix.m[1][2] = m12;
	matrix.m[1][3] = m13;

	matrix.m[2][0] = m20;
	matrix.m[2][1] = m21;
	matrix.m[2][2] = m22;
	matrix.m[2][3] = m23;

	matrix.m[3][0] = m30;
	matrix.m[3][1] = m31;
	matrix.m[3][2] = m32;
	matrix.m[3][3] = m33;

	_framework->SetViewMatrix(matrix);
}

void DirectXApplication::SetWorldMatrix(float m00, float m01, float m02, float m03, float m10, float m11, float m12, float m13, float m20, float m21, float m22, float m23, float m30, float m31, float m32, float m33)
{
	::DirectX::XMFLOAT4X4 matrix;

	matrix.m[0][0] = m00;
	matrix.m[0][1] = m01;
	matrix.m[0][2] = m02;
	matrix.m[0][3] = m03;

	matrix.m[1][0] = m10;
	matrix.m[1][1] = m11;
	matrix.m[1][2] = m12;
	matrix.m[1][3] = m13;

	matrix.m[2][0] = m20;
	matrix.m[2][1] = m21;
	matrix.m[2][2] = m22;
	matrix.m[2][3] = m23;

	matrix.m[3][0] = m30;
	matrix.m[3][1] = m31;
	matrix.m[3][2] = m32;
	matrix.m[3][3] = m33;

	_framework->SetWorldMatrix(matrix);
}

void DirectXApplication::SetProjectionMatrix(float m00, float m01, float m02, float m03, float m10, float m11, float m12, float m13, float m20, float m21, float m22, float m23, float m30, float m31, float m32, float m33)
{
	::DirectX::XMFLOAT4X4 matrix;

	matrix.m[0][0] = m00;
	matrix.m[0][1] = m01;
	matrix.m[0][2] = m02;
	matrix.m[0][3] = m03;

	matrix.m[1][0] = m10;
	matrix.m[1][1] = m11;
	matrix.m[1][2] = m12;
	matrix.m[1][3] = m13;

	matrix.m[2][0] = m20;
	matrix.m[2][1] = m21;
	matrix.m[2][2] = m22;
	matrix.m[2][3] = m23;

	matrix.m[3][0] = m30;
	matrix.m[3][1] = m31;
	matrix.m[3][2] = m32;
	matrix.m[3][3] = m33;

	_framework->SetProjectionMatrix(matrix);
}

void DirectXApplication::DrawQuadArrays(const Platform::Array<float>^ vertices, const Platform::Array<float>^ uvs, int vertexCount, DirectXTexture^ texture)
{
	_framework->DrawQuadArrays(vertices->Data, uvs->Data, vertexCount, texture);
}

void DirectXApplication::DrawQuadArrays(float x, float y, const Platform::Array<float>^ vertices, const Platform::Array<float>^ uvs, int vertexCount, DirectXTexture^ texture)
{
	_framework->DrawQuadArrays(x, y, vertices->Data, uvs->Data, vertexCount, texture);
}

void DirectXApplication::DrawPolygon(const Platform::Array<float>^ vertices, int vertexCount, unsigned char red, unsigned char green, unsigned char blue, unsigned char alpha)
{
	_framework->DrawPolygon(vertices->Data, vertexCount, red, green, blue, alpha);
}

DirectXTexture^ DirectXApplication::CreateTexture()
{
	return ref new DirectXTexture(_framework);
}