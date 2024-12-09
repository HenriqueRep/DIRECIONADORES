using Microsoft.AspNetCore.Mvc;

namespace APIDirecionadores.Models
{
    public class PlacemarkDTO
    {
        [FromQuery]
        public List<string>? Cliente { get; set; }

        [FromQuery]
        public List<string>? Situacao { get; set; }

        [FromQuery]
        public List<string>? Bairro { get; set; }
    }
}
