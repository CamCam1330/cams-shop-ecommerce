using SV22T1020670.DomainModels;

namespace SV22T1020670.DomainModels.Models
{
    /// <summary>
    /// ViewModel dùng cho chức năng bổ sung/cập nhật Employee
    /// </summary>
    public class EmployeeEditModel : Employee
    {
        public IFormFile? UploadPhoto { get; set; }
    }
}
