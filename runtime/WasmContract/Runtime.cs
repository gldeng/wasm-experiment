using Wasmtime;

namespace WasmContract;

public class Runtime : IDisposable
{
    private readonly Store _store;
    private readonly Engine _engine;
    private readonly Linker _linker;
    private readonly Memory _memory;
    private readonly Module _module;
    public byte[] ReturnBuffer = Array.Empty<byte>();
    public byte[] Input { get; set; } = { };

    public Runtime(string name, byte[] code)
    {
        _engine = new Engine();
        _store = new Store(_engine);
        _linker = new Linker(_engine);
        _memory = new Memory(_store, 16, 16);
        _module = Module.FromBytes(_engine, "contract", code);
        DefineImportFunctions();
    }

    public Instance Instantiate()
    {
        return _linker.Instantiate(_store, _module);
    }

    private void InputFunc(int dataPtr, int dataLenPtr)
    {
        WriteBytes(dataPtr, Input);
        WriteUInt32(dataLenPtr, Convert.ToUInt32(Input.Length));
    }

    private void ReturnFunc(int flags, int dataPtr, int dataLen)
    {
        Console.WriteLine("return called");
        ReturnBuffer = new byte[dataLen];
        for (var offset = dataLen - 1; offset >= 0; offset--)
        {
            ReturnBuffer[offset] = _memory.ReadByte(dataPtr + offset);
        }

        Console.WriteLine("return done");
    }

    private int DebugMessage(int stringPtr, int stringLength)
    {
        Console.WriteLine("debug called");
        return 0;
    }

    private void ValueTransferred(int valuePtr, int valueLengthPtr)
    {
        Console.WriteLine("value_trasnferred called");
        WriteUInt32(valueLengthPtr, 0);

        Console.WriteLine("value_trasnferred done");
    }

    private void DefineImportFunctions()
    {
        _linker.Define("env", "memory", _memory);
        _linker.DefineFunction("seal0", "input", (Action<int, int>)InputFunc);
        _linker.DefineFunction("seal0", "seal_return", (Action<int, int, int>)ReturnFunc);
        _linker.DefineFunction("seal0", "debug_message", (Func<int, int, int>)DebugMessage);
        _linker.DefineFunction("seal0", "value_transferred", (Action<int, int>)ValueTransferred);
    }

    private void WriteBytes(int address, byte[] data)
    {
        foreach (var (offset, byt) in data.Select((b, i) => (i, b)))
        {
            _memory.Write(address + offset, byt);
        }
    }

    private void WriteUInt32(int address, uint value)
    {
        var numberInBytes = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(numberInBytes);
        }

        WriteBytes(address, numberInBytes);
    }

    void IDisposable.Dispose()
    {
        _module?.Dispose();
        _linker?.Dispose();
        _store?.Dispose();
        _engine?.Dispose();
    }
}