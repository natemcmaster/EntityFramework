// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FileSystemFileService : IFileService
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool DirectoryExists(string directoryName)
            => Directory.Exists(directoryName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool FileExists(string directoryName, string fileName)
            => File.Exists(Path.Combine(directoryName, fileName));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsFileReadOnly(string directoryName, string fileName)
        {
            var fullFileName = Path.Combine(directoryName, fileName);
            return File.Exists(fullFileName)
                   && File.GetAttributes(fullFileName).HasFlag(FileAttributes.ReadOnly);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string RetrieveFileContents(string directoryName, string fileName)
            => File.ReadAllText(Path.Combine(directoryName, fileName));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void WriteFile(string path, string contents)
            => File.WriteAllText(path, contents, Encoding.UTF8);

        /// <inheritdoc />
        public void CreateDirectory(string path)
            => Directory.CreateDirectory(path);

        /// <inheritdoc />
        public void Delete(string path)
            => File.Delete(path);

        /// <inheritdoc />
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
            => Directory.EnumerateFiles(path, searchPattern, searchOption);
    }
}
