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
#include "Framework.h"
#include "Common\DirectXHelper.h"
#include "DirectXTexture.h"

using namespace Windows::Foundation;
using namespace Windows::System::Threading;
using namespace Concurrency;
using namespace Swarm2D::UniversalWindowsPlatform::DirectX;
using namespace DirectX;

// Loads and initializes application assets when the application is loaded.
Framework::Framework(const std::shared_ptr<DX::DeviceResources>& deviceResources) :
	m_deviceResources(deviceResources),
	m_loadingComplete(false),
	m_indexCount(0)
{
	_width = 1280;
	_height = 720;

	m_DepthStencilState = nullptr;
	m_RasterizerState = nullptr;
	m_samplerState = nullptr;
	m_blendState = nullptr;
	_lastBindTexture = nullptr;

	 for (int i = 0; i < VertexBufferCount; i++)
	 {
		 _vertexBuffers[i] = nullptr;
	 }

	 _currentVertexBuffer = 0;

	// Register to be notified if the Device is lost or recreated
	m_deviceResources->RegisterDeviceNotify(this);

	DirectXApplication::Initialize(this);

	CreateDeviceDependentResources();
	CreateWindowSizeDependentResources();

	// TODO: Change the timer settings if you want something other than the default variable timestep mode.
	// e.g. for 60 FPS fixed timestep update logic, call:
	/*
	m_timer.SetFixedTimeStep(true);
	m_timer.SetTargetElapsedSeconds(1.0 / 60);
	*/
}

Framework::~Framework()
{
	// Deregister device notification
	m_deviceResources->RegisterDeviceNotify(nullptr);
}

void Framework::BeginFrame()
{
	//_lastBindTexture = nullptr;

	XMStoreFloat4x4(&_modelMatrix, XMMatrixIdentity());
	XMStoreFloat4x4(&_viewMatrix, XMMatrixIdentity());
	XMStoreFloat4x4(&_projectionMatrix, XMMatrixIdentity());

	auto context = m_deviceResources->GetD3DDeviceContext();

	if (m_loadingComplete)
	{
		// Reset the viewport to target the whole screen.
		auto viewport = m_deviceResources->GetScreenViewport();
		context->RSSetViewports(1, &viewport);

		// Reset render targets to the screen.
		ID3D11RenderTargetView *const targets[1] = { m_deviceResources->GetBackBufferRenderTargetView() };
		context->OMSetRenderTargets(1, targets, m_deviceResources->GetDepthStencilView());

		// Clear the back buffer and depth stencil view.
		context->ClearRenderTargetView(m_deviceResources->GetBackBufferRenderTargetView(), ::DirectX::Colors::Black);
		//context->ClearDepthStencilView(m_deviceResources->GetDepthStencilView(), D3D11_CLEAR_DEPTH | D3D11_CLEAR_STENCIL, 1.0f, 0);

		//context->OMSetDepthStencilState(m_DepthStencilState, 0);
		//context->RSSetState(m_RasterizerState);
	}
}

void Framework::SwapBuffers()
{
	if (m_loadingComplete)
	{
		m_deviceResources->Present();
	}
}

int Framework::Width()
{
	return _width;
}

int Framework::Height()
{
	return _height;
}

void Framework::SetViewMatrix(const ::DirectX::XMFLOAT4X4& matrix)
{
	_viewMatrix = matrix;
}

void Framework::SetWorldMatrix(const ::DirectX::XMFLOAT4X4& matrix)
{
	_modelMatrix = matrix;
}

void Framework::SetProjectionMatrix(const ::DirectX::XMFLOAT4X4& matrix)
{
	XMMATRIX matrixToTranspose = XMLoadFloat4x4(&matrix);
	XMMATRIX transposedMatrix = XMMatrixTranspose(matrixToTranspose);

	_projectionMatrix = matrix;
}

void Framework::DrawQuadArrays(float vertices[], float uvs[], int vertexCount, DirectXTexture^ texture)
{
	if (m_loadingComplete)
	{
		int quadCount = vertexCount / 4;
		int indexCount = 6 * quadCount;

		auto context = m_deviceResources->GetD3DDeviceContext();

		m_constantBufferData.view = _viewMatrix;
		m_constantBufferData.model = _modelMatrix;
		m_constantBufferData.projection = _projectionMatrix;

		VertexPositionColor verticesToSend[2048];

		for (int i = 0; i < vertexCount; i++)
		{
			verticesToSend[i].pos = { vertices[2 * i], vertices[2 * i + 1] };
			verticesToSend[i].uv = { uvs[2 * i], uvs[2 * i + 1] };
		}

		// Prepare the constant buffer to send it to the graphics device.
		context->UpdateSubresource1(m_constantBuffer.Get(), 0, NULL, &m_constantBufferData, 0, 0, 0);

		{
			D3D11_MAPPED_SUBRESOURCE mappedResource;
			ZeroMemory(&mappedResource, sizeof(D3D11_MAPPED_SUBRESOURCE));
			context->Map(_vertexBuffers[_currentVertexBuffer], 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
			memcpy(mappedResource.pData, verticesToSend, sizeof(VertexPositionColor) * vertexCount);
			context->Unmap(_vertexBuffers[_currentVertexBuffer], 0);
		}

		ID3D11ShaderResourceView* textureView = texture->GetView();

		// Each vertex is one instance of the VertexPositionColor struct.
		UINT stride = sizeof(VertexPositionColor);
		UINT offset = 0;
		context->IASetVertexBuffers(0, 1, &_vertexBuffers[_currentVertexBuffer], &stride, &offset);
		context->IASetIndexBuffer(m_indexBuffer.Get(), DXGI_FORMAT_R16_UINT, 0);
		context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
		context->IASetInputLayout(m_inputLayout.Get());

		// Attach our vertex shader.
		context->VSSetShader(m_vertexShader.Get(), nullptr, 0);

		// Send the constant buffer to the graphics device.
		context->VSSetConstantBuffers1(0, 1, m_constantBuffer.GetAddressOf(), nullptr, nullptr);			

		// Set the sampler state in the pixel shader.
		context->PSSetSamplers(0, 1, &m_samplerState);
		context->PSSetShaderResources(0, 1, &textureView);

		// Attach our pixel shader.
		context->PSSetShader(m_pixelShader.Get(), nullptr, 0);

		context->DrawIndexed(indexCount, 0, 0);

		_currentVertexBuffer++;
		if (_currentVertexBuffer >= VertexBufferCount) _currentVertexBuffer = 0;
	}
}

void Framework::DrawQuadArrays(float x, float y, float vertices[], float uvs[], int vertexCount, DirectXTexture^ texture)
{
	if (m_loadingComplete)
	{
		int quadCount = vertexCount / 4;
		int indexCount = 6 * quadCount;

		auto context = m_deviceResources->GetD3DDeviceContext();

		XMMATRIX viewMatrix = XMLoadFloat4x4(&_viewMatrix);
		XMMATRIX translation = XMMatrixTranspose(XMMatrixTranslation(x, y, 0));
		XMMATRIX realViewMatrix = translation * viewMatrix;

		XMStoreFloat4x4(&m_constantBufferData.view, realViewMatrix);
		m_constantBufferData.model = _modelMatrix;
		m_constantBufferData.projection = _projectionMatrix;

		VertexPositionColor verticesToSend[2048];

		for (int i = 0; i < vertexCount; i++)
		{
			verticesToSend[i].pos = { vertices[2 * i], vertices[2 * i + 1] };
			verticesToSend[i].uv = { uvs[2 * i], uvs[2 * i + 1] };
		}

		// Prepare the constant buffer to send it to the graphics device.
		context->UpdateSubresource1(m_constantBuffer.Get(), 0, NULL, &m_constantBufferData, 0, 0, 0);

		{
			D3D11_MAPPED_SUBRESOURCE mappedResource;
			ZeroMemory(&mappedResource, sizeof(D3D11_MAPPED_SUBRESOURCE));
			context->Map(_vertexBuffers[_currentVertexBuffer], 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
			memcpy(mappedResource.pData, verticesToSend, sizeof(VertexPositionColor) * vertexCount);
			context->Unmap(_vertexBuffers[_currentVertexBuffer], 0);
		}

		ID3D11ShaderResourceView* textureView = texture->GetView();

		// Each vertex is one instance of the VertexPositionColor struct.
		UINT stride = sizeof(VertexPositionColor);
		UINT offset = 0;
		context->IASetVertexBuffers(0, 1, &_vertexBuffers[_currentVertexBuffer], &stride, &offset);
		context->IASetIndexBuffer(m_indexBuffer.Get(), DXGI_FORMAT_R16_UINT, 0);
		context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
		context->IASetInputLayout(m_inputLayout.Get());

		// Attach our vertex shader.
		context->VSSetShader(m_vertexShader.Get(), nullptr, 0);

		// Send the constant buffer to the graphics device.
		context->VSSetConstantBuffers1(0, 1, m_constantBuffer.GetAddressOf(), nullptr, nullptr);

		// Set the sampler state in the pixel shader.
		context->PSSetSamplers(0, 1, &m_samplerState);
		context->PSSetShaderResources(0, 1, &textureView);

		// Attach our pixel shader.
		context->PSSetShader(m_pixelShader.Get(), nullptr, 0);

		context->DrawIndexed(indexCount, 0, 0);

		_currentVertexBuffer++;
		if (_currentVertexBuffer >= VertexBufferCount) _currentVertexBuffer = 0;
	}
}

void Framework::DrawPolygon(float vertices[], int vertexCount, unsigned char red, unsigned char green, unsigned char blue, unsigned char alpha)
{
	if (m_loadingComplete)
	{
		auto context = m_deviceResources->GetD3DDeviceContext();

		m_constantBufferData.view = _viewMatrix;
		m_constantBufferData.model = _modelMatrix;
		m_constantBufferData.projection = _projectionMatrix;

		VertexPosition verticesToSend[2048];

		// Prepare the constant buffer to send it to the graphics device.
		context->UpdateSubresource1(m_constantBuffer.Get(), 0, NULL, &m_constantBufferData, 0, 0, 0);

		{
			int indexCount = 3 * (vertexCount - 2);

			{
				D3D11_MAPPED_SUBRESOURCE mappedResource;
				ZeroMemory(&mappedResource, sizeof(D3D11_MAPPED_SUBRESOURCE));
				context->Map(_vertexBuffers[_currentVertexBuffer], 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
				memcpy(mappedResource.pData, vertices, sizeof(VertexPosition) * vertexCount);
				context->Unmap(_vertexBuffers[_currentVertexBuffer], 0);
			}

			// Each vertex is one instance of the VertexPositionColor struct.
			UINT stride = sizeof(VertexPosition);
			UINT offset = 0;
			context->IASetVertexBuffers(0, 1, &_vertexBuffers[_currentVertexBuffer], &stride, &offset);
			context->IASetIndexBuffer(_polygonIndexBuffer.Get(), DXGI_FORMAT_R16_UINT, 0);
			context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
			context->IASetInputLayout(_polygonInputLayout.Get());

			// Attach our vertex shader.
			context->VSSetShader(_polygonVertexShader.Get(), nullptr, 0);

			// Send the constant buffer to the graphics device.
			context->VSSetConstantBuffers1(0, 1, m_constantBuffer.GetAddressOf(), nullptr, nullptr);

			// Attach our pixel shader.
			context->PSSetShader(_polygonPixelShader.Get(), nullptr, 0);

			context->DrawIndexed(indexCount, 0, 0);

			_currentVertexBuffer++;
			if (_currentVertexBuffer >= VertexBufferCount) _currentVertexBuffer = 0;
		}

		{
			int indexCount = vertexCount * 2;

			for (int i = 0; i < vertexCount; i++)
			{
				float x1 = vertices[2 * i];
				float y1 = vertices[2 * i + 1];

				float x2 = 0;
				float y2 = 0;

				if (i + 1 == vertexCount)
				{
					x2 = vertices[0];
					y2 = vertices[1];
				}
				else
				{
					x2 = vertices[2 * i + 2];
					y2 = vertices[2 * i + 3];
				}

				verticesToSend[2 * i].pos = { x1, y1 };
				verticesToSend[2 * i + 1].pos = { x2, y2 };
			}

			{
				D3D11_MAPPED_SUBRESOURCE mappedResource;
				ZeroMemory(&mappedResource, sizeof(D3D11_MAPPED_SUBRESOURCE));
				context->Map(_vertexBuffers[_currentVertexBuffer], 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
				memcpy(mappedResource.pData, verticesToSend, sizeof(VertexPosition) * vertexCount * 2);
				context->Unmap(_vertexBuffers[_currentVertexBuffer], 0);
			}

			// Each vertex is one instance of the VertexPositionColor struct.
			UINT stride = sizeof(VertexPosition);
			UINT offset = 0;
			context->IASetVertexBuffers(0, 1, &_vertexBuffers[_currentVertexBuffer], &stride, &offset);
			context->IASetIndexBuffer(_linesIndexBuffer.Get(), DXGI_FORMAT_R16_UINT, 0);
			context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_LINELIST);
			context->IASetInputLayout(_polygonInputLayout.Get());

			// Attach our vertex shader.
			context->VSSetShader(_polygonVertexShader.Get(), nullptr, 0);

			// Send the constant buffer to the graphics device.
			context->VSSetConstantBuffers1(0, 1, m_constantBuffer.GetAddressOf(), nullptr, nullptr);

			// Attach our pixel shader.
			context->PSSetShader(_polygonPixelShader.Get(), nullptr, 0);

			context->DrawIndexed(indexCount, 0, 0);

			_currentVertexBuffer++;
			if (_currentVertexBuffer >= VertexBufferCount) _currentVertexBuffer = 0;
		}
	}
}

// Updates application state when the window size changes (e.g. device orientation change)
void Framework::CreateWindowSizeDependentResources()
{
	Size outputSize = m_deviceResources->GetOutputSize();

	_width = (int)outputSize.Width;
	_height = (int)outputSize.Height;
}

// Notifies renderers that device resources need to be released.
void Framework::OnDeviceLost()
{
	ReleaseDeviceDependentResources();
}

// Notifies renderers that device resources may now be recreated.
void Framework::OnDeviceRestored()
{
	CreateDeviceDependentResources();
	CreateWindowSizeDependentResources();
}

void Framework::CreateDeviceDependentResources()
{
	auto loadVSTask = DX::ReadDataAsync(L"Swarm2D\\VertexShader.cso");
	auto loadPSTask = DX::ReadDataAsync(L"Swarm2D\\PixelShader.cso");

	auto createVSTask = loadVSTask.then([this](const std::vector<byte>& fileData)
	{
		DX::ThrowIfFailed(m_deviceResources->GetD3DDevice()->CreateVertexShader(&fileData[0], fileData.size(), nullptr, &m_vertexShader));

		static const D3D11_INPUT_ELEMENT_DESC vertexDesc[] =
		{
			{ "POSITION", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
			{ "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT, D3D11_INPUT_PER_VERTEX_DATA, 0 },
			//{ "COLOR", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
		};

		DX::ThrowIfFailed(m_deviceResources->GetD3DDevice()->CreateInputLayout(vertexDesc, ARRAYSIZE(vertexDesc), &fileData[0], fileData.size(), &m_inputLayout));
	});

	auto createPSTask = loadPSTask.then([this](const std::vector<byte>& fileData)
	{
		DX::ThrowIfFailed(m_deviceResources->GetD3DDevice()->CreatePixelShader(&fileData[0], fileData.size(), nullptr, &m_pixelShader));

		CD3D11_BUFFER_DESC constantBufferDesc(sizeof(ModelViewProjectionConstantBuffer), D3D11_BIND_CONSTANT_BUFFER);
		DX::ThrowIfFailed(m_deviceResources->GetD3DDevice()->CreateBuffer(&constantBufferDesc, nullptr, &m_constantBuffer));		
	});

	auto loadPolygonVSTask = DX::ReadDataAsync(L"Swarm2D\\PolygonVertexShader.cso");
	auto loadPolygonPSTask = DX::ReadDataAsync(L"Swarm2D\\PolygonPixelShader.cso");

	auto createPolygonVSTask = loadPolygonVSTask.then([this](const std::vector<byte>& fileData)
	{
		DX::ThrowIfFailed(m_deviceResources->GetD3DDevice()->CreateVertexShader(&fileData[0], fileData.size(), nullptr, &_polygonVertexShader));

		static const D3D11_INPUT_ELEMENT_DESC vertexDesc[] =
		{
			{ "POSITION", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
			//{ "COLOR", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
		};

		DX::ThrowIfFailed(m_deviceResources->GetD3DDevice()->CreateInputLayout(vertexDesc, ARRAYSIZE(vertexDesc), &fileData[0], fileData.size(), &_polygonInputLayout));
	});

	auto createPolygonPSTask = loadPolygonPSTask.then([this](const std::vector<byte>& fileData)
	{
		DX::ThrowIfFailed(m_deviceResources->GetD3DDevice()->CreatePixelShader(&fileData[0], fileData.size(), nullptr, &_polygonPixelShader));
	});

	// Once both shaders are loaded, create the mesh.
	auto createCubeTask = (createPSTask && createVSTask && createPolygonVSTask && createPolygonPSTask).then([this]()
	{
		int quadCount = 512;
		int vertexCount = 4 * quadCount; // 2048
		int indexCount = quadCount * 6; //3072

		VertexPositionColor vertices[2048];		

		for (int i = 0; i < VertexBufferCount; i++)
		{
			D3D11_SUBRESOURCE_DATA vertexBufferData = { 0 };
			vertexBufferData.pSysMem = vertices;
			vertexBufferData.SysMemPitch = 0;
			vertexBufferData.SysMemSlicePitch = 0;

			CD3D11_BUFFER_DESC vertexBufferDescription(sizeof(vertices), D3D11_BIND_VERTEX_BUFFER);
			vertexBufferDescription.Usage = D3D11_USAGE_DYNAMIC;
			vertexBufferDescription.BindFlags = D3D11_BIND_VERTEX_BUFFER;
			vertexBufferDescription.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
			vertexBufferDescription.MiscFlags = 0;
			vertexBufferDescription.StructureByteStride = 0;

			DX::ThrowIfFailed(m_deviceResources->GetD3DDevice()->CreateBuffer(&vertexBufferDescription, &vertexBufferData, &_vertexBuffers[i]));
		}

		{
			unsigned short indices[3072];

			for (int i = 0; i < quadCount; i++)
			{
				indices[6 * i + 0] = 4 * i + 0;
				indices[6 * i + 1] = 4 * i + 1;
				indices[6 * i + 2] = 4 * i + 2;

				indices[6 * i + 3] = 4 * i + 2;
				indices[6 * i + 4] = 4 * i + 3;
				indices[6 * i + 5] = 4 * i + 0;
			}

			D3D11_SUBRESOURCE_DATA indexBufferData = { 0 };
			indexBufferData.pSysMem = indices;
			indexBufferData.SysMemPitch = 0;
			indexBufferData.SysMemSlicePitch = 0;
			CD3D11_BUFFER_DESC indexBufferDesc(sizeof(indices), D3D11_BIND_INDEX_BUFFER);
			DX::ThrowIfFailed(m_deviceResources->GetD3DDevice()->CreateBuffer(&indexBufferDesc, &indexBufferData, &m_indexBuffer));
		}

		{
			unsigned short indices[3072];

			for (int i = 0; i < 1024; i++)
			{
				indices[3 * i] = 0;
				indices[3 * i + 1] = i + 1;
				indices[3 * i + 2] = i + 2;
			}

			D3D11_SUBRESOURCE_DATA indexBufferData = { 0 };
			indexBufferData.pSysMem = indices;
			indexBufferData.SysMemPitch = 0;
			indexBufferData.SysMemSlicePitch = 0;
			CD3D11_BUFFER_DESC indexBufferDesc(sizeof(indices), D3D11_BIND_INDEX_BUFFER);
			DX::ThrowIfFailed(m_deviceResources->GetD3DDevice()->CreateBuffer(&indexBufferDesc, &indexBufferData, &_polygonIndexBuffer));
		}

		{
			unsigned short indices[3072];

			for (int i = 0; i < 3072; i++)
			{
				indices[i] = i;
			}

			D3D11_SUBRESOURCE_DATA indexBufferData = { 0 };
			indexBufferData.pSysMem = indices;
			indexBufferData.SysMemPitch = 0;
			indexBufferData.SysMemSlicePitch = 0;
			CD3D11_BUFFER_DESC indexBufferDesc(sizeof(indices), D3D11_BIND_INDEX_BUFFER);
			DX::ThrowIfFailed(m_deviceResources->GetD3DDevice()->CreateBuffer(&indexBufferDesc, &indexBufferData, &_linesIndexBuffer));
		}
	});

	// Once the cube is loaded, the object is ready to be rendered.
	createCubeTask.then([this]()
	{
		auto device = m_deviceResources->GetD3DDevice();
		auto deviceContext = m_deviceResources->GetD3DDeviceContext();

		{
			D3D11_DEPTH_STENCIL_DESC depthStencilDesc;
			depthStencilDesc.DepthEnable = false;
			depthStencilDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
			depthStencilDesc.DepthFunc = D3D11_COMPARISON_ALWAYS;
			depthStencilDesc.StencilEnable = false;
			depthStencilDesc.StencilReadMask = 0xFF;
			depthStencilDesc.StencilWriteMask = 0xFF;

			// Stencil operations if pixel is front-facing
			depthStencilDesc.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
			depthStencilDesc.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_INCR;
			depthStencilDesc.FrontFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;
			depthStencilDesc.FrontFace.StencilFunc = D3D11_COMPARISON_ALWAYS;

			// Stencil operations if pixel is back-facing
			depthStencilDesc.BackFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
			depthStencilDesc.BackFace.StencilDepthFailOp = D3D11_STENCIL_OP_DECR;
			depthStencilDesc.BackFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;
			depthStencilDesc.BackFace.StencilFunc = D3D11_COMPARISON_ALWAYS;

			device->CreateDepthStencilState(&depthStencilDesc, &m_DepthStencilState);
			deviceContext->OMSetDepthStencilState(m_DepthStencilState, 0);
		}

		{
			D3D11_RASTERIZER_DESC rasterizedDesc;

			rasterizedDesc.FillMode = D3D11_FILL_SOLID;
			rasterizedDesc.CullMode = D3D11_CULL_NONE;
			rasterizedDesc.FrontCounterClockwise = true;
			rasterizedDesc.DepthBias = 0;
			rasterizedDesc.SlopeScaledDepthBias	= 0.0f;
			rasterizedDesc.DepthBiasClamp = 0.0f;
			rasterizedDesc.DepthClipEnable = false;
			rasterizedDesc.ScissorEnable = false;
			rasterizedDesc.MultisampleEnable = false;
			rasterizedDesc.AntialiasedLineEnable = false;

			device->CreateRasterizerState(&rasterizedDesc, &m_RasterizerState);
			deviceContext->RSSetState(m_RasterizerState);
		}

		{
			D3D11_SAMPLER_DESC samplerDesc;

			// Create a texture sampler state description.
			samplerDesc.Filter = D3D11_FILTER_MIN_LINEAR_MAG_MIP_POINT;
			samplerDesc.AddressU = D3D11_TEXTURE_ADDRESS_CLAMP;
			samplerDesc.AddressV = D3D11_TEXTURE_ADDRESS_CLAMP;
			samplerDesc.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
			samplerDesc.MipLODBias = 0.0f;
			samplerDesc.MaxAnisotropy = 0;
			samplerDesc.ComparisonFunc = D3D11_COMPARISON_NEVER;
			samplerDesc.BorderColor[0] = 1;
			samplerDesc.BorderColor[1] = 1;
			samplerDesc.BorderColor[2] = 1;
			samplerDesc.BorderColor[3] = 1;
			samplerDesc.MinLOD = 0;
			samplerDesc.MaxLOD = 0;

			// Create the texture sampler state.
			device->CreateSamplerState(&samplerDesc, &m_samplerState);
		}

		{
			D3D11_BLEND_DESC blendDesc;

			ZeroMemory(&blendDesc, sizeof(D3D11_BLEND_DESC));
			blendDesc.RenderTarget[0].BlendEnable = true;

			blendDesc.RenderTarget[0].SrcBlend = D3D11_BLEND_SRC_ALPHA;
			blendDesc.RenderTarget[0].DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
			blendDesc.RenderTarget[0].BlendOp = D3D11_BLEND_OP_ADD;
			blendDesc.RenderTarget[0].SrcBlendAlpha = D3D11_BLEND_ONE;
			blendDesc.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_ZERO;
			blendDesc.RenderTarget[0].BlendOpAlpha = D3D11_BLEND_OP_ADD;

			blendDesc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;

			float blendFactor[4] = { 0.0f, 0.0f, 0.0f, 0.0f };
			UINT sampleMask = 0xffffffff;

			device->CreateBlendState(&blendDesc, &m_blendState);
			deviceContext->OMSetBlendState(m_blendState, blendFactor, sampleMask);
		}

		m_loadingComplete = true;
	});
}

void Framework::ReleaseDeviceDependentResources()
{
	m_loadingComplete = false;
	m_vertexShader.Reset();
	m_inputLayout.Reset();
	m_pixelShader.Reset();
	m_constantBuffer.Reset();
	m_indexBuffer.Reset();

	for (int i = 0; i < VertexBufferCount; i++)
	{
		_vertexBuffers[i]->Release();
	}
}

std::shared_ptr<DX::DeviceResources>& Framework::GetDeviceResources()
{
	return m_deviceResources;
}