#include "pch.h"
#include "ComServerFactory.h"
#include "ComServerImpl.h"

ComServerFactory::ComServerFactory()
{
	m_refCount = 0;
}

ComServerFactory::~ComServerFactory()
{
}

#pragma region IUknown

STDMETHODIMP ComServerFactory::QueryInterface(REFIID riid, void** pIFace)
{
	if (pIFace == nullptr) return E_POINTER;

	if (riid == IID_IUnknown)
		*pIFace = (IUnknown*)this;
	else if (riid == IID_IClassFactory)
		*pIFace = (IClassFactory*)this;
	else
	{
		*pIFace = NULL;
		return E_NOINTERFACE;
	}
	AddRef();
	return S_OK;
}

STDMETHODIMP_(ULONG) ComServerFactory::AddRef()
{
	return ++m_refCount;
}

STDMETHODIMP_(ULONG) ComServerFactory::Release()
{
	if (--m_refCount == 0)
	{
		delete this;
		return 0;
	}
	return m_refCount;
}

#pragma endregion

#pragma region IClassFactory

STDMETHODIMP ComServerFactory::LockServer(BOOL fLock)
{
	return S_OK;
}

STDMETHODIMP ComServerFactory::CreateInstance(LPUNKNOWN pUnkOuter, REFIID riid, void** ppv)
{
	if (pUnkOuter != NULL)
		return CLASS_E_NOAGGREGATION;

	ComServerImpl* pInstance = NULL;
	HRESULT hr;

	pInstance = new ComServerImpl();
	hr = pInstance->QueryInterface(riid, ppv);

	if (FAILED(hr))
		delete pInstance;

	return hr;
}

#pragma endregion