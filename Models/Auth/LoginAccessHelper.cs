using AttandanceSyncApp.Models;
using AttendanceSyncApp.Models;
using System.Linq;

namespace AttendanceSyncApp.Controllers.Auth
{
    public static class LoginAccessHelper
    {
        public static bool IsUserApproved(string email)
        {
            using (var db = new AppDbContext())
            {
                var approval = db.UserApprovals
                    .FirstOrDefault(x => x.EmployeeEmail == email);

                if (approval == null)
                    return false;

                return approval.ApprovalStatus == "Approved";
            }
        }
    }
}
