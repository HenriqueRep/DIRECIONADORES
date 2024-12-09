using APIDirecionadores.Models;
using SharpKml.Dom;

namespace APIDirecionadores.Interface
{
    public interface IPlacermarkFilterService
    {
        List<ExtendedDataModel> FilterPlacemarks(PlacemarkModel filter);
        List<ExtendedDataModel> FilterPlacemarksCSB(PlacemarkDTO filter);
        Document CreateFilteredKmlDocument(PlacemarkModel filter);
     


    }
}
