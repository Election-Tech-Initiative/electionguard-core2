namespace ElectionGuard
{
    public static class ExceptionHandler
    {
        public static void GetData(out string function, out string message, out ulong code)
        {
            var status = NativeInterface.ExceptionHandler.GetData(out var functionData, out var messageData, out var codeData);
            function = functionData.PtrToStringUTF8();
            message = messageData.PtrToStringUTF8();
            code = codeData;
            _ = NativeInterface.Memory.FreeIntPtr(functionData);
            _ = NativeInterface.Memory.FreeIntPtr(messageData);
        }

    }
}
