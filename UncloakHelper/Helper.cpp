#include "pch.h"
#include "Helper.h"

HRESULT DisplayNumber(int num) {
	wchar_t buffer[256];
	wsprintfW(buffer, L"%d", num);
	MessageBoxW(nullptr, buffer, L"Information", MB_OK);

	return 0;
}