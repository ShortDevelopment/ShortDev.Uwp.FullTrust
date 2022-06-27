#pragma once

#include <windows.h>
#include "ole2.h"
#include "Interfaces.h"

typedef HRESULT(CALLBACK* SetModernAppWindowProc)(HWND parentHwnd, HWND childHwnd);

class ComServerImpl : public IDispatch
{
public:
	ComServerImpl();
	virtual ~ComServerImpl();

	STDMETHODIMP QueryInterface(REFIID riid, void** pIFace);
	STDMETHODIMP_(DWORD)AddRef();
	STDMETHODIMP_(DWORD)Release();

	HRESULT STDMETHODCALLTYPE GetTypeInfoCount(__RPC__out UINT* pctinfo);
	HRESULT STDMETHODCALLTYPE GetTypeInfo(UINT iTInfo, LCID lcid, __RPC__deref_out_opt ITypeInfo** ppTInfo);
	HRESULT STDMETHODCALLTYPE GetIDsOfNames(__RPC__in REFIID riid, __RPC__in_ecount_full(cNames) LPOLESTR* rgszNames, __RPC__in_range(0, 16384) UINT cNames, LCID lcid, __RPC__out_ecount_full(cNames) DISPID* rgDispId);
	HRESULT STDMETHODCALLTYPE Invoke(_In_  DISPID dispIdMember, _In_  REFIID riid, _In_  LCID lcid, _In_  WORD wFlags, _In_  DISPPARAMS* pDispParams, _Out_opt_  VARIANT* pVarResult, _Out_opt_  EXCEPINFO* pExcepInfo, _Out_opt_  UINT* puArgErr);

	HRESULT STDMETHODCALLTYPE SetWindowCloak(HWND hWnd, bool isCloaked);
	HRESULT STDMETHODCALLTYPE SetModernWindow(HWND parentHwnd, HWND childHwnd);

private:
	DWORD	m_refCount;
	SetModernAppWindowProc SetModernAppWindowImpl;
};
