using System;
using System.IO;

namespace Pulumi.Azure.Extensions.Utils
{
    public sealed class TempFolder : IDisposable
    {
        private readonly bool _isTemp;

        public string Path { get; }

        public TempFolder(string path, bool isTemp)
        {
            _isTemp = isTemp;

            if (isTemp)
            {
                Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString()); ;
                Clear(true);
                Create(true);
            }
            else
            {
                Path = path;
            }
        }

        private void Create(bool isTemp)
        {
            if (!isTemp)
            {
                return;
            }

            try
            {
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
            }
            catch (IOException)
            {
            }
        }

        private void Clear(bool isTemp)
        {
            if (!isTemp)
            {
                return;
            }

            try
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, true);
                }
            }
            catch (IOException)
            {
            }
        }

        /// <summary>
        /// An indicator whether this object is being actively disposed or not.
        /// </summary>
        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases managed resources upon dispose.
        /// </summary>
        /// <remarks>
        /// All managed resources must be released in this
        /// method, so after disposing this object no other
        /// object is being referenced by it anymore.
        /// </remarks>
        private void ReleaseManagedResources()
        {
            Clear(_isTemp);
        }

        /// <summary>
        /// Releases unmanaged resources upon dispose.
        /// </summary>
        /// <remarks>
        /// All unmanaged resources must be released in this
        /// method, so after disposing this object no other
        /// object is beeing referenced by it anymore.
        /// </remarks>
        private void ReleaseUnmanagedResources()
        {
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                /* Release unmanaged resources */
                ReleaseUnmanagedResources();

                if (disposing)
                {
                    /* Release managed resources */
                    ReleaseManagedResources();
                }

                /* Set indicator that this object is disposed */
                _disposed = true;
            }
        }
    }
}