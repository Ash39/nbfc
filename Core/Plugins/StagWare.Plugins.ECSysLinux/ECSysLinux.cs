using StagWare.FanControl.Plugins;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System;

namespace StagWare.Plugins.ECSysLinux
{
    [Export(typeof(IEmbeddedController))]
    [FanControlPluginMetadata(
        "StagWare.Plugins.ECSysLinux",
        SupportedPlatforms.Unix,
        SupportedCpuArchitectures.x86 | SupportedCpuArchitectures.x64,
        FanControlPluginMetadataAttribute.DefaultPriority + 10)]
    public class ECSysLinux : IEmbeddedController
    {
        #region Constants

        private const string EC0IOPath = "/sys/kernel/debug/ec/ec0/io";

        #endregion

        #region Private Fields

        static readonly object syncRoot = new object();
        bool disposed = false;
        int fd = -1;

        #endregion

        #region IEmbeddedController implementation

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (!this.IsInitialized)
            {
                try
                {
                    Process modprobe = new Process();
                    modprobe.StartInfo.FileName = "modprobe";
                    modprobe.StartInfo.Arguments = "ec_sys write_support=1";
                    modprobe.Start();
                    modprobe.WaitForExit();

                    IsInitialized = modprobe.ExitCode == 0 && File.Exists(EC0IOPath);
                }
                catch
                {
                }
            }
        }

        public void WriteByte(byte register, byte value)
        {
            byte[] buffer = new byte[] { value };
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                
            SysCall.pwrite(this.fd, handle.AddrOfPinnedObject(), (ulong) buffer.Length, register);
                
            handle.Free();
        }

        public void WriteWord(byte register, ushort value)
        {
            // little endian
            byte msb = (byte)(value >> 8);
            byte lsb = (byte)value;

            byte[] buffer = new byte[] { lsb, msb };
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                
            SysCall.pwrite(this.fd, handle.AddrOfPinnedObject(), (ulong) buffer.Length, register);
                
            handle.Free();
        }

        public byte ReadByte(byte register)
        {
            byte[] buffer = new byte[1];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                
            SysCall.pread(this.fd, handle.AddrOfPinnedObject(), (ulong) buffer.Length, register);
                
            handle.Free();

            return buffer[0];
        }

        public ushort ReadWord(byte register)
        {
            // little endian
            byte[] buffer = new byte[2];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                
            SysCall.pread(this.fd, handle.AddrOfPinnedObject(), (ulong) buffer.Length, register);
                
            handle.Free();

            return (ushort)((buffer[1] << 8) | buffer[0]);
        }

        public bool AcquireLock(int timeout)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ECSysLinux));
            }

            bool success = false;
            bool syncRootLockTaken = false;

            try
            {
                Monitor.TryEnter(syncRoot, timeout, ref syncRootLockTaken);

                if (!syncRootLockTaken)
                {
                    return false;
                }

                if(this.fd == -1)
                {
                    fd = SysCall.open(EC0IOPath, SysCall.OpenFlags.O_RDWR | SysCall.OpenFlags.O_EXCL);

                    if (fd == -1)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    
                }
                success = this.fd != -1;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                if(syncRootLockTaken && !success)
                {
                    Monitor.Exit(syncRoot);
                }
            }

            return success;
        }

        public void ReleaseLock()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ECSysLinux));
            }

            try
            {
                if (this.fd != -1)
                {
                    SysCall.close(fd);
                    fd = -1;
                }
            }
            finally
            {
                Monitor.Exit(syncRoot);
            }
        }

        public void Dispose()
        {
            lock (syncRoot)
            {
                if (this.fd != -1)
                {
                    SysCall.close(fd);
                    fd = -1;
                }

                disposed = true;
            }
        }

        #endregion
    }
}
