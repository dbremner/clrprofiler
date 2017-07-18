namespace CLRProfiler
{
    internal partial class FunctionList
    {
        internal class FunctionDescriptor
        {
            internal FunctionDescriptor(int functionId, int funcCallStack, uint funcSize, int funcModule)
            {
                this.functionId = functionId;
                this.funcCallStack = funcCallStack;
                this.funcSize = funcSize;
                this.funcModule = funcModule;
            }

            internal readonly int functionId;
            internal readonly int funcCallStack;
            internal readonly uint funcSize;
            internal readonly int funcModule;
        }
    }
}