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
        private readonly IPlacermarkFilterService _placermarkFilterService;
        private readonly ILogger<KmlController> _logger;

        public KmlController(IKmlFileService kmlFileService, IPlacermarkFilterService placermarkFilterService, ILogger<KmlController> logger)
        {
            _logger = logger;
            _kmlFileService = kmlFileService;
            _placermarkFilterService = placermarkFilterService;
        }

        [HttpPost("import")]
        public IActionResult OpenKmlFile(IFormFile kmlFile)
        {
            try
            {
                var document = _kmlFileService.OpenKmlFile(kmlFile);
                _logger.LogInformation("Arquivo KML processado com sucesso.");

                return Ok(document);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar o arquivo KML.");
                return StatusCode(500, $"Erro ao processar o arquivo KML: {ex.Message}");
            }
        }

        [HttpPost("export")]
        public IActionResult ExportFilteredKmlFile([FromQuery] PlacemarkModel filter)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string filePath = Path.Combine("Exports", $"Filtered_{DateTime.Now:yyyyMMddHHmmss}.kml");
                var filteredDocument = _placermarkFilterService.CreateFilteredKmlDocument(filter);

                var resultMessage = _kmlFileService.SaveKmlFile(filteredDocument, filePath);

                return Ok(new { Message = resultMessage, FilePath = filePath });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("api/placemarks")]
        public IActionResult GetDataJson([FromQuery] PlacemarkModel filter)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var Jsonlister = _placermarkFilterService.FilterPlacemarks(filter);

                return Ok(Jsonlister);

            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("filters")]
        public IActionResult GetListFilter(PlacemarkDTO placemarkDTO)
        {
            try
            {
                var filterOptions = _placermarkFilterService.FilterPlacemarksCSB(placemarkDTO);
                return Ok(filterOptions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
