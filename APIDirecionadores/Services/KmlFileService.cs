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
        public Document OpenKmlFile(IFormFile kmlFile)
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

        public string SaveKmlFile(Document document, string filePath)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document), "O documento KML não pode ser nulo.");
               
            KmlFile kml = KmlFile.Create(document, false);
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
        public ExtendedData CreateExtendedData(List<KeyValuePair<string, string>> extendedDataList)
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

