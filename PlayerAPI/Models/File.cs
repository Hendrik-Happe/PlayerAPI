using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlayerAPI.Models
{
    [Table("Manager_file")]
    public class File
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("playList_id")]
        public int PlayListId { get; set; }


        //public virtual PlayList Playlist { get; set; }

        [Column("file")]
        public string Filename { get; set; }

        [Column("number")]
        public int TrackNumber { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}
