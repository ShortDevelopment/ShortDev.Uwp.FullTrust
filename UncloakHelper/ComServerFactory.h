#pragma once

#include <windows.h>
#include "ole2.h"

class ComServerFactory : public IClassFactory
{
public:
	ComServerFactory();
	virtual ~ComServerFactory();

	// IUnknown
	STDMETHODIMP QueryInterface(REFIID riid, void** pIFace);
	STDMETHODIMP_(ULONG)AddRef();
	STDMETHODIMP_(ULONG)Release();

	// IClassFactory
	STDMETHODIMP LockServer(BOOL fLock);
	STDMETHODIMP CreateInstance(LPUNKNOWN pUnkOuter, REFIID riid, void** ppv);

private:

	ULONG m_refCount;
};