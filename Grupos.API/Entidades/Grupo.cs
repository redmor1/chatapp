using System.ComponentModel.DataAnnotations;

namespace Grupos.API.Entidades
{
    public class Grupo
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        public ICollection<MiembroGrupo> Miembros { get; set; } = new List<MiembroGrupo>();

    }
}
