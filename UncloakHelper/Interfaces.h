#pragma once

#include "windows.h"

MIDL_INTERFACE("2DFCC24C-DB98-4A8D-9EC1-E685BCD8D71D")
ICommunication : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE SetWindowCloak(HWND hWnd, bool isCloaked) = 0;
    virtual HRESULT STDMETHODCALLTYPE SetModernWindow(HWND parentHwnd, HWND childHwnd) = 0;
    
};

// {2DFCC24C-DB98-4A8D-9EC1-E685BCD8D71D}
static const GUID IID_ICommunication =
{ 0x2dfcc24c, 0xdb98, 0x4a8d, { 0x9e, 0xc1, 0xe6, 0x85, 0xbc, 0xd8, 0xd7, 0x1d } };

// {F77F471B-0A90-4FCB-ADD9-191542432BE9}
static const GUID CLSID_ComServer =
{ 0xf77f471b, 0xa90, 0x4fcb, { 0xad, 0xd9, 0x19, 0x15, 0x42, 0x43, 0x2b, 0xe9 } };
