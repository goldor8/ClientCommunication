using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Packet : IDisposable
{
    public enum ClientPacketIDReference
    {
        ResendMessage = 0,
        UpdatePos = 1,
        IsConnected = 2,
        Disconnect = 3
    }
        
    public enum ServerPacketIDReference
    {
        Connected = 0,
        DebugMessage = 1,
        wtf = 2,
        RemovePlayer = 3,
        NewPlayer = 4,
        UpdatePosOfAPlayer = 5,
        
    }
    
    private int _readPos;
    private List<Byte> _editBuffer;
    private Byte[] _readBuffer;

    public Packet()
    {
        _readPos = 0;
        _editBuffer = new List<byte>();
    }
    
    public Packet(int id)
    {
        _readPos = 0;
        _editBuffer = new List<byte>();
        Write(id);
    }
    
    public Packet(Byte[] bytesData)
    {
        _readPos = 0;
        _editBuffer = new List<byte>();
        SetData(bytesData);
    }


    public void SetData(Byte[] bytesData)
    {
        _readBuffer = bytesData.ToArray();
        _editBuffer.Clear(); // make sure the edit buffer is empty
        _editBuffer.AddRange(bytesData);
    }

    public void InsertInt(int intData)
    {
        _editBuffer.InsertRange(0, BitConverter.GetBytes(intData));
    }
    
    public void Write(Byte[] bytesData)
    {
        _editBuffer.AddRange(bytesData);
        ActualiseReadBuffer();
    }
    
    public void Write(Byte byteData)
    {
        _editBuffer.Add(byteData);
        ActualiseReadBuffer();
    }
    
    public void Write(int intData)
    {
        _editBuffer.AddRange(BitConverter.GetBytes(intData));
        ActualiseReadBuffer();
    }
    
    public void Write(bool boolData)
    {
        _editBuffer.AddRange(BitConverter.GetBytes(boolData));
        ActualiseReadBuffer();
    }
    
    public void Write(float floatData)
    {
        _editBuffer.AddRange(BitConverter.GetBytes(floatData));
        ActualiseReadBuffer();
    }

    public void Write(String stringData)
    {
        Write(stringData.Length);
        _editBuffer.AddRange(Encoding.ASCII.GetBytes(stringData));
        ActualiseReadBuffer();
    }

    private void ActualiseReadBuffer()
    {
        _readBuffer = _editBuffer.ToArray();
    }

    public Byte[] ReadAllBytes()
    {
        ActualiseReadBuffer();
        
        if (_readBuffer.Length > 0)
        {
            return _readBuffer;
        }
        else
        {
            throw new Exception("No bytes to read");
        }
    }

    public Byte[] ReadBytes(int offSet,int lenght,bool moveReadPos)
    {
        if (lenght <= _readBuffer.Length - offSet)
        {
            Byte[] returnValue = _editBuffer.GetRange(offSet,lenght).ToArray();
            
            if (moveReadPos)
            {
                _readPos += lenght;
            }

            return returnValue;
        }
        else
        {
            throw new Exception("No more bytes to read type 'bytes'");
        }
    }
    
    public Byte[] ReadBytes(int lenght,bool moveReadPos)
    {
        if (lenght <= _readBuffer.Length - _readPos)
        {
            Byte[] returnValue = _editBuffer.GetRange(_readPos,lenght).ToArray();
            
            if (moveReadPos)
            {
                _readPos += lenght;
            }

            return returnValue;
        }
        else
        {
            throw new Exception("No more bytes to read type 'bytes'");
        }
    }
    
    public Byte[] ReadBytes(bool moveReadPos)
    {
        int lenght = ReadInt(true);
        if (lenght <= _readBuffer.Length - _readPos)
        {
            Byte[] returnValue = _editBuffer.GetRange(_readPos,lenght).ToArray();
            
            if (moveReadPos)
            {
                _readPos += lenght;
            }

            return returnValue;
        }
        else
        {
            throw new Exception("No more bytes to read type 'bytes'");
        }
    }

    public Byte ReadByte(bool moveReadPos)
    {
        if (1 <= _readBuffer.Length - _readPos)
        {
            Byte returnValue = _readBuffer[_readPos];

            if (moveReadPos)
            {
                _readPos += 1;
            }


            return returnValue;
        }
        else
        {
            throw new Exception("No more bytes to read type 'byte'");
        }
    }
    
    public int ReadInt(bool moveReadPos)
    {
        if (4 <= _readBuffer.Length - _readPos)
        {
            int returnValue = BitConverter.ToInt32(_readBuffer,_readPos);

            if (moveReadPos)
            {
                _readPos += 4;
            }

            
            return returnValue;
        }
        else
        {
            throw new Exception("No more bytes to read type 'int'");
        }
    }
    
    public bool ReadBool(bool moveReadPos)
    {
        if (1 <= _readBuffer.Length - _readPos)
        {
            bool returnValue = BitConverter.ToBoolean(_readBuffer,_readPos);

            if (moveReadPos)
            {
                _readPos += 1;
            }


            return returnValue;
        }
        else
        {
            throw new Exception("No more bytes to read type 'boolean'");
        }
    }
    
    public float ReadFloat(bool moveReadPos)
    {
        if (4 <= _readBuffer.Length - _readPos)
        {
            float returnValue = BitConverter.ToSingle(_readBuffer,_readPos);

            if (moveReadPos)
            {
                _readPos += 4;
            }


            return returnValue;
        }
        else
        {
            throw new Exception("No more bytes to read type 'float'");
        }
    }
    
    public String ReadString(bool moveReadPos)
    {
        int lenght = ReadInt(true);
        if (lenght <= _readBuffer.Length - _readPos)
        {
            String returnValue = Encoding.ASCII.GetString(_readBuffer,_readPos,lenght);
            
            if (moveReadPos)
            {
                _readPos += lenght;
            }

            return returnValue;
        }
        else
        {
            throw new Exception("No more bytes to read type 'string'");
        }
    }


    public int GetLenght()
    {
        return _readBuffer.Length;
    }

    public int GetUnreadLenght()
    {
        return _readBuffer.Length - _readPos;
    }
    
    
    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _editBuffer = null;
                _readBuffer = null;
                _readPos = 0;
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}