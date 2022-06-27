#include "pch.h"
#include "ComServerImpl.h"

#include "Helper.h"
#include <dwmapi.h>
#pragma comment(lib, "Dwmapi.lib")

ComServerImpl::ComServerImpl()
{
	m_refCount = 0;

	HMODULE hMod = LoadLibrary(L"user32.dll");
	SetModernAppWindowImpl = (SetModernAppWindowProc)GetProcAddress(hMod, (LPCSTR)2568);
}

ComServerImpl::~ComServerImpl()
{
}

#pragma region IUnknown

STDMETHODIMP ComServerImpl::QueryInterface(REFIID riid, void** pIFace)
{
	if (pIFace == nullptr)
		return E_POINTER;

	if (riid == IID_IUnknown)
		*pIFace = (IUnknown*)(IDispatch*)this;
	else if (riid == IID_IDispatch)
		*pIFace = (IDispatch*)this;
	else
	{
		*pIFace = NULL;
		return E_NOINTERFACE;
	}

	AddRef();
	return S_OK;
}

STDMETHODIMP_(DWORD) ComServerImpl::AddRef()
{
	return ++m_refCount;
}

STDMETHODIMP_(DWORD) ComServerImpl::Release()
{
	if (--m_refCount == 0)
	{
		delete this;
		return 0;
	}
	return m_refCount;
}

HRESULT STDMETHODCALLTYPE ComServerImpl::GetTypeInfoCount(__RPC__out UINT* pctinfo) {
	*pctinfo = 0;
	return S_OK;
}

HRESULT STDMETHODCALLTYPE ComServerImpl::GetTypeInfo(UINT iTInfo, LCID lcid, __RPC__deref_out_opt ITypeInfo** ppTInfo) {
	return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE ComServerImpl::GetIDsOfNames(__RPC__in REFIID riid, __RPC__in_ecount_full(cNames) LPOLESTR* rgszNames, __RPC__in_range(0, 16384) UINT cNames, LCID lcid, __RPC__out_ecount_full(cNames) DISPID* rgDispId) {
	for (UINT i = 0; i < cNames; i++)
	{
		LPOLESTR currentName = rgszNames[i];
		if (currentName == L"SetWindowCloak")
			rgDispId[i] = 0;
		else if (currentName == L"SetModernWindow")
			rgDispId[i] = 1;
		else
			return DISP_E_UNKNOWNNAME;
	}
	return S_OK;
}

HRESULT STDMETHODCALLTYPE ComServerImpl::Invoke(_In_  DISPID dispIdMember, _In_  REFIID riid, _In_  LCID lcid, _In_  WORD wFlags, _In_  DISPPARAMS* pDispParams, _Out_opt_  VARIANT* pVarResult, _Out_opt_  EXCEPINFO* pExcepInfo, _Out_opt_  UINT* puArgErr) {
	HRESULT result;
	if (dispIdMember == 0)
		result = SetWindowCloak((HWND)pDispParams->rgvarg[0].lVal, pDispParams->rgvarg[1].boolVal);
	else if (dispIdMember == 1)
		result = SetModernWindow((HWND)pDispParams->rgvarg[0].lVal, (HWND)pDispParams->rgvarg[1].lVal);
	else
		return DISP_E_MEMBERNOTFOUND;

	DisplayNumber((int)result);

	VARIANT variant;
	variant.vt = VT_I4;
	variant.intVal = result;
	pVarResult = &variant;

	return S_OK;
}

#pragma endregion

HRESULT STDMETHODCALLTYPE ComServerImpl::SetWindowCloak(HWND hWnd, bool isCloaked) {
	BOOL value = isCloaked;
	return DwmSetWindowAttribute(hWnd, DWMWA_CLOAK, &value, sizeof(value));
}

HRESULT STDMETHODCALLTYPE ComServerImpl::SetModernWindow(HWND parentHwnd, HWND childHwnd) {
	SetModernAppWindowImpl(parentHwnd, childHwnd);

	return GetLastError();
}