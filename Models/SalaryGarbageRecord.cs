using System;

namespace AttandanceSyncApp.Models.SalaryGarbge
{
    public class SalaryGarbageRecord
    {
        public int Id { get; set; }

        public string ServerIP { get; set; }

        public string DatabaseName { get; set; }

        public int EmployeeId { get; set; }

        public string EmployeeCode { get; set; }

        public string EmployeeName { get; set; }

        public string Problem { get; set; }

        public bool IsSolve { get; set; }

        public DateTime EntryDateTime { get; set; }
    }
}