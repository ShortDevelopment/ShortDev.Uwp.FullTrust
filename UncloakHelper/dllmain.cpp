// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <servprov.h>

#include "Helper.h"

#include "Interfaces.h"
#include "ComServerFactory.h"

HMODULE currentModule;

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	currentModule = hModule;

	return TRUE;
}

DWORD WINAPI StartServerImpl(LPVOID lpParam) {
	// Init COM.
	// CoInitialize(NULL);

	// Create the Class Factory
	ComServerFactory factory;

	// Register the Class Factory.
	DWORD regID = 0;
	HRESULT hr = CoRegisterClassObject(CLSID_ComServer, (IUnknown*)(IClassFactory*)&factory, CLSCTX_LOCAL_SERVER, REGCLS_MULTIPLEUSE, &regID);
	if (FAILED(hr))
	{
		CoUninitialize();
		exit(1);
	}
	else {
		// CoRegisterPSClsid(IID_ICommunication, )
	}

	while (true) {}

	// Unregister Class Factory
	CoRevokeClassObject(regID);

	// Terminate COM.
	CoUninitialize();
}

#define PINVOKE extern "C" __declspec(dllexport)

PINVOKE HRESULT StartServer(int _) {
	DWORD threadId;
	CreateThread(NULL, 0, StartServerImpl, 0, 0, &threadId);
	return 0;
}