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
			class Framework;
			ref class DirectXTexture;

			public interface class IDirectXDomain
			{
				virtual void Update() = 0;
			};

			public ref class DirectXApplication sealed
			{
			public:
				static void Start(IDirectXDomain^ directXDomain);

				static void BeginFrame();
				static void SwapBuffers();

				static int Width();
				static int Height();

				static void SetViewMatrix(float m00, float m01, float m02, float m03, float m10, float m11, float m12, float m13, float m20, float m21, float m22, float m23, float m30, float m31, float m32, float m33);
				static void SetWorldMatrix(float m00, float m01, float m02, float m03, float m10, float m11, float m12, float m13, float m20, float m21, float m22, float m23, float m30, float m31, float m32, float m33);
				static void SetProjectionMatrix(float m00, float m01, float m02, float m03, float m10, float m11, float m12, float m13, float m20, float m21, float m22, float m23, float m30, float m31, float m32, float m33);

				static void DrawQuadArrays(const Platform::Array<float>^ vertices, const Platform::Array<float>^ uvs, int vertexCount, DirectXTexture^ texture);
				static void DrawQuadArrays(float x, float y, const Platform::Array<float>^ vertices, const Platform::Array<float>^ uvs, int vertexCount, DirectXTexture^ texture);
				static void DrawPolygon(const Platform::Array<float>^ vertices, int vertexCount, unsigned char red, unsigned char blue, unsigned char green, unsigned char alpha);

				static DirectXTexture^ CreateTexture();

			internal:
				static void Initialize(Framework* renderer);

			private:
				static Framework* _framework;
			};
		}
	}
}
