using APIDirecionadores.Models;
using SharpKml.Dom;

namespace APIDirecionadores.Interface
{
    public interface IKmlFileService
    {
        Document GetFile(IFormFile kmlFile);
        Document GetCachedDocument();
        string ExportKmlFile(PlacemarkFilter filter, string filePath);
        List<ExtendedDataModel> FilterPlacemarks(PlacemarkFilter filter);
        List<ExtendedDataModel> ExtractPlacemarkData(Document document);
        object GetFilterOptions();
    }
}
