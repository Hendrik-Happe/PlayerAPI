using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlayerAPI.Models
{
    [Table("Manager_playlist")]
    public class PlayList
    {
        [Column("id")]
        [Key]
        public int ID { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("tag")]
        public string Tag { get; set; }

        public virtual List<File> Files { get; set; }
    }
}
