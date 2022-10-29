using StagWare.FanControl.Plugins;
using StagWare.Hardware.LPC;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System;

namespace StagWare.Plugins
{
    [Export(typeof(IEmbeddedController))]
    [FanControlPluginMetadata(
        "StagWare.Plugins.ECLinux",
        SupportedPlatforms.Unix,
        SupportedCpuArchitectures.x86 | SupportedCpuArchitectures.x64)]
    public class ECLinux : EmbeddedControllerBase, IEmbeddedController
    {
        #region Constants

        const string PortFilePath = "/dev/port";
        
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
                    this.IsInitialized = AcquireLock(500);
                }
                catch
                {
                }

                if (this.IsInitialized)
                {
                    ReleaseLock();
                }
            }
        }

        public bool AcquireLock(int timeout)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ECLinux));
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
                    fd = SysCall.open(PortFilePath, SysCall.OpenFlags.O_RDWR | SysCall.OpenFlags.O_EXCL);

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
                    Monitor.Exit(syncRootLockTaken);
                }
            }

            return success;
        }

        public void ReleaseLock()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ECLinux));
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

        #region EmbeddedControllerBase implementation

        protected override void WritePort(int port, byte value)
        {
            unsafe
            {
                byte[] buffer = new byte[] { value };
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                
                SysCall.pwrite(this.fd, handle.AddrOfPinnedObject(), (ulong) buffer.Length, port);
                
                handle.Free();
            }
        }

        protected override byte ReadPort(int port)
        {
            byte[] buffer = new byte[1];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                
            SysCall.pread(this.fd, handle.AddrOfPinnedObject(), (ulong) buffer.Length, port);
                
            handle.Free();

            return buffer[0];
        }
        

        #endregion
    }
}
