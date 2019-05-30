using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class SlypeMemory
{
    // Memory Address Functions
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

    // Desired Process Access Levels
    Dictionary<string, int> processAccess = new Dictionary<string, int>
    {
        { "readonly",  0x0010 }, // PROCESS_VM_READ
        { "writeonly",  0x0020 }, // PROCESS_VM_WRITE
        { "full", 0x1F0FFF } // PROCESS_ALL_ACCESS
    };

    // Declaration of Variables
    public string processName;
    public Process process;
    public int processHandle;
    public Dictionary<string, int> modules;
    public string accessLevel;
    

    // Constructor
    public SlypeMemory(string _processName, string _accessLevel, int numOfRetries = 10, int timeout = 100)
    {
        processName = _processName;
        process = resolveProcess(_processName, numOfRetries, timeout); // Check this after init, may be null
        accessLevel = _accessLevel;  
        modules = new Dictionary<string, int>();
    }

    // Attempts to resolve process, numOfRetries & timeout between tries are optional
    public static Process resolveProcess(string processName, int numOfRetries = 10, int timeout = 100)
    {
        Process process = null;
        Process[] processes;
        for (int i = 0; i < numOfRetries; i++)
        {
            processes = Process.GetProcessesByName(processName);
            if (processes.Length == 1) // Process found
            {
                process = processes[0];
                return process;
            }
            else
                Thread.Sleep(timeout);
        }
        return null;
    }

    // Attempts to open a handle to a process with default accessLevel
    public bool openHandle()
    {
        if (!processAccess.ContainsKey(accessLevel))
            return false;
        processHandle = (int)OpenProcess(processAccess[accessLevel], false, process.Id);
        if (processHandle != 0)
            return true;
        return false;
    }

    // Attempts to open a handle to a process with a given desiredAccess (readonly, writeonly, full)
    public int openHandle(Process process, string desiredAccess)
    {
        if (!processAccess.ContainsKey(desiredAccess))
            return -1;
        return (int)OpenProcess(processAccess[desiredAccess], false, process.Id);
    }

    // Attempts to resolve address of a DLL/Module
    public int resolveModule(string moduleName, int numOfRetries = 10, int timeout = 100)
    {
        ProcessModuleCollection modules = process.Modules;
        for (int i = 0; i < numOfRetries; i++)
        {
            foreach (ProcessModule mod in modules)
            {
                if (mod.ModuleName == moduleName)
                    return (int)mod.BaseAddress;
            }
            Thread.Sleep(timeout);
        }
        return -1;
    }

    // Attempts to add a module to modules table
    public bool addModule(string name)
    {
        int addr = resolveModule(name, 1, 1);
        if(addr != 0)
        {
            modules.Add(name, addr);
            return true;
        }
        return false;
    }

    // Returns an address for a given module if it exists in the modules table
    public int module(string name)
    {
        if (modules.ContainsKey(name))
            return modules[name];
        return -1;
    }

    // Lists all current modules
    public ProcessModuleCollection allModules()
    {
        return process.Modules;
    }

    // Reads a memory address and returns a byte array
    public byte[] readMemory(int address, int bytesToRead)
    {
        int bytesRead = 0;
        byte[] buffer = new byte[bytesToRead];
        ReadProcessMemory(processHandle, address, buffer, buffer.Length, ref bytesRead);
        return buffer;
    }

    // Reads a byte from memory
    public byte readByte(int address)
    {
        return readMemory(address, 1)[0];
    }

    // Reads a bool from memory
    public bool readBool(int address)
    {
        return BitConverter.ToBoolean(readMemory(address, 1), 0);
    }

    // Reads an int16 from memory
    public Int32 readInt16(int address)
    {
        return BitConverter.ToInt16(readMemory(address, 2), 0);
    }

    // Reads an int32 from memory
    public Int32 readInt32(int address)
    {
        return BitConverter.ToInt32(readMemory(address, 4), 0);
    }

    // Reads an int64 from memory
    public Int64 readInt64(int address)
    {
        return BitConverter.ToInt64(readMemory(address, 8), 0);
    }

    // Reads a float from memory
    public float readFloat(int address)
    {
        return BitConverter.ToSingle(readMemory(address, 4), 0);
    }

    // Reads a double from memory
    public double readDouble(int address)
    {
        return BitConverter.ToDouble(readMemory(address, 8), 0);
    }


    // Writes a byte array to memory
    public void writeMemory(int address, byte[] content)
    {
        int bytesWritten = 0;
        WriteProcessMemory(processHandle, address, content, content.Length, ref bytesWritten);
    }

    // Writes a byte from memory
    public void writeByte(int address, byte content)
    {
        writeMemory(address, new byte[] { content });
    }

    // Writes a bool from memory
    public void writeBool(int address, bool content)
    {
        writeMemory(address, BitConverter.GetBytes(content));
    }

    // Writes an int16 from memory
    public void writeInt16(int address, Int16 content)
    {
        writeMemory(address, BitConverter.GetBytes(content));
    }

    // Writes an int32 from memory
    public void writeInt32(int address, Int32 content)
    {
        writeMemory(address, BitConverter.GetBytes(content));
    }

    // Writes an int64 from memory
    public void writeInt64(int address, Int64 content)
    {
        writeMemory(address, BitConverter.GetBytes(content));
    }

    // Writes a float from memory
    public void writeFloat(int address, float content)
    {
        writeMemory(address, BitConverter.GetBytes(content));
    }

    // Writes a double from memory
    public void writeDouble(int address, double content)
    {
        writeMemory(address, BitConverter.GetBytes(content));
    }
	
	// Calculate sum of offsets
	public int sumOffsets(params int[] addresses)
	{
		int total = 0;
		for(int i = 0;i < addresses.Length;i++)
		{
			total += addresses[i];
		}
		return total;
	}
	
	// Read & jump multiple times [FIX]
	public int jumpMultiple(int baseAddress, params int[] offsets)
	{
		int addr = baseAddress;
		for(int i = 0;i < offsets.Length;i++)
		{
			addr = readInt32(addr);
		}
		return addr;
	}
}
