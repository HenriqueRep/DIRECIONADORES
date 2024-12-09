using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace APIDirecionadores.Models
{
    public class PlacemarkModel
    {
        [FromQuery]
        public List<string>? Cliente { get; set; }

        [FromQuery]
        public List<string>? Situacao { get; set; }

        [FromQuery]
        public List<string>? Bairro { get; set; }

        [FromQuery]
        [MinLength(3, ErrorMessage = "O campo 'Referencia' deve ter pelo menos 3 caracteres.")]
        public string? Referencia { get; set; }

        [FromQuery]
        [MinLength(3, ErrorMessage = "O campo 'Rua ou Cruzamento' deve ter pelo menos 3 caracteres.")]
        public string? RuaOuCruzamento { get; set; }

    }
}
