using APIDirecionadores.Interface;
using APIDirecionadores.Models;
using SharpKml.Dom;

namespace APIDirecionadores.Services
{
    public class PlacermarkFilterService : IPlacermarkFilterService
    {
        private readonly IKmlFileService _kmlFileService;

        public PlacermarkFilterService(IKmlFileService kmlFileService)
        {
            _kmlFileService = kmlFileService;
        }

        public List<ExtendedDataModel> FilterPlacemarks(PlacemarkModel filter)
        {
            var document = _kmlFileService.GetCachedDocument();

            var placemarks = _kmlFileService.ExtractPlacemarkData(document);

            var filteredPlacemarks = placemarks.Where(p =>
                (string.IsNullOrEmpty(filter.Cliente) || p.ExtendedData.Any(e => e.Key == "CLIENTE" && e.Value.Contains(filter.Cliente, StringComparison.OrdinalIgnoreCase))) &&
                (string.IsNullOrEmpty(filter.Situacao) || p.ExtendedData.Any(e => e.Key == "SITUAÇÃO" && e.Value.Contains(filter.Situacao, StringComparison.OrdinalIgnoreCase))) &&
                (string.IsNullOrEmpty(filter.Bairro) || p.ExtendedData.Any(e => e.Key == "BAIRRO" && e.Value.Contains(filter.Bairro, StringComparison.OrdinalIgnoreCase))) &&
                (string.IsNullOrEmpty(filter.Referencia) || p.ExtendedData.Any(e => e.Key == "REFERENCIA" && e.Value.Contains(filter.Referencia, StringComparison.OrdinalIgnoreCase))) &&
                (string.IsNullOrEmpty(filter.RuaOuCruzamento) || p.ExtendedData.Any(e => e.Key == "RUA" && e.Value.Contains(filter.RuaOuCruzamento, StringComparison.OrdinalIgnoreCase)) ||
                 p.ExtendedData.Any(e => e.Key == "CRUZAMENTO" && e.Value.Contains(filter.RuaOuCruzamento, StringComparison.OrdinalIgnoreCase)))
            ).ToList();

            return filteredPlacemarks;
        }
        public Document CreateFilteredKmlDocument(PlacemarkModel filter)
        {
            // Aplica o filtro nos placemarks
            var filteredPlacemarks = FilterPlacemarks(filter);

            // Cria um novo documento com os placemarks filtrados
            var newDocumentFiltered = new Document();

            foreach (var placemark in filteredPlacemarks)
            {
                var newPlacemark = new Placemark
                {
                    Name = placemark.Name,
                    ExtendedData = _kmlFileService.CreateExtendedData(placemark.ExtendedData)
                };

                newDocumentFiltered.AddFeature(newPlacemark);
            }
            return newDocumentFiltered;
        }

        public Dictionary<string, List<string>> GetListFilter()
        {
            var document = _kmlFileService.GetCachedDocument();
            var placemarkData = _kmlFileService.ExtractPlacemarkData(document);

            var clientes = placemarkData
                .SelectMany(p => p.ExtendedData.Where(d => d.Key == "CLIENTE").Select(d => d.Value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .ToList();

            var situacoes = placemarkData
                .SelectMany(p => p.ExtendedData.Where(d => d.Key == "SITUAÇÃO").Select(d => d.Value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .ToList();

            var bairros = placemarkData
                .SelectMany(p => p.ExtendedData.Where(d => d.Key == "BAIRRO").Select(d => d.Value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .ToList();

            return new Dictionary<string, List<string>>
        {
            { "Clientes", clientes },
            { "Situacoes", situacoes },
            { "Bairros", bairros }
        };
        }
    }
}
