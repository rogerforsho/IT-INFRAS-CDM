using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace CDM.InventorySystem.Utilities
{
    public static class BarcodeGenerator
    {
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
                bitmap.Save(ms, ImageFormat.Png);
                var imageBytes = ms.ToArray();
                return $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}";
            }
            catch (Exception ex)
            {
                // Fallback to text if barcode generation fails
                return "Barcode generation failed: " + content;
            }
        }
    }
}