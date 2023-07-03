using Solang;
using Wasmtime;

namespace WasmContract;

public static class Simple
{
    public static void Demo()
    {
        var input = new byte[] { 194, 152, 85, 120 }; // 0xc2985578 means calling foo() function
        var output = new Compiler().BuildWasm(
            @"contract simple {
                function foo() public pure returns (uint32) {
                    return 2;
                }
            }");
        using var runtime = new Runtime("contract", output.Contracts[0].WasmCode.ToByteArray());
        runtime.Input = input;

        var instance = runtime.Instantiate();
        var run = instance.GetAction("call");

        try
        {
            run?.Invoke();
        }
        catch (TrapException ex)
        {
            Console.WriteLine("got exception " + ex.Message);
        }

        Console.WriteLine(Convert.ToHexString(runtime.ReturnBuffer));
    }
}