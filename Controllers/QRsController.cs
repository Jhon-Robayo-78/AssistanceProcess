using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace AsistenciaProcess.Controllers
{
    [Authorize]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    [Route("api/[controller]")]
    [ApiController]
    public class QRsController : ControllerBase
    {
        [Authorize(Roles = "admin, profesor")]
        [HttpPost]
        public async Task<IActionResult> GenerateQrCode([FromBody] string link)
        {
            try
            {
                if (string.IsNullOrEmpty(link))
                {
                    return BadRequest("El enlace no puede estar vacío");
                }

                // Creamos un generador de código QR
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

                // Convertimos el código QR en un arreglo de bytes
                byte[] qrCodeBytes = qrCode.GetGraphic(20);

                // Cargamos la imagen con ImageSharp usando el arreglo de bytes
                using Image image = Image.Load(qrCodeBytes);
                using MemoryStream ms = new MemoryStream();
                await Task.Run(() => image.Save(ms, new PngEncoder()));
                byte[] imageBytes = ms.ToArray();

                // Devolvemos la imagen como un archivo
                return File(imageBytes, "image/png");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}


