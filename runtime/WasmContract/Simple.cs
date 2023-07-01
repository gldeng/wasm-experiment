using Wasmtime;

namespace WasmContract;

public class Simple
{
    public static void Run()
    {
        // 0xc2985578
        var input = new byte[] { 194, 152, 85, 120 };
        using var runtime = new Runtime("contract",File.ReadAllBytes("/Users/guanglei/repo/gldeng/wasm-experiment/solang/simple.wasm"));
        runtime.Input = input;

        var instance = runtime.Instantiate();
        var run = instance.GetAction("call");
        
        try
        {
            run?.Invoke();
        }
        catch (TrapException ex)
        {
            Console.WriteLine("got exception "+ ex.Message);
        }

        Console.WriteLine(Convert.ToHexString(runtime.ReturnBuffer));

        Console.WriteLine("Done");
    }
}