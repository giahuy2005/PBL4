using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;   
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Media.Media3D;
// Cần thêm using System.Windows.Media.Media3D; không liên quan đến Entity, nên xóa nếu không dùng

namespace PBL4.Model.Entities
{
    [Table("users")]
    public class User : BaseModel
    {
        [PrimaryKey("id_user", false)]
        [Column("id_user")]
        public string? IdUser { get; set; }

        [Column("username")]  
        public string? UserName { get; set; }

        [Column("password")]
        public string? Password { get; set; }
        public List<Cameras>? ListCamera { get; set; }
    }
}