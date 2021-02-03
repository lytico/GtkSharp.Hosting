using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Entity
{
    [Table("Blog")]
    public class Blog
    {
        [Key] public string Id { get; set; }
    }
}