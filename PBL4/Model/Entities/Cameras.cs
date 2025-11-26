using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PBL4.Model.Entities
{
    // Bắt buộc phải kế thừa BaseModel và ánh xạ tới tên bảng DB
    [Table("Cameras")]
    public class Cameras : BaseModel // <--- PHẢI KẾ THỪA BASEMODEL
    {
        // Khóa chính
        [PrimaryKey("id_camera", false)]
        public string? IdCamera { get; set; }

        [Column("id_user")]
        public string? IdUser { get; set; }
        [Column("namecamera")]
        public string? NameCamera { get; set; }
        [Column("url")]
        public string? URL { get; set; }
        [Column("nameuser")]
        public string? NameUser { get; set; }
        [Column("password")]
        public string? Password { get; set; }

        public User? User { get; set; }

        public Cameras() { }
    }
}