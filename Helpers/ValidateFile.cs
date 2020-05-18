using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace UploadImageDotNetCore.Helpers
{
  public class FileValidation
  {
    // get file signatures from https://www.filesignatures.net/
    private static readonly Dictionary<string, List<byte[]>> _fileSignature =
      new Dictionary<string, List<byte[]>>
      {
        { "image", new List<byte[]>
          {
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, // jpeg, jpg
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }, // jpg
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 }, // jpeg
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }, // jpeg
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }, // jpg
            new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, // png
            new byte[] { 0x47, 0x49, 0x46, 0x38 }, // gif
            new byte[] { 0x42, 0x4D }, // bmp
          }
        },
      };
    private static bool BaseValidation(IFormFile file)
    {
      bool minimumFileSizeValidation = file.Length > 0;
      bool maximumFileSizeValidation = file.Length < 1 * 1024 * 1024 * 1024; // 1 GB
      return (
        minimumFileSizeValidation &&
        maximumFileSizeValidation
      );
    }
    public static bool ValidateImageFile(IFormFile file)
    {
      if (BaseValidation(file)) {
        using (var reader = new BinaryReader(file.OpenReadStream()))
        {
          var signatures = _fileSignature["image"];
          var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));
          return signatures.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature));
        }
      }
      return false;
    }
  }
}