using APIDirecionadores.Interface;
using APIDirecionadores.Models;
using SharpKml.Dom;
using System.Text.Json;

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
               (filter.Cliente == null || filter.Cliente.Any(c => p.ExtendedData.Any(e => e.Key == "CLIENTE" && e.Value.Contains(c, StringComparison.OrdinalIgnoreCase)))) &&
               (filter.Situacao == null || filter.Situacao.Any(s => p.ExtendedData.Any(e => e.Key == "SITUAÇÃO" && e.Value.Contains(s, StringComparison.OrdinalIgnoreCase)))) &&
               (filter.Bairro == null || filter.Bairro.Any(b => p.ExtendedData.Any(e => e.Key == "BAIRRO" && e.Value.Contains(b, StringComparison.OrdinalIgnoreCase)))) &&
               (string.IsNullOrEmpty(filter.Referencia) || p.ExtendedData.Any(e => e.Key == "REFERENCIA" && e.Value.Contains(filter.Referencia, StringComparison.OrdinalIgnoreCase))) &&
               (string.IsNullOrEmpty(filter.RuaOuCruzamento) || p.ExtendedData.Any(e => e.Key == "RUA" && e.Value.Contains(filter.RuaOuCruzamento, StringComparison.OrdinalIgnoreCase)) ||
                p.ExtendedData.Any(e => e.Key == "CRUZAMENTO" && e.Value.Contains(filter.RuaOuCruzamento, StringComparison.OrdinalIgnoreCase)))
            ).ToList();

            if (!filteredPlacemarks.Any())
            {
                throw new ArgumentException("Nenhum resultado encontrado com os parâmetros fornecidos.");
            }
            return filteredPlacemarks;
        }

        public List<ExtendedDataModel> FilterPlacemarksCSB(PlacemarkDTO filter)
        {
            var document = _kmlFileService.GetCachedDocument();

            var placemarks = _kmlFileService.ExtractPlacemarkData(document);

            var filteredPlacemarks = placemarks.Where(p =>
               (filter.Cliente == null || filter.Cliente.Any(c => p.ExtendedData.Any(e => e.Key == "CLIENTE" && e.Value.Contains(c, StringComparison.OrdinalIgnoreCase)))) &&
               (filter.Situacao == null || filter.Situacao.Any(s => p.ExtendedData.Any(e => e.Key == "SITUAÇÃO" && e.Value.Contains(s, StringComparison.OrdinalIgnoreCase)))) &&
               (filter.Bairro == null || filter.Bairro.Any(b => p.ExtendedData.Any(e => e.Key == "BAIRRO" && e.Value.Contains(b, StringComparison.OrdinalIgnoreCase))))).ToList();

            return filteredPlacemarks;
        }
        public Document CreateFilteredKmlDocument(PlacemarkModel filter)
        {
            var filteredPlacemarks = FilterPlacemarks(filter);

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
    }
}
