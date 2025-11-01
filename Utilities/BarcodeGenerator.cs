using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using System.Runtime.Versioning;

namespace CDM.InventorySystem.Utilities
{
    public static class BarcodeGenerator
    {
        [SupportedOSPlatform("windows6.1")]
        public static string GenerateBarcodeImage(string content)
        {
            try
            {
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Width = 300,
                        Height = 100,
                        Margin = 2,
                        PureBarcode = false
                    }
                };

                using var bitmap = writer.Write(content);
                using var ms = new MemoryStream();
                
                // Use a platform-safe approach for saving bitmap
                if (OperatingSystem.IsWindows())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                }
                else
                {
                    // Fallback for non-Windows platforms
                    throw new PlatformNotSupportedException("Barcode generation is only supported on Windows platforms.");
                }
                
                var imageBytes = ms.ToArray();
                return $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}";
            }
            catch (Exception ex)
            {
                // Fallback to text if barcode generation fails
                return "Barcode generation failed: " + content;
            }
        }

        // Alternative method that returns raw bytes for more flexibility
        [SupportedOSPlatform("windows6.1")]
        public static byte[] GenerateBarcodeBytes(string content)
        {
            try
            {
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Width = 300,
                        Height = 100,
                        Margin = 2,
                        PureBarcode = false
                    }
                };

                using var bitmap = writer.Write(content);
                using var ms = new MemoryStream();
                
                if (OperatingSystem.IsWindows())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
                else
                {
                    throw new PlatformNotSupportedException("Barcode generation is only supported on Windows platforms.");
                }
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }
    }
}