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

#include "Common\StepTimer.h"
#include "Common\DeviceResources.h"
#include "ShaderStructures.h"
#include "DirectXApplication.h"

#define VertexBufferCount 1024

namespace Swarm2D
{
	namespace UniversalWindowsPlatform
	{
		namespace DirectX
		{
			class Framework : public DX::IDeviceNotify
			{
			public:
				Framework(const std::shared_ptr<DX::DeviceResources>& deviceResources);
				~Framework();

				void BeginFrame();
				void SwapBuffers();

				int Width();
				int Height();

				void SetViewMatrix(const ::DirectX::XMFLOAT4X4& matrix);
				void SetWorldMatrix(const ::DirectX::XMFLOAT4X4& matrix);
				void SetProjectionMatrix(const ::DirectX::XMFLOAT4X4& matrix);

				void DrawQuadArrays(float vertices[], float uvs[], int vertexCount, DirectXTexture^ texture);
				void DrawQuadArrays(float x, float y, float vertices[], float uvs[], int vertexCount, DirectXTexture^ texture);
				void DrawPolygon(float vertices[], int vertexCount, unsigned char red, unsigned char green, unsigned char blue, unsigned char alpha);

				void CreateWindowSizeDependentResources();
				void CreateDeviceDependentResources();
				void ReleaseDeviceDependentResources();

				// IDeviceNotify
				virtual void OnDeviceLost();
				virtual void OnDeviceRestored();

				std::shared_ptr<DX::DeviceResources>& GetDeviceResources();

			private:
				// Cached pointer to device resources.
				std::shared_ptr<DX::DeviceResources> m_deviceResources;

				Microsoft::WRL::ComPtr<ID3D11Buffer>		m_constantBuffer;

				Microsoft::WRL::ComPtr<ID3D11InputLayout>	m_inputLayout;
				Microsoft::WRL::ComPtr<ID3D11Buffer>		m_indexBuffer;
				Microsoft::WRL::ComPtr<ID3D11VertexShader>	m_vertexShader;
				Microsoft::WRL::ComPtr<ID3D11PixelShader>	m_pixelShader;				

				Microsoft::WRL::ComPtr<ID3D11Buffer>		_polygonIndexBuffer;
				Microsoft::WRL::ComPtr<ID3D11Buffer>		_linesIndexBuffer;
				Microsoft::WRL::ComPtr<ID3D11InputLayout>	_polygonInputLayout;
				Microsoft::WRL::ComPtr<ID3D11VertexShader>	_polygonVertexShader;
				Microsoft::WRL::ComPtr<ID3D11PixelShader>	_polygonPixelShader;
				
				ID3D11Buffer* _vertexBuffers[VertexBufferCount];
				int _currentVertexBuffer;

				ID3D11DepthStencilState* m_DepthStencilState;
				ID3D11RasterizerState* m_RasterizerState;
				ID3D11SamplerState* m_samplerState;
				ID3D11BlendState* m_blendState;

				// System resources for cube geometry.
				ModelViewProjectionConstantBuffer	m_constantBufferData;
				uint32	m_indexCount;

				// Variables used with the rendering loop.
				bool	m_loadingComplete;

				int _width;
				int _height;

				::DirectX::XMFLOAT4X4 _viewMatrix;
				::DirectX::XMFLOAT4X4 _modelMatrix;
				::DirectX::XMFLOAT4X4 _projectionMatrix;

				ID3D11ShaderResourceView* _lastBindTexture;
			};
		}
	}
}