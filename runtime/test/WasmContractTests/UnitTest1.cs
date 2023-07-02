using Solang;

namespace WasmContractTests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var compiler = new Compiler();
        var output = compiler.BuildWasm(
            @"contract simple {
                        function foo() public pure returns (uint32) {
                            return 2;
                        }
                    }");
        Console.WriteLine("done");
    }
}