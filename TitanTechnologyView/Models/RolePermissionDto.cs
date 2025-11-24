namespace TitanTechnologyView.Models
{
    public class RolePermissionDto
    {
        public int Id { get; set; }

        public int RoleId { get; set; }
        public List<int> PermissionIds { get; set; }
    }
}
