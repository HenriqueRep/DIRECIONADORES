using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace APIDirecionadores.Models
{
    public class PlacemarkFilter
    {
        [FromQuery]
        public string? Cliente { get; set; }

        [FromQuery]
        public string? Situacao { get; set; }

        [FromQuery]
        public string? Bairro { get; set; }

        [FromQuery]
        [MinLength(3, ErrorMessage = "O campo 'Referencia' deve ter pelo menos 3 caracteres.")]
        public string? Referencia { get; set; }

        [FromQuery]
        [MinLength(3, ErrorMessage = "O campo 'Rua ou Cruzamento' deve ter pelo menos 3 caracteres.")]
        public string? RuaOuCruzamento { get; set; }

    }
}
