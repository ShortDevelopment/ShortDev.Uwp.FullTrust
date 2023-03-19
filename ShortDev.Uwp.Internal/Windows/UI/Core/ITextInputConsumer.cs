using System;
using System.Runtime.InteropServices;

namespace Windows.UI.Core;

[Guid("a9d00ab3-2fef-41a0-b0ad-4b2129ea2663"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
public interface ITextInputConsumer
{
    ITextInputProducer TextInputProducer { get; set; }
    void InvokeAcceleratorKeyEventHandlers();
    void InvokeKeyDownEventHandlers();
    void InvokeKeyUpEventHandlers();
    void InvokeCharacterReceivedEventHandlers();
    void InvokeSystemKeyDownEventHandlers();
    void InvokeSystemKeyUpEventHandlers();
    void InvokeNavigationFocusEventHandlers();
    void OnTextInputProducerFocusChanged();
    void OnEnableNonCUIDepartFocus();
}
