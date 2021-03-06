﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using falcon.Falcon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace evaporate.Controllers
{
    [ApiController]
    [Route("File")]
    public class FileController : ControllerBase
    {
        ILogger<FileController> _logger;
        public FileController(ILogger<FileController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("Upload")]
        public IActionResult Upload()
        {
            try
            {
                var files = Request.Form.Files;
                if (files == null || !files.Any())
                    return NoContent();

                if (files.Count() > 1)
                    return BadRequest("Cannot accept more than one file");

                var file = files.FirstOrDefault();

                if (file == null)
                    return BadRequest("Invalid file");

                if (Path.GetExtension(file.FileName) != ".zip")
                    return BadRequest("Only zip files are accepted");

                var context = CreateDeployContext(file);

                if (Falcon.Instance.Deploy(context))
                    return Content(context.DestPath);
                else
                    throw context.Exception;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        private Context CreateDeployContext(IFormFile file)
            =>
            new Context(
                file.OpenReadStream(),
                contentPath,
                file.FileName
                );


        static readonly string
            contentPath = Path.Combine(Program.ContentRoot(), "Contents");
        static FileController()
        {
            if (!Directory.Exists(contentPath))
                Directory.CreateDirectory(contentPath);
        }
    }
}
