using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces.Auth;
using AttandanceSyncApp.Repositories.Interfaces.SalaryGarbge;
using System;


namespace AttandanceSyncApp.Repositories.Interfaces
{
    public interface IAuthUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }

        ILoginSessionRepository LoginSessions { get; }

        ISyncCompanyRepository SyncCompanies { get; }

        IToolRepository Tools { get; }

        IEmployeeRepository Employees { get; }

        IUserToolRepository UserTools { get; }
        IAttandanceSyncRequestRepository AttandanceSyncRequests { get; }

        ICompanyRequestRepository CompanyRequests { get; }

        IDatabaseConfigurationRepository DatabaseConfigurations { get; }

        IDatabaseAssignRepository DatabaseAssignments { get; }

        IServerIpRepository ServerIps { get; }

        IDatabaseAccessRepository DatabaseAccess { get; }

        int SaveChanges();
    }

}
