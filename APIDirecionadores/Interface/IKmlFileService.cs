using APIDirecionadores.Models;
using SharpKml.Dom;

namespace APIDirecionadores.Interface
{
    public interface IKmlFileService
    {
        Document OpenKmlFile(IFormFile kmlFile);
        string SaveKmlFile(Document document, string filePath);
        Document GetCachedDocument();
        List<ExtendedDataModel> ExtractPlacemarkData(Document document);
        ExtendedData CreateExtendedData(List<KeyValuePair<string, string>> extendedDataList);
    }
}
