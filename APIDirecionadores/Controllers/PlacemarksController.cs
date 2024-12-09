using APIDirecionadores.Interface;
using APIDirecionadores.Models;
using Microsoft.AspNetCore.Mvc;

namespace APIDirecionadores.Controllers
{
    [ApiController]
    [Route("api/placemarks")]
    public class KmlController : ControllerBase
    {
        private readonly IKmlFileService _kmlFileService;
        private readonly ILogger<KmlController> _logger;

        public KmlController(IKmlFileService kmlFileService, ILogger<KmlController> logger)
        {
            _logger = logger;
            _kmlFileService = kmlFileService;
        }

        [HttpPost]
        public IActionResult OpenKmlFile(IFormFile kmlFile)
        {
            try
            {
                var document = _kmlFileService.GetFile(kmlFile);
                _logger.LogInformation("Arquivo KML processado com sucesso.");

                return Ok(document);
            }          
       
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar o arquivo KML.");
                return StatusCode(500, $"Erro ao processar o arquivo KML: {ex.Message}");
            }
        }

        [HttpPost("export/save")]
        public IActionResult ExportAndSaveKml([FromQuery] PlacemarkFilter filter)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "exported-data.kml");
                var message = _kmlFileService.ExportKmlFile(filter, filePath);

                return Ok(new { Message = message, FilePath = filePath });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }


        [HttpGet("api/placemarks")]
        public IActionResult GetDataJson([FromQuery] PlacemarkFilter filter)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var Jsonlister = _kmlFileService.FilterPlacemarks(filter);

                return Ok(Jsonlister);

            }       
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }


        [HttpGet("filters")]
        public IActionResult FilterPlacemarks([FromQuery] string cliente = null, [FromQuery] string situacao = null, [FromQuery] string bairro = null)
        {
            try
            {
                var filterOptions = _kmlFileService.GetFilterOptions();
                var validClientes = ((List<string>)filterOptions.GetType().GetProperty("Clientes").GetValue(filterOptions)) ?? new List<string>();
                var validSituacoes = ((List<string>)filterOptions.GetType().GetProperty("Situacoes").GetValue(filterOptions)) ?? new List<string>();
                var validBairros = ((List<string>)filterOptions.GetType().GetProperty("Bairros").GetValue(filterOptions)) ?? new List<string>();

                if (!string.IsNullOrEmpty(cliente) && !validClientes.Contains(cliente, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { Error = "Cliente fornecido não está na lista de opções disponíveis." });
                }

                if (!string.IsNullOrEmpty(situacao) && !validSituacoes.Contains(situacao, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { Error = "Situação fornecida não está na lista de opções disponíveis." });
                }

                if (!string.IsNullOrEmpty(bairro) && !validBairros.Contains(bairro, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { Error = "Bairro fornecido não está na lista de opções disponíveis." });
                }

                var document = _kmlFileService.GetCachedDocument();
                var placemarkData = _kmlFileService.ExtractPlacemarkData(document);

                var filteredData = placemarkData.Where(p =>
                    (string.IsNullOrEmpty(cliente) || p.ExtendedData.Any(d => d.Key.Equals("Cliente", StringComparison.OrdinalIgnoreCase) && d.Value.Equals(cliente, StringComparison.OrdinalIgnoreCase))) &&
                    (string.IsNullOrEmpty(situacao) || p.ExtendedData.Any(d => d.Key.Equals("Situação", StringComparison.OrdinalIgnoreCase) && d.Value.Equals(situacao, StringComparison.OrdinalIgnoreCase))) &&
                    (string.IsNullOrEmpty(bairro) || p.ExtendedData.Any(d => d.Key.Equals("Bairro", StringComparison.OrdinalIgnoreCase) && d.Value.Equals(bairro, StringComparison.OrdinalIgnoreCase)))
                ).ToList();

                return Ok(filteredData);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
