using APIDirecionadores.Models;
using SharpKml.Dom;

namespace APIDirecionadores.Interface
{
    public interface IPlacermarkFilterService
    {
        List<ExtendedDataModel> FilterPlacemarks(PlacemarkModel filter);
        Document CreateFilteredKmlDocument(PlacemarkModel filter);
        Dictionary<string, List<string>> GetListFilter();
    }
}
