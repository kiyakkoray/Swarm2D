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

#pragma once

namespace Swarm2D
{
	namespace UniversalWindowsPlatform
	{
		namespace DirectX
		{
			struct ModelViewProjectionConstantBuffer
			{
				::DirectX::XMFLOAT4X4 model;
				::DirectX::XMFLOAT4X4 view;
				::DirectX::XMFLOAT4X4 projection;

				float test1;
				float test2;
				float test3;
				float test4;
			};

			struct VertexPositionColor
			{
				::DirectX::XMFLOAT2 pos;
				::DirectX::XMFLOAT2 uv;
			};

			struct VertexPosition
			{
				::DirectX::XMFLOAT2 pos;
			};
		}
	}
}