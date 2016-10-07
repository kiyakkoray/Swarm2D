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

#include "pch.h"
#include "Common\DeviceResources.h"
#include "Framework.h"
#include "DirectXApplication.h"

namespace Swarm2D
{
	namespace UniversalWindowsPlatform
	{
		namespace DirectX
		{
			ref class App sealed : public Windows::ApplicationModel::Core::IFrameworkView
			{
			public:
				App(IDirectXDomain^ directXDomain);

				// IFrameworkView Methods.
				virtual void Initialize(Windows::ApplicationModel::Core::CoreApplicationView^ applicationView);
				virtual void SetWindow(Windows::UI::Core::CoreWindow^ window);
				virtual void Load(Platform::String^ entryPoint);
				virtual void Run();
				virtual void Uninitialize();

			protected:
				// Application lifecycle event handlers.
				void OnActivated(Windows::ApplicationModel::Core::CoreApplicationView^ applicationView, Windows::ApplicationModel::Activation::IActivatedEventArgs^ args);
				void OnSuspending(Platform::Object^ sender, Windows::ApplicationModel::SuspendingEventArgs^ args);
				void OnResuming(Platform::Object^ sender, Platform::Object^ args);

				// Window event handlers.
				void OnWindowSizeChanged(Windows::UI::Core::CoreWindow^ sender, Windows::UI::Core::WindowSizeChangedEventArgs^ args);
				void OnVisibilityChanged(Windows::UI::Core::CoreWindow^ sender, Windows::UI::Core::VisibilityChangedEventArgs^ args);
				void OnWindowClosed(Windows::UI::Core::CoreWindow^ sender, Windows::UI::Core::CoreWindowEventArgs^ args);

				// DisplayInformation event handlers.
				void OnDpiChanged(Windows::Graphics::Display::DisplayInformation^ sender, Platform::Object^ args);
				void OnOrientationChanged(Windows::Graphics::Display::DisplayInformation^ sender, Platform::Object^ args);
				void OnDisplayContentsInvalidated(Windows::Graphics::Display::DisplayInformation^ sender, Platform::Object^ args);

			private:
				std::shared_ptr<DX::DeviceResources> m_deviceResources;
				std::unique_ptr<Framework> _framework;
				bool m_windowClosed;
				bool m_windowVisible;

				IDirectXDomain^ _directXDomain;
			};
		}
	}
}

ref class Direct3DApplicationSource sealed : Windows::ApplicationModel::Core::IFrameworkViewSource
{
public:
	Direct3DApplicationSource(Swarm2D::UniversalWindowsPlatform::DirectX::IDirectXDomain^ directXDomain);
	virtual Windows::ApplicationModel::Core::IFrameworkView^ CreateView();

private:
	Swarm2D::UniversalWindowsPlatform::DirectX::IDirectXDomain^ _directXDomain;
};
