using Microsoft.AspNetCore.Mvc;
using Alquileres.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Alquileres.Context;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

public class EmpresaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public EmpresaController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var empresa = await _context.Empresas.FirstOrDefaultAsync();
        if (empresa == null)
        {
            empresa = new Empresa();
        }
        return View(empresa);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarDatosEmpresa(
        [FromForm] Empresa empresaData,
        [FromForm] IFormFile? logoFile,
        [FromForm] IFormFile? fondoFile,
        [FromForm] IFormFile? qrRedesFile,
        [FromForm] IFormFile? qrWebFile)
    {
        try
        {
            // 1. Validación de archivos (solo si se proporcionan)
            var fileErrors = new Dictionary<string, string>();
            const int maxFileSize = 2 * 1024 * 1024; // 2MB

            ValidateFile(logoFile, "logoFile", maxFileSize, fileErrors);
            ValidateFile(fondoFile, "fondoFile", maxFileSize, fileErrors);
            ValidateFile(qrRedesFile, "qrRedesFile", maxFileSize, fileErrors);
            ValidateFile(qrWebFile, "qrWebFile", maxFileSize, fileErrors);

            if (fileErrors.Any())
            {
                return Json(new { success = false, errors = fileErrors });
            }

            // 2. Validación de campos básicos
            if (string.IsNullOrWhiteSpace(empresaData.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre de la empresa es requerido");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                );
                return Json(new { success = false, errors = errors });
            }

            // 3. Buscar empresa existente o crear nueva
            Empresa empresa = await _context.Empresas.FirstOrDefaultAsync();

            if (empresa == null) // No existe ninguna empresa, crear nueva
            {
                empresa = new Empresa();
                _context.Empresas.Add(empresa);
            }
            // Si existe, se actualizarán sus datos

            // 4. Actualizar datos básicos
            UpdateBasicInfo(empresa, empresaData);

            // 5. Procesar imágenes (solo actualiza si se proporciona un archivo)
            await ProcessImageFile(logoFile, bytes => empresa.Logo = bytes);
            await ProcessImageFile(fondoFile, bytes => empresa.Fondo = bytes);
            await ProcessImageFile(qrRedesFile, bytes => empresa.CodigoQrRedes = bytes);
            await ProcessImageFile(qrWebFile, bytes => empresa.CodigoQrWeb = bytes);

            // 6. Guardar cambios
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Los datos de la empresa se han guardado correctamente.";
            return Json(new { success = true, redirectUrl = Url.Action("Index") });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al guardar empresa: {ex.Message}");
            return Json(new
            {
                success = false,
                message = "Error interno al guardar los datos",
                errorDetails = ex.Message
            });
        }
    }

    // Métodos auxiliares
    private void ValidateFile(IFormFile? file, string fieldName, int maxSize, Dictionary<string, string> errors)
    {
        if (file != null && file.Length > maxSize)
        {
            errors.Add(fieldName, $"El archivo no puede superar los {maxSize / (1024 * 1024)}MB");
        }
    }

    private void UpdateBasicInfo(Empresa empresa, Empresa newData)
    {
        empresa.Rnc = newData.Rnc ?? empresa.Rnc;
        empresa.Nombre = newData.Nombre ?? empresa.Nombre;
        empresa.Direccion = newData.Direccion ?? empresa.Direccion;
        empresa.Telefonos = newData.Telefonos ?? empresa.Telefonos;
        empresa.Slogan = newData.Slogan ?? empresa.Slogan;
        empresa.Logo = newData.Logo ?? empresa.Logo;
        empresa.Fondo = newData.Fondo ?? empresa.Fondo;
        empresa.CodigoQrWeb = newData.CodigoQrWeb ?? empresa.CodigoQrWeb;
        empresa.CodigoQrRedes = newData.CodigoQrRedes ?? empresa.CodigoQrRedes;

    }

    private async Task ProcessImageFile(IFormFile? file, Action<byte[]> setImageAction)
    {
        if (file != null && file.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            setImageAction(memoryStream.ToArray());
        }
    }

    // Métodos para mostrar imágenes
    [HttpGet("Empresa/Logo")]
    public async Task<IActionResult> GetLogo()
    {
        var empresa = await _context.Empresas.FirstOrDefaultAsync();
        if (empresa?.Logo == null || empresa.Logo.Length == 0)
        {
            var defaultImagePath = Path.Combine(_env.WebRootPath, "images", "logo-default.png");
            if (System.IO.File.Exists(defaultImagePath))
            {
                return PhysicalFile(defaultImagePath, "image/png");
            }
            return NotFound();
        }
        return File(empresa.Logo, "image/png");
    }

    [HttpGet("Empresa/Fondo")]
    public async Task<IActionResult> GetFondo()
    {
        var empresa = await _context.Empresas.FirstOrDefaultAsync();
        if (empresa?.Fondo == null || empresa.Fondo.Length == 0)
        {
            var defaultImagePath = Path.Combine(_env.WebRootPath, "images", "bg-images-default.png");
            if (System.IO.File.Exists(defaultImagePath))
            {
                return PhysicalFile(defaultImagePath, "image/png");
            }
            return NotFound();
        }
        return File(empresa.Fondo, "image/png");
    }

    [HttpGet("Empresa/QrRedes")]
    public async Task<IActionResult> GetQrRedes()
    {
        var empresa = await _context.Empresas.FirstOrDefaultAsync();
        if (empresa?.CodigoQrRedes == null || empresa.CodigoQrRedes.Length == 0)
            return NotFound();

        return File(empresa.CodigoQrRedes, "image/png");
    }

    [HttpGet("Empresa/QrWeb")]
    public async Task<IActionResult> GetQrWeb()
    {
        var empresa = await _context.Empresas.FirstOrDefaultAsync();
        if (empresa?.CodigoQrWeb == null || empresa.CodigoQrWeb.Length == 0)
            return NotFound();

        return File(empresa.CodigoQrWeb, "image/png");
    }

    [HttpGet("Empresa/HasQrWeb")]
    public async Task<IActionResult> HasQrWeb()
    {
        var empresa = await _context.Empresas.FirstOrDefaultAsync();
        bool hasQr = empresa?.CodigoQrWeb != null && empresa.CodigoQrWeb.Length > 0;
        return Ok(hasQr);
    }

    [HttpGet("Empresa/GetEmpresaInfo")]
    public async Task<IActionResult> GetEmpresaInfo()
    {
        var empresa = await _context.Empresas.FirstOrDefaultAsync();

        if (empresa == null)
        {
            return Json(new
            {
                nombre = "Nombre de Empresa",
                rnc = "RNC no configurado",
                dir = "Direccion de la empresa",
                tel = "Teléfonos no configurados",
                tieneLogo = false
            });
        }

        return Json(new
        {
            nombre = empresa.Nombre ?? "Nombre de Empresa",
            rnc = empresa.Rnc ?? "RNC no configurado",
            dir = empresa.Direccion ?? "Dirección no configurada",
            tel = empresa.Telefonos ?? "Teléfonos no configurados",
            tieneLogo = empresa.Logo != null && empresa.Logo.Length > 0
        });
    }
}