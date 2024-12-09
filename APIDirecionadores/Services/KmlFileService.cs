using APIDirecionadores.Interface;
using APIDirecionadores.Models;
using Microsoft.Extensions.Caching.Memory;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

namespace APIDirecionadores.Services
{
    public class KmlFileService : IKmlFileService
    {
        private readonly IMemoryCache _memoryCache;
        private const string CacheKey = "PlacemarkData";

        public KmlFileService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        public Document GetFile(IFormFile kmlFile)
        {
            if (kmlFile == null || kmlFile.Length == 0)
            {
                throw new ArgumentException("Arquivo KML nulo.");
            }

            try
            {
                using var stream = kmlFile.OpenReadStream();
                var parser = new Parser();
                parser.Parse(stream);

                if (parser.Root is Kml kml && kml.Feature is Document document)
                {
                    _memoryCache.Set(CacheKey, document);
                    return document;
                }
                else
                {
                    throw new InvalidOperationException("Arquivo invalido.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao processar o arquivo KML: {ex.Message}", ex);
            }
        }

        public string ExportKmlFile(PlacemarkFilter filter, string filePath)
        {  
            var filteredPlacemarks = FilterPlacemarks(filter);

            var newDocument = new Document();

            foreach (var placemark in filteredPlacemarks)
            {
                var newPlacemark = new Placemark
                {
                    Name = placemark.Name,    
                    ExtendedData = CreateExtendedData(placemark.ExtendedData)
                };

                newDocument.AddFeature(newPlacemark);
            }

            KmlFile kml = KmlFile.Create(newDocument, false);
            using (var stream = File.Create(filePath))
            {
                kml.Save(stream);
            }

            return $"Arquivo KML criado com sucesso em {filePath}";
        }

        public Document GetCachedDocument()
        {
            if (_memoryCache.TryGetValue(CacheKey, out Document cachedDocument))
            {
                return cachedDocument;
            }

            throw new InvalidOperationException("Nenhum arquivo KML foi carregado.");
        }

        public List<ExtendedDataModel> FilterPlacemarks(PlacemarkFilter filter)
        {
            var document = GetCachedDocument();

            var placemarks = ExtractPlacemarkData(document);

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

        public List<ExtendedDataModel> ExtractPlacemarkData(Document document)
        {
            var placemarks = document.Flatten().OfType<Placemark>();
            var placemarkDataList = new List<ExtendedDataModel>();

            foreach (var placemark in placemarks)
            {
                var dataModel = new ExtendedDataModel
                {
                    Name = placemark.Name,
                    Description = placemark.Description?.Text,
                    Coordinates = placemark.Geometry is Point point
                        ? new[] { point.Coordinate.Longitude, point.Coordinate.Latitude }
                        : null,
                    ExtendedData = placemark.ExtendedData?.Data.Select(d => new KeyValuePair<string, string>(d.Name, d.Value)).ToList()
                };
                placemarkDataList.Add(dataModel);
            }
            return placemarkDataList;
        }

        public object GetFilterOptions()
        {
            try
            {
                var document = GetCachedDocument();
                var placemarkData = ExtractPlacemarkData(document);

                var clientes = placemarkData
                    .SelectMany(p => p.ExtendedData)
                    .Where(d => d.Key.Equals("Cliente", StringComparison.OrdinalIgnoreCase))
                    .Select(d => d.Value)
                    .Distinct()
                    .ToList();

                var situacoes = placemarkData
                    .SelectMany(p => p.ExtendedData)
                    .Where(d => d.Key.Equals("Situação", StringComparison.OrdinalIgnoreCase))
                    .Select(d => d.Value)
                    .Distinct()
                    .ToList();

                var bairros = placemarkData
                    .SelectMany(p => p.ExtendedData)
                    .Where(d => d.Key.Equals("Bairro", StringComparison.OrdinalIgnoreCase))
                    .Select(d => d.Value)
                    .Distinct()
                    .ToList();

                return new
                {
                    Clientes = clientes,
                    Situacoes = situacoes,
                    Bairros = bairros
                };
            }
            catch (Exception ex)
            {               
                throw new Exception("Erro ao obter filtro.", ex);
            }
        }

        private ExtendedData CreateExtendedData(List<KeyValuePair<string, string>> extendedDataList)
        {
            var extendedData = new ExtendedData();
            foreach (var data in extendedDataList)
            {
                extendedData.AddData(new Data
                {
                    Name = data.Key,
                    Value = data.Value
                });
            }
            return extendedData;
        }
    }
}

