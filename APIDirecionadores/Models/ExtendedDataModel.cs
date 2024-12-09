
namespace APIDirecionadores.Models
{
    public class ExtendedDataModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double[] Coordinates { get; set; }
        public List<KeyValuePair<string, string>> ExtendedData { get; set; }
    }
}
