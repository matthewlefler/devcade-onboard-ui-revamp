using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using log4net;

namespace onboard.util; 

public static class Zip {
    private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName);

    public class Entry {
        public string Name { get; set; }
        public string Path { get; init; }
        public byte[] Data { get; init; }
    }
    
    public static Result<IEnumerable<Entry>, Exception> unzip(byte[] zip) {
        using var inputStream = new MemoryStream(zip);
        List<Entry> entries = new();
        logger.Info($"Unzipping buffer ({zip.Length} bytes)");
        try {
            var archive = new ZipArchive(inputStream, ZipArchiveMode.Read);
            foreach(ZipArchiveEntry entry in archive.Entries) {
                using Stream entryStream = entry.Open();
                
                // Read the entry into a byte array
                byte[] buffer = new byte[1024 * 64]; // 64k buffer
                byte[] output = new byte[1024 * 64]; // 64k initial size
                int bytesRead = 0;
                while (entryStream.CanRead) {
                    int read = entryStream.Read(buffer, 0, buffer.Length);
                    if (read == 0) {
                        break;
                    }
                    bytesRead += read;
                    if (bytesRead > output.Length) {
                        Array.Resize(ref output, output.Length * 2);
                    }
                    Array.Copy(buffer, 0, output, bytesRead - read, read);
                }
                
                Array.Resize(ref output, bytesRead); // trim to actual size
                entries.Add(new Entry {
                    Name = entry.Name,
                    Path = entry.FullName,
                    Data = output
                });
            }
        } catch (Exception e) {
            logger.Warn($"Failed to unzip buffer: {e.Message}");
            return Result<IEnumerable<Entry>, Exception>.Err(e);
        }
        
        return Result<IEnumerable<Entry>, Exception>.Ok(entries);
    }
    
    public static Result<byte[], Exception> zip(byte[] data) {
        using var inputStream = new MemoryStream(data);
        using var outputStream = new MemoryStream();
        try {
            using var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true);
            ZipArchiveEntry entry = archive.CreateEntry("data");
            using Stream entryStream = entry.Open();
            inputStream.CopyTo(entryStream);
        } catch (Exception e) {
            return Result<byte[], Exception>.Err(e);
        }
        return Result<byte[], Exception>.Ok(outputStream.ToArray());
    }
}